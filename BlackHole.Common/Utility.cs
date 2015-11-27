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
    }
}
