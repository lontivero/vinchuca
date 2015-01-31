using System.Collections.Generic;
using System.Net;
using DreamBot.Network.Protocol.Messages;
using DreamBot.System;

namespace DreamBot.Network.Comunication
{
    class ReplyWaitManager 
    {
        private readonly IMessageSender _messageSender;
        private readonly Dictionary<ulong, ReplyWait> _internal = new Dictionary<ulong, ReplyWait>();
        private static readonly object LockObject = new object();

        public ReplyWaitManager(IMessageSender messageSender)
        {
            _messageSender = messageSender;
        }

        public void Add(Package package, ulong correlationId)
        {
            lock (LockObject)
            {
                if(!_internal.ContainsKey(correlationId))
                {
                    _internal.Add(correlationId, new ReplyWait(package, correlationId));
                }
            }
        }

        public bool VerifyExpected(IPEndPoint endPoint, ulong correlationId)
        {
            lock (LockObject)
            {
                if (_internal.ContainsKey(correlationId))
                {
                    return Equals(_internal[correlationId].Package.EndPoint, endPoint);
                }
            }
            return false;
        }


        public void PurgeTimeouts()
        {
            lock (LockObject)
            {
                var array = new ReplyWait[_internal.Count];
                _internal.Values.CopyTo(array, 0);
                foreach (var replyWait in array)
                {
                    if (!replyWait.IsTimeout) continue;

                    if(replyWait.Attempts++ >= 3)
                    {
                        _internal.Remove(replyWait.CorrelationId);
                    }
                    else
                    {
                        replyWait.Sent = DateTimeProvider.UtcNow;
                        _messageSender.Send(replyWait.Package.EndPoint, replyWait.Package.Data);
                    }
                }
            }
        }
    }
}