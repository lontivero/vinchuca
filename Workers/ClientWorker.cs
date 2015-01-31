
using System;
using DreamBot.Utils;

namespace DreamBot.Workers
{
    class ClientWorker : IWorker, IWorkScheduler
    {
        private readonly BackgroundWorker _backgroundWorker;
        private readonly TimedWorker _timedWorker;

        public ClientWorker()
        {
            _backgroundWorker = new BackgroundWorker();
            _timedWorker = new TimedWorker();
        }

        public void Queue(Action action)
        {
            _backgroundWorker.Queue(action);
        }

        public void QueueForever(Action action, TimeSpan interval)
        {
            _timedWorker.QueueForever(() => _backgroundWorker.Queue(action), interval);
        }

        public void QueueOneTime(Action action, TimeSpan interval)
        {
            _timedWorker.QueueOneTime(() => _backgroundWorker.Queue(action), interval);
        }

        public void Start()
        {
            _backgroundWorker.Start();
            _timedWorker.Start();
        }

        public void Stop()
        {
            _timedWorker.Stop();
            _backgroundWorker.Stop();
        }
    }
}
