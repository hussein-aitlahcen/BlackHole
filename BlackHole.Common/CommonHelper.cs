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
        public static DownloadedFilePartMessage DownloadFilePart(int id, long currentPart, string path)
        {
            path = Path.GetFullPath(path);

            using (var stream = new FileStream(path, FileMode.Open))
            {
                var totalPart = stream.Length / FILE_PART_SIZE;

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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="part"></param>
        public static void WriteDownloadedPart(DownloadedFilePartMessage part)
        {
            var existing = File.Exists(part.Path);
            // output the part to local file
            using(var stream = new FileStream(part.Path, FileMode.OpenOrCreate))
            {
                stream.Seek(FILE_PART_SIZE * part.CurrentPart, SeekOrigin.Begin);
                stream.Write(part.RawPart, 0, FILE_PART_SIZE);
            }
        }
    }
}
