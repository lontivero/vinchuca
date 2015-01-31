using System;
using System.Net;

namespace DreamBot.Network.Listeners
{
    public class UdpPacketReceivedEventArgs : EventArgs
    {
        private readonly IPEndPoint _endPoint;
        private readonly byte[] _data;

        public UdpPacketReceivedEventArgs(IPEndPoint endPoint, byte[] data)
        {
            _endPoint = endPoint;
            _data = data;
        }

        public IPEndPoint EndPoint
        {
            get { return _endPoint; }
        }

        public byte[] Data
        {
            get { return _data; }
        }
    }
}
