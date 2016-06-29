using System;
using System.Linq;
using System.Windows;

namespace BlackHole.Master
{
    /// <summary>
    /// Logique d'interaction pour App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static ResourceDictionary StaticResources
        {
            get;
            private set;
        }

        public App()
        {
            Startup += new StartupEventHandler(App_Startup);
            AppDomain.CurrentDomain.UnhandledException += (sender, e) =>
            {
            };
        }

        void App_Startup(object sender, StartupEventArgs e)
        {
            StaticResources = Current.Resources.MergedDictionaries.First();
            Master.MainWindow.Instance.Show();
        }
    }
}
