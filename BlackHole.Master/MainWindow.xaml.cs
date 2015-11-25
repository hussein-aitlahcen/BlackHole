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
        public static MainWindow Instance
        {
            get; private set;
        }

        static MainWindow()
        {
            Instance = new MainWindow();
        }

        private MainWindow()
        {
            InitializeComponent();

            Closing += MainWindow_Closing;

            NetworkService.Instance.Start();
            NetworkService.Instance.OnSlaveConnected += OnSlaveConnected;
        }

        private void OnSlaveConnected(Slave slave)
        {
            Dispatcher.InvokeAsync(() =>
            {
                SlavesList.Items.Add(slave);
            });
        }

        private void OnSlaveDisconnected(Slave slave)
        {
            SlavesList.Items.Remove(slave);
        }
        
        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            NetworkService.Instance.Stop();
        }
    }
}
