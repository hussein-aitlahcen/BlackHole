using ProtoBuf;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlackHole.Common.Network.Protocol
{
    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    public sealed class GreetTheMaster : NetMessage
    {
        public string UserName;
        public string MachineName;
        public string OperatingSystem;
    }

    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    public sealed class DoYourDuty : NetMessage
    {
    }

    [ProtoInclude(1001, typeof(DoYourDuty))]
    [ProtoInclude(1000, typeof(GreetTheMaster))]
    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    public abstract class NetMessage
    {
        private byte[] m_serializedBuffer;

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public byte[] Serialize()
        {
            if (m_serializedBuffer == null)
            {
                using (var stream = new MemoryStream())
                {
                    using (var lzStream = new Lz4Net.Lz4CompressionStream(stream, Lz4Net.Lz4Mode.Fast))
                        Serializer.Serialize(lzStream, this);
                    m_serializedBuffer = stream.ToArray();
                }
            }
            return m_serializedBuffer;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static NetMessage Deserialize(byte[] data)
        {
            using (var stream = new Lz4Net.Lz4DecompressionStream(new MemoryStream(data)))            
                return Serializer.Deserialize<NetMessage>(stream);            
        }
    }
}
