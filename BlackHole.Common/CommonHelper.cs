using BlackHole.Common.Network.Protocol;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlackHole.Common
{
    public static class CommonHelper
    {
        /// <summary>
        /// Chunck to download
        /// </summary>
        public const int FILE_PART_SIZE = 64 * 1000;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static DownloadedFilePartMessage DownloadFilePart(int id, int currentPart, string path)
        {
            path = Path.GetFullPath(path);

            using (var stream = new FileStream(path, FileMode.Open))
            {
                int totalPart = (int)stream.Length / FILE_PART_SIZE;

                var partSize = currentPart != totalPart ? FILE_PART_SIZE : stream.Length - (FILE_PART_SIZE * currentPart);
                // read only one chunck
                var output = new byte[partSize];
                stream.Seek(FILE_PART_SIZE * currentPart, SeekOrigin.Begin);
                stream.Read(output, 0, (int)partSize);

                return new DownloadedFilePartMessage()
                {
                    Id = id,
                    CurrentPart = currentPart,
                    Path = path,
                    TotalPart = totalPart,
                    RawPart = output
                };
            }
        }
    }
}
