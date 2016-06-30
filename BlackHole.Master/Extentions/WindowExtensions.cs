using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace BlackHole.Master.Extentions
{
    public static class WindowExtensions
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="window"></param>
        /// <param name="obj"></param>
        /// <param name="action"></param>
        public static async void WrapEvent<T>(this Window window, T obj, Action<T> action) =>
            await ExecuteInDispatcher(window, () => action(obj));
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="window"></param>
        /// <param name="action"></param>
        public static async Task ExecuteInDispatcher(this Window window, Action action) =>
           await window.Dispatcher.InvokeAsync(action, DispatcherPriority.Background);
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="dispatcher"></param>
        /// <param name="delay"></param>
        /// <param name="action"></param>
        public static async void DelayInvoke(this Dispatcher dispatcher, TimeSpan delay, Action action) =>
            await dispatcher.DelayInvoke(delay, action, new CancellationToken(false));

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dispatcher"></param>
        /// <param name="delay"></param>
        /// <param name="action"></param>
        /// <param name="cancellationToken"></param>
        public static async Task DelayInvoke(this Dispatcher dispatcher, TimeSpan delay, Action action, 
            CancellationToken cancellationToken)
        {
            await Task.Delay(delay, cancellationToken);
            await dispatcher.InvokeAsync(action, DispatcherPriority.Background, cancellationToken);
        }
    }
}
