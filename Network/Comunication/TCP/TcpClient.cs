using System;
using System.Net;
using System.Net.Sockets;
using DreamBot.Utils;

namespace DreamBot.Network.Comunication.TCP
{
    public class TcpClient
    {
        public event EventHandler<ClientEventArgs> OnClientConnected;
        public event EventHandler<ClientEventArgs> OnClientDisconnected;

        public event EventHandler<DataEventArgs> OnDataReceived;
        public event EventHandler<DataEventArgs> OnDataSent;

        public Socket Sock { get; set; }
        private byte[] _buffer;

        public TcpClient(Socket sock, int packetSize)
        {
            Sock = sock;
            _buffer = new byte[packetSize];
        }

        public void ConnectAsync(IPEndPoint endpoint)
        {
            Sock.BeginConnect(endpoint, OnEndConnect, Sock);
        }

        private void OnEndConnect(IAsyncResult ar)
        {
            var sock = ar.AsyncState as Socket;
            sock.EndConnect(ar);
            Events.Raise(OnClientConnected, this, new ClientEventArgs(this));
        }

        private void DataReceived(IAsyncResult res)
        {
            try
            {
                var err = SocketError.Success;
                var received = 0;
                var socket = (Socket) res.AsyncState;
                if (socket.Connected) received = socket.EndReceive(res, out err);
                if (received <= 0 || err != SocketError.Success)
                {
                    Disconnect();
                    return;
                }
                var data = new DataEventArgs(this, _buffer, received);
                Events.Raise(OnDataReceived, this, data);
            }
            catch
            {
                Disconnect();
            }
        }

        public int Receive(byte[] data, int offset, int count)
        {
            try
            {
                var received = Sock.Receive(data, offset, count, SocketFlags.None);
                if (received <= 0)
                {
                    Disconnect();
                    return -1;
                }
                return received;
            }
            catch
            {
                Disconnect();
                return -1;
            }
        }

        public void ReceiveAsync()
        {
            Sock.BeginReceive(_buffer, 0, _buffer.Length, SocketFlags.None, DataReceived, Sock);
        }


        public void Disconnect()
        {
            try
            {
                if (Sock != null && Sock.Connected)
                {
                    Events.Raise(OnClientDisconnected, this, new ClientEventArgs(this));
                    Sock.Shutdown(SocketShutdown.Both);
                    Sock.Close();
                    Sock = null;
                    return;
                }
                Events.Raise(OnClientDisconnected, this, new ClientEventArgs(this));
                Dispose();
            }
            catch { }
        }

        private void DataSent(IAsyncResult res)
        {
            try
            {
                var sent = ((Socket)res.AsyncState).EndSend(res);
                if (sent >= 0) return;
                Sock.Shutdown(SocketShutdown.Both);
                Sock.Close();
            }
            catch
            {
                Disconnect();
            }
        }

        public bool Send(byte[] buff)
        {
            return Send(buff, 0, buff.Length);
        }

        public void SendAsync(byte[] buff, int offset, int count)
        {
            try
            {
                if (Sock == null || !Sock.Connected) return;
                var data = new DataEventArgs(this, buff, count);
                Events.Raise(OnDataSent, this, data);
                Sock.BeginSend(buff, offset, count, SocketFlags.None, DataSent, Sock);
            }
            catch
            {
                Disconnect();
            }
        }

        public bool Send(byte[] buff, int offset, int count)
        {
            try
            {
                if (Sock != null)
                {
                    if (Sock.Send(buff, offset, count, SocketFlags.None) <= 0)
                    {
                        Disconnect();
                        return false;
                    }
                    return true;
                }
                return false;
            }
            catch
            {
                Disconnect();
                return false;
            }
        }
        bool _disposed;

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            if (disposing)
            {
                Sock = null;
                _buffer = null;
                OnClientDisconnected = null;
                OnDataReceived = null;
                OnDataSent = null;
            }

            _disposed = true;
        }
    }
}
