using System;
using System.Diagnostics;
using System.Threading;
using Vinchuca.Workers;

namespace Vinchuca.System.Evation
{
    static class AntiDebugging
    {
        private static DateTime _last = DateTime.MinValue;
        private static readonly ClientWorker _worker = ClientWorker.Instance;

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