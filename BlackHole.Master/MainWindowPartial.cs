using BlackHole.Common;
using BlackHole.Common.Network.Protocol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
        public ViewModel<Slave> ViewModelSlaves
        {
            get;
            private set;
        }

        /// <summary>
        /// 
        /// </summary>
        private void Initialize()
        {
            SlavesList.DataContext = ViewModelSlaves = new ViewModel<Slave>();

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
                        AddInfoMessage($"disconnected slave={ev.Source.ToString()}");
                        break;
                    case SlaveEventType.INCOMMING_MESSAGE:
                        if(!(ev.Data is PongMessage))
                        {
                            AddInfoMessage($"received slave={ev.Source.UserName} message={ev.Data.GetType().Name}");
                        }
                        break;
                }
            });
        }
    }
}
