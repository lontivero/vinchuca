using DreamBot.Network.Protocol.Messages;

namespace DreamBot.Network.Protocol.Peers
{
    class BotPackage
    {
        public BotHeader Header { get; private set; }
        public byte[] Payload { get; private set; }

        public BotPackage(BotHeader header, byte[] payload)
        {
            Header = header;
            Payload = payload;
        }
    }
}
