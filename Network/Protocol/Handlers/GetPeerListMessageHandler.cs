using DreamBot.Network.Protocol.Messages;
using DreamBot.Network.Protocol.Messages.System;
using DreamBot.Network.Protocol.Peers;

namespace DreamBot.Network.Protocol.Handlers
{
    public class GetPeerListMessageHandler : IMessageHandler
    {
        private readonly PeerList _peerList;
        private readonly MessageManager _messageManager;

        public GetPeerListMessageHandler(PeerList peerList, MessageManager messageManager)
        {
            _peerList = peerList;
            _messageManager = messageManager;
        }

        public void Handle(BotMessage botMessage)
        {
            var response = new GetPeerListReplyMessage
            {
                Peers = _peerList.GetPeersEndPoint().ToArray(),
            };
            _messageManager.Send(response, botMessage.Header.BotId, botMessage.Header.CorrelationId);
        }
    }
}