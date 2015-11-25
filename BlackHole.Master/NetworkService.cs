using BlackHole.Common;
using BlackHole.Common.Network.Protocol;
using NetMQ;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlackHole.Master
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class NetworkService : Singleton<NetworkService>
    {
        public delegate void SlaveFunction(Slave slave);

        public event SlaveFunction OnSlaveConnected;
        public event SlaveFunction OnSlaveDisconnected;

        private NetMQContext m_netContext;
        private NetMQSocket m_server;
        private Poller m_poller;
        private Dictionary<int, Slave> m_slaveById;

        /// <summary>
        /// 
        /// </summary>
        public NetworkService()
        {
            m_slaveById = new Dictionary<int, Slave>();
        }

        /// <summary>
        /// 
        /// </summary>
        public void Start()
        {
            m_netContext = NetMQContext.Create();
            m_server = m_netContext.CreateRouterSocket();

            m_server.Bind("tcp://*:5556");
            m_server.ReceiveReady += Server_ReceiveReady;

            m_poller = new Poller();
            m_poller.AddSocket(m_server);
            m_poller.PollTillCancelledNonBlocking();
        }

        /// <summary>
        /// 
        /// </summary>
        public void Stop()
        {
            m_poller.Dispose();
            m_server.Dispose();
            m_netContext.Dispose();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="slave"></param>
        private void FireSlaveConnected(Slave slave)
        {
            if (OnSlaveConnected != null)
                OnSlaveConnected(slave);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="slave"></param>
        private void FireSlaveDisconnected(Slave slave)
        {
            if (OnSlaveDisconnected != null)
                OnSlaveDisconnected(slave);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Server_ReceiveReady(object sender, NetMQSocketEventArgs e)
        { 
            var frames = e.Socket.ReceiveMultipartMessage();
            var clientId = BitConverter.ToInt32(frames.First.Buffer, 1);
            var message = NetMessage.Deserialize(frames.Last.Buffer);
            
            Slave slave = null;
            if (!m_slaveById.ContainsKey(clientId))            
                m_slaveById.Add(clientId, slave = new Slave(clientId));            
            else
                slave = m_slaveById[clientId];

            Match<GreetTheMaster>(slave, message, GreetTheMaster);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        private void Match<T>(Slave slave, NetMessage message, Action<Slave, T> fun) where T : NetMessage
        {
            message
                .Match()
                .With<T>(m => fun(slave, m));
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        private void GreetTheMaster(Slave slave, GreetTheMaster message)
        {
            slave.Initialize("", message.OperatingSystem, message.MachineName, message.UserName);

            FireSlaveConnected(slave);
        }
    }
}
