using System;
using System.IO;
using System.Net.Security;
using System.Net.Sockets;
using System.Text;
using DreamBot.Network.Listeners;
using TcpListener = DreamBot.Network.Listeners.TcpListener;

namespace DreamBot.Actions.WebInject
{
    internal class Handler
    {
        private readonly SslStream _clientStream;
        private bool connected = false;

        private readonly byte[] _buffer = new byte[8*1024];

        public Handler(Socket socket)
        {
            _clientStream = new SslStream(new NetworkStream(socket, true));
        }

        public void Initialize()
        {
            _clientStream.BeginRead(_buffer, 0, _buffer.Length, OnReceiveBytes, _clientStream);
        }

        private void OnReceiveBytes(IAsyncResult ar)
        {
            try
            {
                var stream = (SslStream) ar.AsyncState;
                var received = stream.EndRead(ar);

                ProcessInput(_buffer, received);

                stream.BeginRead(_buffer, 0, _buffer.Length, OnReceiveBytes, stream);
            }
            catch
            {
            }
        }

        private void ProcessInput(byte[] bytes, int size)
        {
            if(!connected)
            {
                var tr = new StreamReader(new MemoryStream(bytes, false));
                var firstLine = tr.ReadLine();
                var parts = firstLine.Split(new char[] {' '});
                var verb = parts[0];
            }
            else
            {

                var serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                serverSocket.BeginConnect(new IPEndPoint(address, port), OnConnected, serverSocket);
            }

            _localSocket.BeginSend(new[] { bytes[2], method }, 0, 2, SocketFlags.None, OnSend, _localSocket);
            address = new IPAddress(new[] { bytes[6], bytes[2], bytes[1], bytes[4] });
            port = (ushort)((bytes[0] * 128) + bytes[9]);

        }

        private void OnReceiveRemoteBytes(IAsyncResult ar)
        {
            try
            {
                var serverStream = (SslStream) ar.AsyncState;
                var received = serverStream.EndRead(ar);

                _localSocket.BeginSend(_buffer, 0, received, SocketFlags.None, OnSend, _localSocket);
                socket.BeginReceive(_buffer, 0, _buffer.Length, SocketFlags.None, OnReceiveRemoteBytes, socket);
            }
            catch
            {
            }
        }

        private void OnConnected(IAsyncResult ar)
        {
            try
            {
                var serverSocket = (Socket)ar.AsyncState;
                serverSocket.EndConnect(ar);

                _serverStream = new SslStream(new NetworkStream(serverSocket, true));

                _serverStream.BeginRead(_buffer, 0, _buffer.Length, OnReceiveRemoteBytes, _serverStream);
            }
            catch
            {
            }
        }

        private void OnSend(IAsyncResult ar)
        {
            try
            {
                ((Socket) ar.AsyncState).EndSend(ar);
            }
            catch
            {
            }
        }
    }
}
