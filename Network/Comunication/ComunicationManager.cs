using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using DreamBot.Debugging;
using DreamBot.Network.Comunication.Listeners;
using DreamBot.Network.Protocol.Messages;
using DreamBot.Utils;
using DreamBot.Workers;

namespace DreamBot.Network.Comunication
{
    public class ComunicationManager : IMessageSender
    {
        private readonly IMessageListener _listener;
        private readonly IWorkScheduler _worker;
        private readonly Queue<Package> _receivedMessageQueue;
        private readonly Queue<Package> _sendMessageQueue;
        private readonly List<IPAddress> _blackList; 
        private readonly Dictionary<IPAddress, int> _requestsByIp;

        public EventHandler<PackageReceivedEventArgs<IPEndPoint>> PackageReceivedEventArgs;

        public ComunicationManager(IMessageListener listener, IWorkScheduler worker)
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
            var receivedCount = _receivedMessageQueue.Count;
            for (var i = 0; i < receivedCount; i++)
            {
                var package = _receivedMessageQueue.Dequeue();
                Events.Raise(PackageReceivedEventArgs, this, new PackageReceivedEventArgs<IPEndPoint>(package.EndPoint, package.Data));
            }
        }

        private void SendPendingMessages()
        {
            var sendCount = _sendMessageQueue.Count;
            for (var i = 0; i < sendCount; i++)
            {
                var pkg = _sendMessageQueue.Dequeue();
                _listener.Send(pkg);
            }
        }

        private void AnalyzeRequestList()
        {
            lock (_requestsByIp)
            {
                foreach (var requests in _requestsByIp)
                {
                    var ip = requests.Key;
                    var num = requests.Value;

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
                var package = new Package(endPoint, message); 
                _sendMessageQueue.Enqueue(package);
            }
        }

        public void Receive(IPEndPoint endPoint, byte[] message)
        {
            //if (!IsExpectedMessage(endPoint, message))
            //{
            //    Logger.Log(TraceEventType.Error,  "[X] <--- Unexpected {1,-22}", message.GetType().Name);
            //    return;
            //}
            var ip = endPoint.Address;
            if (IsBlocked(ip)) return;
            IncrementRequestByIp(ip);

            var package = new Package(endPoint, message); 

            _receivedMessageQueue.Enqueue(package);
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
            Logger.Verbose(1, "Blocking IP {0}", ip);
            _blackList.Add(ip);
        }

        //private bool IsExpectedMessage(IPEndPoint endpoint, Message msg)
        //{
        //    // TODO: penalizar
        //    var messageType = _messageManager.GetMessageType(msg.Header.MessageId);

        //    return messageType == MessageType.Request
        //           || (messageType == MessageType.Reply) && _waitingForReply.VerifyExpected(msg.Header.BotId, msg.Header.CorrelationId);
        //}

        [Conditional("DEBUG")]
        internal void Dump()
        {
            Dumper.Dump(_blackList, new[] {
                new Column<IPAddress> {Title = "IP Address", Width = -40, m = info => info.ToString()}
            });
        }

    }
}