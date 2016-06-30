using System.IO;

namespace BlackHole.Common.Helpers
{
    /// <summary>
    /// 
    /// </summary>
    public static class FileHelper
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="directory"></param>
        /// <param name="fileName"></param>
        /// <param name="currentPart"></param>
        /// <param name="rawData"></param>
        public static void WriteDownloadedPart(string directory, string fileName, long currentPart, byte[] rawData) =>
            CommonHelper.WriteDownloadedPart(directory, fileName, currentPart, rawData);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="directory"></param>
        /// <param name="name"></param>
        /// <param name="raw"></param>
        public static void SaveFile(string directory, string name, byte[] raw)
        {
            if (!Directory.Exists(directory))
                Directory.CreateDirectory(directory);
            File.WriteAllBytes(Path.Combine("./", Path.Combine(directory, name)), raw);
        }
    }
}
