using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlackHole.Master.Model
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class SlaveMonitorModel : ViewModel
    {
        public string ListeningState
        {
            get;
            private set;
        }

        public string OnlineSlaves
        {
            get;
            private set;
        }

        public SlaveMonitorModel()
        {
            SetListeningState("Not bound");
            SetOnlineSlaves(0);
        }

        public void SetListeningState(string state)
        {
            ListeningState = $"State : {state}";
            NotifyPropertyChange("ListeningState");
        }
        
        public void SetOnlineSlaves(int count)
        {
            OnlineSlaves = $"Online slaves : {count}";
            NotifyPropertyChange("OnlineSlaves");
        }
    }
}
