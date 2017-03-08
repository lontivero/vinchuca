using DreamBot.Network.Protocol.Messages;
using DreamBot.Network.Protocol.Peers;

namespace DreamBot.Network.Protocol.Handlers
{
    public class InvalidMessageHandler : IMessageHandler
    {
        private readonly PeerList _peerList;

        public InvalidMessageHandler(PeerList peerList)
        {
            _peerList = peerList;
        }

        public void Handle(BotMessage message)
        {
            _peerList.Punish(message.Header.BotId);
        }
    }
}
