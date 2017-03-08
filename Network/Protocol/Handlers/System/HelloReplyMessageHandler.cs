using DreamBot.Network.Protocol.Messages;
using DreamBot.Network.Protocol.Messages.System;
using DreamBot.Network.Protocol.Peers;

namespace DreamBot.Network.Protocol.Handlers
{
    public class HelloReplyMessageHandler : IMessageHandler
    {
        private readonly PeerList _peerList;
        private readonly MessageManager _messageManager;

        public HelloReplyMessageHandler(PeerList peerList, MessageManager messageManager)
        {
            _peerList = peerList;
            _messageManager = messageManager;
        }

        public void Handle(BotMessage botMessage)
        {
            var msg = botMessage.Message as HelloReplyMessage;
            var endpoint = botMessage.Header.EndPoint;

            //_peerList.TryRegister(new PeerInfo(botMessage.Header.BotId, endpoint)))
            //_versionManager.CheckBotVersion(msg.BotVersion, msg.Header.BotId, endpoint);
            //_versionManager.CheckCfgVersion(msg.CfgVersion, msg.Header.BotId, endpoint);

            foreach (var peer in msg.Peers)
            {
                if (_peerList.TryRegister(new PeerInfo(peer.BotId, peer.EndPoint)))
                {
                    var hello = new HelloMessage();
                    _messageManager.Send(hello, peer.BotId);
                }
            }
        }
    }
}