using BlackHole.Common.Network.Protocol;
using BlackHole.Master.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media;

namespace BlackHole.Master.Remote
{
    public interface IRemoteCommand
    {
        long Id { get; }
        string Name { get; }
        Slave Slave { get; }
        long Progress { get; set; }
        Brush ProgressColor { get; set; }
        bool Completed { get; set; }

        void DoExecute();
        void DoFault();
        void UpdateProgression(long current, long maximum);
    }

    public sealed class RemoteCommand<TIn> : CommandModel, IRemoteCommand
    {
        public string Name { get; }
        private Action<RemoteCommand<TIn>> m_executeAction;
        private Action m_faultedAction;
        private Action<TIn> m_continueAction, m_completedAction;

        public RemoteCommand(long id, string name, Slave slave, string headerText, string targetText, Action<RemoteCommand<TIn>> onExecute, Action<TIn> onContinue, Action<TIn> onCompleted, Action onFaulted)
            : base(id, slave, headerText, targetText)
        {
            Name = name;
            m_executeAction = WrapAction(SlaveEventType.COMMAND_EXECUTED, onExecute);
            m_continueAction = WrapAction(SlaveEventType.COMMAND_CONTINUE, onContinue);
            m_completedAction = WrapAction(SlaveEventType.COMMAND_COMPLETED, onCompleted);
            m_faultedAction = WrapAction(SlaveEventType.COMMAND_FAULTED, onFaulted);
        }

        private Action WrapAction(SlaveEventType type, Action action)
            => () =>
            {
                action();
                Slave.SlaveEvents.PostEvent(new SlaveEvent(type, Slave, this));
            };

        private Action<T> WrapAction<T>(SlaveEventType type, Action<T> action)
            => (input) =>
            {
                action(input);
                Slave.SlaveEvents.PostEvent(new SlaveEvent(type, Slave, this));
            };

        public void DoExecute() => m_executeAction(this);
        public void DoContinue(TIn input) => m_continueAction(input);
        public void DoComplete(TIn input) => m_completedAction(input);
        public void DoFault() => m_faultedAction();
    }
}
