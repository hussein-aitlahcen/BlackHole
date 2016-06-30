using System;
using System.Net;

namespace BlackHole.Common
{
    /// <summary>
    /// 
    /// </summary>
    public static class Utility
    {
        static readonly string IPIFY = "https://api.ipify.org/";

        /// <summary>
        /// Wrapper method to simplify such operation.
        /// First, it try to retrieve the object from the "operation" function.
        /// If it suceed, the returned object is passed to the "onSuccess" function, else, the Exception is given to the "onError" function.
        /// 
        /// </summary>
        /// <param name="operation"></param>
        /// <param name="onSuccess"></param>
        /// <param name="onError"></param>
        public static void ExecuteComplexOperation<T>(Func<T> operation, Action<T> onSuccess, Action<Exception> onError)
        {
            try
            {
                onSuccess(operation());
            }
            catch (Exception e)
            {
                onError(e);
            }
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static string GetWanIp()
        {
            try
            {
                using (var client = new WebClient())
                    return client.DownloadString(IPIFY);
            }
            catch
            {
                return "?.?.?.?";
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="length"></param>
        public static string FormatFileSize(double length)
        {
            int order = 0;
            string[] sizes = { "B", "KB", "MB", "GB" };
            while (length >= 1024 && order + 1 < sizes.Length)
            {
                order++;
                length = length / 1024;
            }
            return $"{length:0.##} {sizes[order]}";
        }
    }
}
