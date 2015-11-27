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
    public sealed class FileDownload : FileTransaction
    {
        /// <summary>
        /// 
        /// </summary>
        public const int BUFFER_INITIAL_CAPACITY = 1024;
        
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
        /// <param name="id"></param>
        public FileDownload() : base(TransactionType.DOWNLOAD)
        {            
            RawFile = new List<byte>(BUFFER_INITIAL_CAPACITY);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="part"></param>
        protected override void InternalUpdate(DownloadedFilePartMessage downloadedPart) 
            => RawFile.AddRange(downloadedPart.RawPart);
    }
}
