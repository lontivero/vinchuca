using System;
using System.Collections.Generic;
using DreamBot.System;
using DreamBot.Utils;

namespace DreamBot.Workers
{
    internal class ScheduledAction : IComparable<ScheduledAction>
    {
        private static readonly Queue<ScheduledAction> Pool = new Queue<ScheduledAction>();
        public Action Action { get; private set; }
        public TimeSpan Interval { get; private set; }
        public DateTime NextExecutionDate { get; set; }
        public bool Repeat { get; private set; }

        private ScheduledAction(){}

        public void Execute()
        {
            Action();
        }

        [global::System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods",
            Justification = "NextExecutionDate.CompareTo does the job")]
        public int CompareTo(ScheduledAction other)
        {
            if (other == this) return 0;

            var diff = NextExecutionDate.CompareTo(other.NextExecutionDate);
            return (diff >= 0) ? 1 : -1;
        }

        public static ScheduledAction Create(Action action, TimeSpan interval, bool repeat)
        {
            var sa = Pool.Count > 0 ? Pool.Dequeue() : new ScheduledAction();
            sa.Action = action;
            sa.Interval = interval;
            sa.NextExecutionDate = DateTimeProvider.UtcNow + interval;
            sa.Repeat = repeat;
            return sa;
        }

        public void Release()
        {
            Pool.Enqueue(this);
        }
    }
}