using DreamBot.Network.Protocol.Messages;
using DreamBot.Network.Protocol.Messages.System;
using DreamBot.Network.Protocol.Peers;

namespace DreamBot.Network.Protocol.Handlers
{
    public class GetPeerListReplyMessageHandler : IMessageHandler
    {
        private readonly PeerList _peerList;
        private readonly MessageManager _messageManager;

        public GetPeerListReplyMessageHandler(PeerList peerList, MessageManager messageManager)
        {
            _peerList = peerList;
            _messageManager = messageManager;
        }

        public void Handle(BotMessage message)
        {
            var msg = message.Message as GetPeerListReplyMessage;
            var hello = new HelloMessage();

            foreach (var peer in msg.Peers)
            {
                _messageManager.Send(hello, peer.BotId, 0);
            }
        }
    }
}