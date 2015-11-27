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
        /// <param name="window"></param>
        private async void RegisterOrOpenChildWindow(Window window)
        {
            // we dont need to continue if the window isnt a slave window
            var slaveWindow = window as ISlaveWindow;
            if (slaveWindow == null)
                return;

            // focus the existing window
            var existingWindow = m_childWindows
                .OfType<ISlaveWindow>()
                .FirstOrDefault(w => (w.SlaveId == slaveWindow.SlaveId) && (w.GetType() == slaveWindow.GetType()));
            if (existingWindow != null)
            {
                var w = existingWindow as Window;
                await w.ExecuteInDispatcher(() => w.Focus());
                return;
            }

            // hook the closing so we remove 
            window.Closed += async (s, args) =>
            {
                await this.ExecuteInDispatcher(() =>
                {
                    Slave.SlaveEvents.Unsubscribe(slaveWindow);
                    m_childWindows.Remove(window);
                });
            };

            // register the slave window to the events of the slave
            Slave.SlaveEvents.Subscribe((ev) => ev.Source.Id == slaveWindow.SlaveId, slaveWindow);
                      
            m_childWindows.Add(window);

            // finally, open up the window
            window.Show();
        }
    }
}
