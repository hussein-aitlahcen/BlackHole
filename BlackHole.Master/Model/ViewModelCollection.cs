using BlackHole.Master.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Animation;

namespace BlackHole.Master.Model
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class ViewModelCollection<T>
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
        public ViewModelCollection()
        {
            Items = new ObservableCollection<T>();
        }
    }
}
