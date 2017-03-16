using System;
using System.Net.Sockets;
using Vinchuca.Network.Listeners;
using Vinchuca.Network.Protocol.Messages;

namespace Vinchuca.Network.Comunication.Listeners
{
    public interface IMessageListener
    {
        void Send(Package package);
        event EventHandler<UdpPacketReceivedEventArgs> UdpPacketReceived;
        void Start();
        void Stop();
    }

    class MessageListener : UdpListener, IMessageListener
    {
        public MessageListener(int port) 
            : base(port)
        {
        }

        public void Send(Package package)
        {
            if (Status == ListenerStatus.Stopped) return;

            var saea = SaeaPool.Take();
            saea.Completed += IOSendCompleted;
            saea.SetBuffer(package.Data, 0, package.Count);
            saea.RemoteEndPoint = package.EndPoint;
            var async = Listener.SendToAsync(saea);

            if (!async)
            {
                IOSendCompleted(null, saea);
            }
        }

        private void IOSendCompleted(object sender, SocketAsyncEventArgs saea)
        {
            try
            {
            }
            finally
            {
                saea.Completed -= IOSendCompleted;
                SaeaPool.Add(saea);
            }
        }
    }
}