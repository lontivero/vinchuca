using System.Threading;
using DreamBot.Utils;

namespace DreamBot.Workers
{
    internal class BackgroundWorker : IWorker
    {
        private readonly BlockingQueue<Action> _queue;
        private bool _isStoped = true;

        public BackgroundWorker()
        {
            _queue = new BlockingQueue<Action>();
        }

        public void Start()
        {
            _isStoped = false;
            ThreadPool.QueueUserWorkItem(state => {
                while (!_isStoped)
                {
                    var action = _queue.Dequeue();
                    if(!_isStoped)
                        action();
                }
            });
        }

        public void Stop()
        {
            _isStoped = true;
        }

        public void Queue(Action action)
        {
            _queue.Enqueue(action);
        }
    }
}