using System;
using System.Windows.Media;

namespace BlackHole.Master.Model
{
    public abstract class CommandModel : ViewModel
    {
        public long Id { get; }
        public string HeaderText { get; }
        public string TargetText { get; }
        public Slave Slave { get; }

        public Brush ProgressColor
        {
            get { return m_progressColor; }
            set
            {
                m_progressColor = value;
                NotifyPropertyChange("ProgressColor");
            }
        }

        public long Progress
        {
            get { return m_progress; }
            set
            {
                m_progress = value;
                NotifyPropertyChange("Progress");
            }
        }

        public bool Completed
        {
            get;
            set;
        }

        private long m_progress;
        private Brush m_progressColor;

        protected CommandModel(long id, Slave slave, string headerText, string targetText)
        {
            Id = id;
            Slave = slave;
            HeaderText = headerText;
            TargetText = targetText;
            m_progressColor = Brushes.Green;
        }

        public void UpdateProgression(long current, long maximum) => Progress = (current + 1) * 100 / Math.Max(maximum, 1);
    }
}
