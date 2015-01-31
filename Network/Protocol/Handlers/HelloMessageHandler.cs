using System;
using DreamBot.Network.Protocol.Messages;
using DreamBot.Network.Protocol.Messages.System;
using DreamBot.Network.Protocol.Peers;

namespace DreamBot.Network.Protocol.Handlers
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

            if (_peerList.TryRegister(new PeerInfo(botMessage.Header.BotId, endpoint)))
            {
                var endpoints = _peerList.GetPeersEndPoint();
                var bestEndpoints = endpoints.GetRange(0, Math.Min(3, endpoints.Count));

                var reply = new HelloReplyMessage {
                    BotVersion = 1,
                    Peers = bestEndpoints.ToArray(),
                };
                _messageManager.Send(reply, botMessage.Header.BotId, botMessage.Header.CorrelationId);
            }
        }
    }
}