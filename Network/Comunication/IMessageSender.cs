using System.Net;

namespace DreamBot.Network.Comunication
{
    public interface IMessageSender
    {
        void Send(IPEndPoint endPoint, byte[] message);
    }
}