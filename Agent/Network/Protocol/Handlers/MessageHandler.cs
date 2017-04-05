using Vinchuca.Network.Protocol.Messages;

namespace Vinchuca.Network.Protocol.Handlers
{
    public interface IMessageHandler
    {
        void Handle(BotMessage peerMessage);
    }
}