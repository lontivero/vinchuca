using System.Diagnostics;

namespace Vinchuca
{
    [Remove]
    public class Log
    {
        private readonly TraceSource _source;
        private static readonly TraceListener Ctl = new ColorConsoleTraceListener(0);

        public Log(TraceSource source)
        {
            _source = source;
            _source.Listeners.Add(Ctl);
        }
        public void Info(string format, params object[] p)
        {
            _source.TraceEvent(TraceEventType.Information, 0, format, p);
        }

        public void Verbose(string format, params object[] p)
        {
            _source.TraceEvent(TraceEventType.Verbose, 0, format, p);
        }

        public void Warn(string format, params object[] p)
        {
            _source.TraceEvent(TraceEventType.Warning, 0, format, p);
        }
    }
}
