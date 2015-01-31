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

    class MessageMetadata
    {
        public short MessageId { get; private set; }
        public Type Type { get; private set; }
        public IMessageHandler MessageHandler { get; private set; }
        public bool Encrypted { get; private set; }
        public MessageType MessageType { get; private set; }

        public MessageMetadata(short messageId, MessageType messageType, Type type, IMessageHandler messageHandler, bool encrypted)
        {
            MessageId = messageId;
            Type = type;
            MessageType = messageType;
            MessageHandler = messageHandler;
            Encrypted = encrypted;
        }
    }
}