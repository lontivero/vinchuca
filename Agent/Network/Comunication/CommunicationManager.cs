using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using Vinchuca.Network.Comunication.Listeners;
using Vinchuca.Network.Protocol.Messages;
using Vinchuca.Utils;
using Vinchuca.Workers;

namespace Vinchuca.Network.Comunication
{
    public class CommunicationManager : IMessageSender
    {
        private readonly IMessageListener _listener;
        private readonly IWorkScheduler _worker;
        private readonly Queue<Package> _receivedMessageQueue;
        private readonly Queue<Package> _sendMessageQueue;
        private readonly List<IPAddress> _blackList; 
        private readonly Dictionary<IPAddress, int> _requestsByIp;
        private static readonly Log Logger = new Log(new TraceSource("Comm-Manager", SourceLevels.Verbose));

        public EventHandler<PackageReceivedEventArgs<IPEndPoint>> PackageReceivedEventArgs;

        public CommunicationManager(IMessageListener listener, IWorkScheduler worker)
        {
            _listener = listener;
            _worker = worker;

            _receivedMessageQueue = new Queue<Package>();
            _sendMessageQueue = new Queue<Package>();
            _blackList = new List<IPAddress>();
            _requestsByIp = new Dictionary<IPAddress, int>();

            _worker.QueueForever(SendReceive, TimeSpan.FromMilliseconds(200));
            _worker.QueueForever(AnalyzeRequestList, TimeSpan.FromMinutes(1));
        }

        private void SendReceive()
        {
            SendPendingMessages();
            ReceiveAndProcessPendingMessages();
        }

        private void ReceiveAndProcessPendingMessages()
        {
            lock (_receivedMessageQueue)
            {
                var receivedCount = _receivedMessageQueue.Count;
                for (var i = 0; i < receivedCount; i++)
                {
                    var package = _receivedMessageQueue.Dequeue();
                    Events.Raise(PackageReceivedEventArgs, this, new PackageReceivedEventArgs<IPEndPoint>(package.EndPoint, package.Data, package.Count));
                }
            }
        }

        private void SendPendingMessages()
        {
            lock (_sendMessageQueue)
            {
                var sendCount = _sendMessageQueue.Count;
                for (var i = 0; i < sendCount; i++)
                {
                    var pkg = _sendMessageQueue.Dequeue();
                    _listener.Send(pkg);
                }
            }
        }

        private void AnalyzeRequestList()
        {
            lock (_requestsByIp)
            {

                var ips = new IPAddress[_requestsByIp.Count];
                _requestsByIp.Keys.CopyTo(ips, 0);
                foreach (var ip in ips)
                {
                    var num = _requestsByIp[ip];

                    if(num > 10 && !IsBlocked(ip))
                    {
                        BlockIp(ip);
                        return;
                    }
                    _requestsByIp[ip] = 0;
                }
            }
        }

        public void Send(IPEndPoint endPoint, byte[] message)
        {
            lock (_sendMessageQueue)
            {
                var package = new Package(endPoint, message, message.Length); 
                _sendMessageQueue.Enqueue(package);
            }
        }

        public void Receive(IPEndPoint endPoint, byte[] message, int count)
        {
            var ip = endPoint.Address;
            if (IsBlocked(ip)) return;
            IncrementRequestByIp(ip);

            var package = new Package(endPoint, message, count);
            lock (_receivedMessageQueue)
            {
                _receivedMessageQueue.Enqueue(package);
            }
        }

        private void IncrementRequestByIp(IPAddress ip)
        {
            lock (_requestsByIp)
            {
                if(_requestsByIp.ContainsKey(ip))
                {
                    _requestsByIp[ip]++;
                    return;
                }
                _requestsByIp.Add(ip, 1);
            }
        }

        private bool IsBlocked(IPAddress ip)
        {
            return _blackList.Contains(ip);
        }

        public void BlockIp(IPAddress ip)
        {
            Logger.Verbose("Blocking IP {0}", ip);
            _blackList.Add(ip);
        }
    }
}