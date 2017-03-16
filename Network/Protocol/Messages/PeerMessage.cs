using System.Net;

namespace Vinchuca.Network.Protocol.Messages
{
    public class Package
    {
        public IPEndPoint EndPoint { get; private set; }
        public byte[] Data { get; private set; }
        public int Count { get; private set; }

        public Package(IPEndPoint endPoint, byte[] data, int count)
        {
            Count = count;
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