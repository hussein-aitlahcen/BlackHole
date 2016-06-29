using System.IO;
using System.Windows.Forms;

namespace BlackHole.Slave.Helper
{
    public static class MaliciousHelper
    {
        public static string ApplicationPath => Application.ExecutablePath;
        public static string ExecutableName => Path.GetFileName(ApplicationPath);
        public static string StartupKey = "WindowUpdate";
    }
}
