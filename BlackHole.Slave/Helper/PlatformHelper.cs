using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace BlackHole.Slave.Helper
{
    class PlatformHelper
    {
        static PlatformHelper()
        {
            Win32NT = Environment.OSVersion.Platform == PlatformID.Win32NT;
            XpOrHigher = Win32NT && Environment.OSVersion.Version.Major >= 5;
            VistaOrHigher = Win32NT && Environment.OSVersion.Version.Major >= 6;
            SevenOrHigher = Win32NT && (Environment.OSVersion.Version >= new Version(6, 1));
            EightOrHigher = Win32NT && (Environment.OSVersion.Version >= new Version(6, 2, 9200));
            EightPointOneOrHigher = Win32NT && (Environment.OSVersion.Version >= new Version(6, 3));
            TenOrHigher = Win32NT && (Environment.OSVersion.Version >= new Version(10, 0));
            RunningOnMono = Type.GetType("Mono.Runtime") != null;

            Name = "Unknown OS";
            using (var searcher = new ManagementObjectSearcher("SELECT Caption FROM Win32_OperatingSystem"))
            {
                foreach (var os in searcher.Get())
                {
                    Name = os["Caption"].ToString();
                    break;
                }
            }

            Name = Regex.Replace(Name, "^.*(?=Windows)", "").TrimEnd().TrimStart(); // Remove everything before first match "Windows" and trim end & start
            Is64Bit = Environment.Is64BitOperatingSystem;
            FullName = string.Format("{0} {1} Bit", Name, Is64Bit ? 64 : 32);
        }
        
        public static string FullName { get; private set; }
        public static string Name { get; private set; }
        public static bool Is64Bit { get; private set; }
        public static bool RunningOnMono { get; private set; }
        public static bool Win32NT { get; private set; }
        public static bool XpOrHigher { get; private set; }
        public static bool VistaOrHigher { get; private set; }
        public static bool SevenOrHigher { get; private set; }
        public static bool EightOrHigher { get; private set; }        
        public static bool EightPointOneOrHigher { get; private set; }        
        public static bool TenOrHigher { get; private set; }
    }
}
