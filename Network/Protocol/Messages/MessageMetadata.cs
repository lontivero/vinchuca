using System;
using Vinchuca.Network.Protocol.Handlers;

namespace Vinchuca.Network.Protocol.Messages
{
    public enum MessageType
    {
        Request,
        Reply,
        Special
    }

    public enum MessageCode : short
    {
        Syn = 0,
        AckSyn = 1,
        Ack = 2,
        GetPeerList = 3,
        GetPeerListReply = 4,
        Ping = 5,
        Pong = 6,
        DDoSStart = 7,
        DDoSStop = 8,
        Backdoor = 9,
        MaxValid = Backdoor,
        Unknown = 255
    }

    public enum Difficulty
    {
#if DEBUG
        NoWork = 0,
        Easiest= 0,
        Easy = 0,
        Medium = 0,
        Hard = 0,
        Hardest= 0
#else
        NoWork = 0,
        Easiest= 2,
        Easy = 8,   
        Medium = 10,
        Hard = 17,
        Hardest= 20
#endif
    }

    class MessageMetadata
    {
        public MessageCode MessageId { get; private set; }
        public Type Type { get; private set; }
        public IMessageHandler MessageHandler { get; private set; }
        public MessageType MessageType { get; private set; }
        public int RequiredWork { get; set; }

        public MessageMetadata(MessageCode messageId, MessageType messageType, Type type, IMessageHandler messageHandler, int requiredWork)
        {
            MessageId = messageId;
            Type = type;
            MessageType = messageType;
            MessageHandler = messageHandler;
            RequiredWork = requiredWork;
        }
    }
}