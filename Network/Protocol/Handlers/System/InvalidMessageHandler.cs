using Vinchuca.Network.Protocol.Messages;
using Vinchuca.Network.Protocol.Peers;

namespace Vinchuca.Network.Protocol.Handlers
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
