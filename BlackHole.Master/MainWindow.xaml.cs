using BlackHole.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace BlackHole.Master
{
    /// <summary>
    /// Logique d'interaction pour MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        /// <summary>
        /// 
        /// </summary>
        public static MainWindow Instance
        {
            get;
            private set;
        }

        /// <summary>
        /// 
        /// </summary>
        private List<Window> m_childWindows;

        /// <summary>
        /// 
        /// </summary>
        static MainWindow()
        {
            Instance = new MainWindow();
        }
        
        /// <summary>
        /// 
        /// </summary>
        private MainWindow()
        {
            m_childWindows = new List<Window>();

            InitializeComponent();     
            Initialize();

            Closing += MainWindow_Closing;
        }
                
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            NetworkService.Instance.Stop();
            m_childWindows.ForEach(async (window) => await window.ExecuteInDispatcher(() => window.Close()));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OpenFileManager(object sender, RoutedEventArgs e)
        {
            if (SlavesList.SelectedItem == null)
                return;

            RegisterOrOpenChildWindow(new FileManager((Slave)SlavesList.SelectedItem));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="slaveId"></param>
        private List<Window> FindSlaveWindows(int slaveId)
        {
            var windows = new List<Window>();
            foreach (var window in m_childWindows.OfType<ISlaveWindow>())
                if (window.Slave.Id == slaveId)
                    windows.Add((Window)window);
            return windows;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="slave"></param>
        private void CloseSlaveWindows(int slaveId)
           => FindSlaveWindows(slaveId).ForEach(async window => await window.ExecuteInDispatcher(() => window.Close()));

        /// <summary>
        /// 
        /// </summary>
        /// <param name="window"></param>
        private async void RegisterOrOpenChildWindow<T>(T window) where T : Window, ISlaveWindow
        {
            // focus the existing window
            var existingWindow = m_childWindows
                .OfType<T>()
                .FirstOrDefault(w => (w.Slave.Id == window.Slave.Id));
            if (existingWindow != null)
            {
                await existingWindow.ExecuteInDispatcher(() => existingWindow.Focus());
                return;
            }

            // hook the closing so we remove 
            window.Closed += async (s, args) =>
            {
                await this.ExecuteInDispatcher(() =>
                {
                    Slave.SlaveEvents.Unsubscribe(window);
                    m_childWindows.Remove(window);
                });
            };

            // register the slave window to the events of the slave
            Slave.SlaveEvents.Subscribe((ev) => ev.Source.Id == window.Slave.Id, window);
                      
            m_childWindows.Add(window);

            // finally, open up the window
            window.Show();
        }
    }
}
