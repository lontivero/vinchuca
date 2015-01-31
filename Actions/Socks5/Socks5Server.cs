using System;
using System.Collections.Generic;
using DreamBot.Network.Comunication.Listeners;
using DreamBot.Network.Comunication.TCP;
using DreamBot.Network.Listeners;

namespace DreamBot.Actions.Socks5
{
    public class Socks5Server
    {
        public int Timeout { get; set; }
        public int PacketSize { get; set; }
        public bool Authentication { get; set; }

        public event EventHandler<SocksAuthenticationEventArgs> OnAuthentication;

        private readonly TcpListener _server;

        public List<SocksClient> Clients = new List<SocksClient>();
        
        public Socks5Server(int port)
        {
            Authentication = false;
            Timeout = 1000;
            PacketSize = 128;
            _server = new TcpListener(port);
            _server.ConnectionRequested += OnClientConnected;
        }

        public void Start()
        {
            _server.Start();
        }

        public void Stop()
        {
            _server.Stop();
            for (var i = 0; i < Clients.Count; i++)
            {
                Clients[i].Client.Disconnect();
            }
            Clients.Clear();
        }

        private void OnClientConnected(object sender, NewConnectionEventArgs e)
        {
            var client = new SocksClient(new TcpClient(e.Socket, PacketSize));
            client.OnClientDisconnected += OnClientDisconnected;
            client.OnClientAuthenticating += OnClientAuthenticating;
            Clients.Add(client);
            client.Authentication = Authentication;
            client.Begin(PacketSize, Timeout);
        }

        private void OnClientAuthenticating(object sender, SocksAuthenticationEventArgs e)
        {
            if(!Authentication)
            {
                e.Status = LoginStatus.Correct;
            }
            else
            {
                OnAuthentication(sender, e);
            }
        }

        void OnClientDisconnected(object sender, SocksClientEventArgs e)
        {
            e.Client.OnClientDisconnected -= OnClientDisconnected;
            Clients.Remove(e.Client);
        }
    }
}
