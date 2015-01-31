using System.Diagnostics;

namespace DreamBot
{
    [Remove]
    public static class Logger
    {
        private static TraceSource[] Tracers;

        static Logger()
        {
            var cctl = new ColorConsoleTraceListener(0);

            Tracers = new TraceSource[] {
                new TraceSource("BOT", SourceLevels.Verbose),
                new TraceSource("Communication", SourceLevels.Verbose),
                new TraceSource("List-Manager", SourceLevels.Verbose),
                new TraceSource("Peer-Manager", SourceLevels.Verbose),
                new TraceSource("Mesg-Manager", SourceLevels.Verbose)
            };

            foreach (var traceSource in Tracers)
            {
                traceSource.Listeners.Add(cctl);
            }
        }

        public static void Info(int ti, string format, params object[] p)
        {
            Tracers[ti].TraceEvent(TraceEventType.Information, 0, format, p);            
        }

        public static void Verbose(int ti, string format, params object[] p)
        {
            Tracers[ti].TraceEvent(TraceEventType.Verbose, 0, format, p);
        }

        public static void  Warn(int ti, string format, params object[] p)
        {
            Tracers[ti].TraceEvent(TraceEventType.Warning, 0, format, p);
        }
    }
}
