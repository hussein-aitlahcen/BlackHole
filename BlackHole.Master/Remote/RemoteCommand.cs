using System;
using System.Windows.Media;
using BlackHole.Master.Model;

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
        private readonly Action<RemoteCommand<TIn>> m_executeAction;
        private readonly Action m_faultedAction;
        private readonly Action<TIn> m_continueAction;
        private readonly Action<TIn> m_completedAction;

        public RemoteCommand(long id, string name, Slave slave, string headerText, string targetText, 
            Action<RemoteCommand<TIn>> onExecute, Action<TIn> onContinue, Action<TIn> onCompleted, Action onFaulted)
            : base(id, slave, headerText, targetText)
        {
            Name = name;
            m_executeAction = WrapAction(SlaveEventType.CommandExecuted, onExecute);
            m_continueAction = WrapAction(SlaveEventType.CommandContinue, onContinue);
            m_completedAction = WrapAction(SlaveEventType.CommandCompleted, onCompleted);
            m_faultedAction = WrapAction(SlaveEventType.CommandFaulted, onFaulted);
        }

        private Action WrapAction(SlaveEventType type, Action action) => () =>
        {
            action();
            Slave.SlaveEvents.PostEvent(new SlaveEvent(type, Slave, this));
        };

        private Action<T> WrapAction<T>(SlaveEventType type, Action<T> action) => input =>
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
