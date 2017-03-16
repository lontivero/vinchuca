using Vinchuca.Network.Protocol.Messages;

namespace Vinchuca.Network.Protocol.Peers
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
