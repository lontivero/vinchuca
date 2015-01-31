using System;
using DreamBot.Network.Protocol.Messages;
using DreamBot.System;

namespace DreamBot.Network.Comunication
{
    class ReplyWait
    {
        public Package Package { get; private set; }
        public DateTime Sent { get; set; }
        public ulong CorrelationId { get; private set; }
        public int Attempts { get; set; }
        public bool IsTimeout
        {
            get { return (DateTimeProvider.UtcNow - Sent).TotalSeconds > 20; }
        }
        public ReplyWait(Package package, ulong correlationId)
        {
            Package = package;
            CorrelationId = correlationId;
            Sent = DateTimeProvider.UtcNow;
            Attempts = 0;
        }
    }
}