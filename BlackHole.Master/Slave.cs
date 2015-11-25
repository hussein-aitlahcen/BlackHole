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
    public sealed class Slave
    {
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
        /// <param name="id"></param>
        public Slave(int id)
        {
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
    }
}
