using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace BlackHole.Master
{
    /// <summary>
    /// Logique d'interaction pour App.xaml
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
            Startup += new StartupEventHandler(App_Startup);
            AppDomain.CurrentDomain.UnhandledException += (sender, e) =>
            {
            };
        }

        void App_Startup(object sender, StartupEventArgs e)
        {
            Master.MainWindow.Instance.Show();
        }
    }
}
