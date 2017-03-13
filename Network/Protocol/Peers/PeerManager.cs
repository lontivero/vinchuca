using System;
using System.Diagnostics;
using System.Net;
using System.Security.Cryptography;
using DreamBot.Crypto;
using DreamBot.Network.Comunication;
using DreamBot.Network.Protocol.Messages;
using DreamBot.System;
using DreamBot.Utils;
using DreamBot.Workers;

namespace DreamBot.Network.Protocol.Peers
{
    public class PeerManager
    {
        private readonly BotIdentifier _botId;
        private readonly IWorkScheduler _worker;
        private readonly CommunicationManager _communicationManager;
        internal readonly ReplyWaitManager WaitingForReply;

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
            Logger.Verbose(3, "Broken Bot detected at {0}", e.PeerInfo);
            _communicationManager.BlockIp(e.PeerInfo.EndPoint.Address);
        }

        private void PackageReceivedEventArgs(object sender, PackageReceivedEventArgs<IPEndPoint> e)
        {
            var data = e.Payload;

            //var now = new TimeSpan(DateTime.UtcNow.Ticks);
            //var minutes = (long)now.TotalMinutes;
            //var xor = new Mod2(BitConverter.GetBytes(minutes));
            //xor.Decrypt(data);

            var rc4 = new Rc4(_botId.ToByteArray());
            rc4.Decrypt(data);

            var botHeader = BotHeader.Decode(data);
            if(!IsValidHeader(botHeader))
            {
                _peerList.Punish(botHeader.BotId);
                return;
            }

            botHeader.EndPoint = e.Proto;
            _peerList.UpdatePeer(botHeader.BotId);

            var args = new PackageReceivedEventArgs<BotHeader>(botHeader, data);
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

        private bool IsValidHeader(BotHeader botHeader)
        {
            return !(botHeader.Padding < 2 || botHeader.Padding > 128
                    || botHeader.MessageId < 0 || botHeader.MessageId > 4
                    || botHeader.Ttl < 2 || botHeader.Ttl > 6);
        }

        private void PurgeTimeouts()
        {
            WaitingForReply.PurgeTimeouts();
        }

        public void Send(short messageId, ulong correlationId, short ttl, byte[] payload, BotIdentifier peerBotId, int requiredWork)
        {
            if (!_peerList.IsRegisteredBot(peerBotId) && messageId != 0)
            {
                Logger.Verbose(3, "Cannot send message to unkown {0} bot", peerBotId);
                return;
            }
            byte[] message;
            BotHeader header;
            do
            {
                var padding = RandomUtils.NextPadding();
                header = new BotHeader
                {
                    CorrelationId = correlationId == 0 ? RandomUtils.NextCorrelationId() : correlationId,
                    BotId = _botId,
                    MessageId = messageId,
                    PayloadSize = (short) payload.Length,
                    Padding = (short) padding.Length,
                    Ttl = ttl == 0 ? RandomUtils.NextTtl() : ttl
                };

                var preambule = BufferUtils.Concat(header.Encode(), padding);
                message = BufferUtils.Concat(preambule, payload);
            } while (!PoW.IsEnough(message, 0, message.Length, requiredWork));

            var rc4 = new Rc4(peerBotId.ToByteArray());
            rc4.Encrypt(message);

            //var now = new TimeSpan(DateTime.UtcNow.Ticks);
            //var minutes = (long)now.TotalMinutes;
            //var xor = new Mod2(BitConverter.GetBytes(minutes));
            //xor.Encrypt(message);

            var endPoint = _peerList[peerBotId];

            Logger.Verbose(3, "{0}@{1} {2}", header.BotId, endPoint, header.CorrelationId);
            _communicationManager.Send(endPoint, message);
            if (correlationId == 0)
            {
                WaitingForReply.Add(new Package(endPoint, message), header.CorrelationId);
            }
        }
    }
}