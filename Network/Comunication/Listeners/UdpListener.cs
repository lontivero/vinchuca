using System;
using System.Net;
using System.Net.Sockets;
using DreamBot.Network.Comunication.Listeners;
using DreamBot.Utils;

namespace DreamBot.Network.Listeners
{
    public class UdpListener : ListenerBase
    {
        public event EventHandler<UdpPacketReceivedEventArgs> UdpPacketReceived;

        internal static readonly BlockingPool<SocketAsyncEventArgs> SaeaPool =
            new BlockingPool<SocketAsyncEventArgs>(() =>
                {
                    var e = new SocketAsyncEventArgs();
                    return e;
                });

        public UdpListener(int port) : base(port)
        {
        }

        protected override Socket CreateSocket()
        {
            var socket = new Socket(_endPoint.AddressFamily, SocketType.Dgram, ProtocolType.Udp);
            socket.Bind(_endPoint);
            return socket;
        }

        protected override bool ListenAsync(SocketAsyncEventArgs saea)
        {
            const int bufferSize = 4 * 1024;
            saea.SetBuffer(new byte[bufferSize], 0, bufferSize);
            saea.RemoteEndPoint = new IPEndPoint(IPAddress.Any, Port);
            return Listener.ReceiveFromAsync(saea);
        }

        protected override void Notify(SocketAsyncEventArgs saea)
        {
            var endPoint = saea.RemoteEndPoint as IPEndPoint;
            Events.Raise(UdpPacketReceived, this, new UdpPacketReceivedEventArgs(endPoint, saea.Buffer));
        }
    }
}