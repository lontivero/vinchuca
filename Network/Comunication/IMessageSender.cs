using System.Net;

namespace Vinchuca.Network.Comunication
{
    public interface IMessageSender
    {
        void Send(IPEndPoint endPoint, byte[] message);
    }
}