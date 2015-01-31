using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using DreamBot.Network.Protocol.Handlers;
using DreamBot.Network.Protocol.Peers;

namespace DreamBot.Network.Protocol.Messages
{
    public class PackageReceivedEventArgs<T> : EventArgs
    {
        public T Proto { get; set; }
        public byte[] Payload { get; set; }

        public PackageReceivedEventArgs(T proto, byte[] payload)
        {
            Proto = proto;
            Payload = payload;
        }
    }

    public class MessageManager
    {
        private readonly PeerManager _peerManager;
        private readonly IDictionary<short, MessageMetadata> _messageIdMap;
        private readonly IDictionary<Type, MessageMetadata> _messageTypeMap;

        public MessageManager(PeerManager peerManager)
        {
            _messageIdMap = new Dictionary<short, MessageMetadata>();
            _messageTypeMap=new Dictionary<Type, MessageMetadata>();

            _peerManager = peerManager;
            _peerManager.BotPackageReceivedEventArgs += BotPackageReceivedEventArgs;
        }

        private void BotPackageReceivedEventArgs(object sender, PackageReceivedEventArgs<BotHeader> e)
        {
            var message = DecodeMessage(e.Proto.MessageId, e.Payload);
            LogMessaging(message, e.Proto.BotId, false);

            var botMessage = new BotMessage {Header = e.Proto, Message = message};
            ProcessMessage(botMessage);
        }

        public void Register(short messageId, MessageType messageType, Type type, IMessageHandler messageHandler, bool encrypted)
        {
            var metadata = new MessageMetadata(messageId, messageType, type, messageHandler, encrypted);
            _messageIdMap.Add(messageId, metadata);
            _messageTypeMap.Add(type, metadata);
        }

        public void ProcessMessage(BotMessage message)
        {
            var metadata = _messageIdMap[message.Header.MessageId];
            metadata.MessageHandler.Handle(message);
        }

        public byte[] EncodeMessage(Message message, BotIdentifier botIdentifier)
        {
            return message.Encode();
        }

        public Message DecodeMessage(short messageId, byte[] data)
        {
            try
            {
                var messageMetadata = _messageIdMap[messageId];
                var type = messageMetadata.Type;
                var message = (Message)Activator.CreateInstance(type);

                using (var br = new BinaryReader(new MemoryStream(data)))
                {
                    message.Decode(br);
                }
                return message;
            }
            catch (Exception e)
            {
                return new InvalidMessage(e);
            }
        }

        public void Send(Message message, BotIdentifier botId, ulong correlationId)
        {
            var messageId = _messageTypeMap[message.GetType()].MessageId;
            var payload = message.Encode();
            LogMessaging(message, botId, true);
            _peerManager.Send(messageId, correlationId, 0, payload, botId);
        }

        [Conditional("DEBUG")]
        public static void LogMessaging(Message m, BotIdentifier botId, bool sending)
        {
            var messageType = m.GetType().Name;
            var bid = botId.ToString();

            Logger.Verbose(4,  "[{0}] {1} {2,-24} {3,-5} {4} ",
                                   sending ? "S" : "R",
                                   sending ? "--->" : "<---",
                                   messageType,
                                   sending ? "to" : "from",
                                   bid.Substring(0, Math.Min(12, bid.Length))
                );
        }
    }
}