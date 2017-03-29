using System;
using System.Net;

namespace Vinchuca.Network.Listeners
{
    public class UdpPacketReceivedEventArgs : EventArgs
    {
        private readonly IPEndPoint _endPoint;
        private readonly byte[] _data;
        private readonly int _bytesReceived;

        public UdpPacketReceivedEventArgs(IPEndPoint endPoint, byte[] data, int bytesReceived)
        {
            _endPoint = endPoint;
            _data = data;
            _bytesReceived = bytesReceived;
        }

        public IPEndPoint EndPoint
        {
            get { return _endPoint; }
        }

        public byte[] Data
        {
            get { return _data; }
        }

        public int BytesReceived
        {
            get { return _bytesReceived; }
        }
    }
}
