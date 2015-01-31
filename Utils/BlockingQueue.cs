using System.Collections.Generic;
using System.Threading;

namespace DreamBot.Utils
{
    class Cell<T>
    {
        internal T m_obj;
        internal Cell(T obj) { m_obj = obj; }
    }

    public class BlockingQueue<T>
    {
        private readonly Queue<Cell<T>> m_queue = new Queue<Cell<T>>();

        public void Enqueue(T obj)
        {
            var c = new Cell<T>(obj);
            lock (m_queue)
            {
                m_queue.Enqueue(c);
                Monitor.Pulse(m_queue);
                Monitor.Wait(m_queue);
            }
        }

        public T Dequeue()
        {
            Cell<T> c;
            lock (m_queue)
            {
                while (m_queue.Count == 0)
                    Monitor.Wait(m_queue);
                c = m_queue.Dequeue();
                Monitor.Pulse(m_queue);
            }
            return c.m_obj;
        }
    }

    class xBlockingQueue<T>
    {
        private readonly Queue<T> _queue = new Queue<T>();
        private readonly object _queueLock = new object();
        private readonly AutoResetEvent _resetEvent = new AutoResetEvent(false);

        public T Take()
        {
            lock (_queueLock)
            {
                if (_queue.Count > 0)
                    return _queue.Dequeue();
            }
            _resetEvent.WaitOne();

            return Take();
        }

        public void Add(T obj)
        {
            lock (_queueLock)
            {
                if(_queue.Count > 4 * 1024) return;
                _queue.Enqueue(obj);
                _resetEvent.Set();
            }
        }

        public int Count
        {
            get
            {
                lock (_queueLock)
                {
                    return _queue.Count;
                }
            }
        }
    }
}