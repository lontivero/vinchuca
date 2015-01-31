using System;

namespace DreamBot.Network.Comunication.TCP
{
    public class DataEventArgs : EventArgs
    {
        public TcpClient Client { get; set; }
        public byte[] Buffer { get; set; }
        public int Count { get; set; }
        public int Offset { get; set; }
        public DataEventArgs(TcpClient client, byte[] buffer, int count)
        {
            Client = client;
            Buffer = buffer;
            Count = count;
            Offset = 0;
        }
    }
}
