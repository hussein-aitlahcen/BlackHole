using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlackHole.Master
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class ViewModel<T>
    {
        /// <summary>
        /// 
        /// </summary>
        public ObservableCollection<T> Items
        {
            get;
            set;
        }

        /// <summary>
        /// 
        /// </summary>
        public ViewModel()
        {
            Items = new ObservableCollection<T>();
        }
    }
}
