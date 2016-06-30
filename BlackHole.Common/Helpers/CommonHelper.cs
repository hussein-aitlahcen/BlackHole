using System.IO;
using BlackHole.Common.Network.Protocol;

namespace BlackHole.Common.Helpers
{
    public static class CommonHelper
    {
        /// <summary>
        /// Chunck to download
        /// </summary>
        public const int FilePartSize = 64 * 1000;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="currentPart"></param>
        /// <param name="path"></param>
        /// <returns></returns>
        public static DownloadedFilePartMessage DownloadFilePart(long id, long currentPart, string path)
        {
            path = Path.GetFullPath(path);

            using (var stream = new FileStream(path, FileMode.Open))
            {
                var totalPart = stream.Length / FilePartSize;

                var partSize = currentPart != totalPart ? FilePartSize : stream.Length - FilePartSize * currentPart;
                // read only one chunck
                var output = new byte[partSize];
                stream.Seek(FilePartSize * currentPart, SeekOrigin.Begin);
                stream.Read(output, 0, (int)partSize);

                return new DownloadedFilePartMessage
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
        /// <param name="directory"></param>
        /// <param name="fileName"></param>
        /// <param name="currentPart"></param>
        /// <param name="rawData"></param>
        public static void WriteDownloadedPart(string directory, string fileName, long currentPart, byte[] rawData)
        {
            if (!Directory.Exists(directory))
                Directory.CreateDirectory(directory);

            var path = Path.Combine(directory, Path.GetFileName(fileName));
            //var existing = File.Exists(path);

            // output the part to local file
            using(var stream = new FileStream(path, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite))
            {
                stream.Seek(FilePartSize * currentPart, SeekOrigin.Begin);
                stream.Write(rawData, 0, rawData.Length);
            }
        }
    }
}
