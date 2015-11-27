using BlackHole.Common;
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
    public interface ISlaveWindow : IEventListener<SlaveEvent, Slave>
    {
        Slave Slave { get; }
    }
}
