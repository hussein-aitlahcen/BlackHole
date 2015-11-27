using BlackHole.Common.Network.Protocol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace BlackHole.Master
{
    public static class WindowExtensions
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="window"></param>
        /// <param name="message"></param>
        public static void Send(this ISlaveWindow window, NetMessage message) => window.Slave.Send(message);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="action"></param>
        public static async void WrapEvent<T>(this Window window, T obj, Action<T> action)
        {
            await ExecuteInDispatcher(window, () => action(obj));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="action"></param>
        public static async Task ExecuteInDispatcher(this Window window, Action action)
        {
            await window.Dispatcher.InvokeAsync(action);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dispatcher"></param>
        /// <param name="action"></param>
        public static async void DelayInvoke(this Dispatcher dispatcher, TimeSpan delay, Action action) =>
            await dispatcher.DelayInvoke(delay, action, new CancellationToken(false));

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dispatcher"></param>
        /// <param name="action"></param>
        public static async Task DelayInvoke(this Dispatcher dispatcher, TimeSpan delay, Action action, CancellationToken cancellationToken)
        {
            await Task.Delay(delay);
            await dispatcher.InvokeAsync(action, DispatcherPriority.Background, cancellationToken);
        }
    }
}
