using BlackHole.Common;
using NetMQ;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlackHole.Slave
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class NetworkService : Singleton<NetworkService>
    {
        /// <summary>
        /// 
        /// </summary>
        public void Initialize()
        {
            var netContext = NetMQContext.Create();
            var master = new MasterServer(netContext, "tcp://127.0.0.1:5556");
        }
    }
}
