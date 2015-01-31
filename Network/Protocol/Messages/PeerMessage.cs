using System.Net;

namespace DreamBot.Network.Protocol.Messages
{
    public class Package
    {
        public IPEndPoint EndPoint { get; private set; }
        public byte[] Data { get; private set; }

        public Package(IPEndPoint endPoint, byte[] data)
        {
            EndPoint = endPoint;
            Data = data;
        }
    }

    public class BotMessage
    {
        public Message Message { get; set; }
        public BotHeader Header { get; set; }
    }
}