using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace BlackHole.Common
{
    /// <summary>
    /// 
    /// </summary>
    public static class Utility
    {
        static string IPIFY = "https://api.ipify.org/";

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="fun"></param>
        /// <param name="success"></param>
        /// <param name="error"></param>
        public static void ExecuteComplexOperation<T>(string name, Func<T> operation, Action<T> success, Action<Exception> error)
        {
            try
            {
                success(operation());
            }
            catch (Exception e)
            {
                error(e);
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
            catch(Exception e)
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
            return string.Format("{0:0.##} {1}", length, sizes[order]);
        }
    }
}
