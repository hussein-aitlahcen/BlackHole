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
        /// <summary>
        /// 
        /// </summary>
        public void Initialize()
        {
            using (var ctx = NetMQContext.Create())
            {
                using (var server = ctx.CreateRouterSocket())
                {
                    server.Bind("tcp://127.0.0.1:5556");
                    server.ReceiveReady += Server_ReceiveReady;
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Server_ReceiveReady(object sender, NetMQSocketEventArgs e)
        {
            var frames = e.Socket.ReceiveMultipartMessage();
            var message = NetMessage.Deserialize(frames.First.Buffer);
            message.Match()
                .With<GreetTheMaster>(GreetTheMaster);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        private void GreetTheMaster(GreetTheMaster message)
        {

        }
    }
}
