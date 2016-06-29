using System.Collections.Generic;
using BlackHole.Common;

namespace BlackHole.Slave
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class NetworkService : Singleton<NetworkService>
    {
        /// <summary>
        /// /
        /// </summary>
        private readonly List<MasterServer> m_serverConnections;

        /// <summary>
        /// 
        /// </summary>
        public NetworkService()
        {
            m_serverConnections = new List<MasterServer>();
        }

        /// <summary>
        /// 
        /// </summary>
        public void Initialize()
        {
            CreateConnection("tcp://127.0.0.1:5556");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="address"></param>
        private void CreateConnection(string address) => m_serverConnections.Add(new MasterServer(address));

        /// <summary>
        /// 
        /// </summary>
        /// <param name="reason"></param>
        public void FireShutdown(int reason) => m_serverConnections.ForEach(connection => connection.FireShutdown(reason));
    }
}
