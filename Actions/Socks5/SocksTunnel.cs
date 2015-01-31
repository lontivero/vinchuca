using System.Net;
using System.Net.Sockets;
using DreamBot.Network;
using DreamBot.Network.Comunication.TCP;
using TcpClient = DreamBot.Network.Comunication.TCP.TcpClient;

namespace DreamBot.Actions.Socks5
{
    class SocksTunnel
    {
        public SocksRequest Req;

        public SocksClient Client;
        public TcpClient RemoteClient;

        private readonly int _packetSize = 2048;
        private TcpTunnel _tcpTunnel;

        public SocksTunnel(SocksClient p, SocksRequest req, int packetSize)
        {
            RemoteClient = new TcpClient(new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp), _packetSize);
            Client = p;
            Req = req;
            _packetSize = packetSize;
        }

        public void Open()
        {
            if (Req.Address == null || Req.Port <= -1)
            {
                Client.Client.Disconnect(); 
                return;
            }

            var socketArgs = Sockets.ConnectSaeaPool.Take();
            socketArgs.RemoteEndPoint = new IPEndPoint(Req.IP, Req.Port);
            socketArgs.Completed += OnCompleted;

            RemoteClient.Sock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            if (!RemoteClient.Sock.ConnectAsync(socketArgs))
            {
                _tcpTunnel = new TcpTunnel(Client.Client, RemoteClient);
                _tcpTunnel.Start();
            }
        }

        void OnCompleted(object sender, SocketAsyncEventArgs e)
        {
            try
            {
                byte[] request = Req.GetData();
                if (e.SocketError != SocketError.Success)
                {
                    request[1] = (byte) SocksError.Unreachable;
                }
                else
                {
                    request[1] = 0x00;
                }

                Client.Client.Send(request);

                switch (e.LastOperation)
                {
                    case SocketAsyncOperation.Connect:
                        //connected;
                        _tcpTunnel = new TcpTunnel(Client.Client, RemoteClient);
                        _tcpTunnel.Start();
                        break;
                }
            }
            finally
            {
                e.Completed -= OnCompleted;
                e.RemoteEndPoint = null;
                Sockets.ConnectSaeaPool.Add(e);
            }
        }
    }
}
