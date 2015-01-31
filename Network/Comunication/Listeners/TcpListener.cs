using System;
using System.Net.Sockets;
using DreamBot.Network.Listeners;
using DreamBot.Utils;

namespace DreamBot.Network.Comunication.Listeners
{
    public class TcpListener : ListenerBase
    {
        internal event EventHandler<NewConnectionEventArgs> ConnectionRequested;

        public TcpListener(int port) : base(port)
        {
        }

        protected override Socket CreateSocket()
        {
            var socket = new Socket(_endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
//          socket.SetIPProtectionLevel(IPProtectionLevel.Unrestricted);
            socket.Bind(_endPoint);
            socket.Listen(4);
            return socket;
        }

        protected override bool ListenAsync(SocketAsyncEventArgs saea)
        {
            return Listener.AcceptAsync(saea);
        }

        protected override void Notify(SocketAsyncEventArgs saea)
        {
            Events.Raise(ConnectionRequested, this, new NewConnectionEventArgs(saea.AcceptSocket));
        }
    }
}