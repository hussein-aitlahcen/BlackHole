using System.Collections.ObjectModel;

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
        public ObservableCollection<T> Items { get; set; }
        
        /// <summary>
        /// 
        /// </summary>
        public ViewModelCollection()
        {
            Items = new ObservableCollection<T>();
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="item"></param>
        public void AddItem(T item) => Items.Add(item);

        /// <summary>
        /// 
        /// </summary>
        public void Clear() => Items.Clear();//.ToList().ForEach(RemoveItem);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="item"></param>
        public void RemoveItem(T item) => Items.Remove(item);
    }
}
