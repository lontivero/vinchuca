using DreamBot.Network.Protocol.Messages;

namespace DreamBot.Network.Protocol.Handlers
{
    public interface IMessageHandler
    {
        void Handle(BotMessage peerMessage);
    }
}