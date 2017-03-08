using System.Collections.Generic;
using DreamBot.Network.Comunication.TCP;
using DreamBot.Network.Listeners;
using TcpListener = DreamBot.Network.Comunication.Listeners.TcpListener;

namespace DreamBot.Actions.WebInject
{
    public class HttpsProxyServer
    {
        private readonly TcpListener _server;

        public List<HttpClient> Clients = new List<HttpClient>();

        public HttpsProxyServer(int port)
        {
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
             //   Clients[i].Disconnect();
            }
            Clients.Clear();
        }

        private void OnClientConnected(object sender, NewConnectionEventArgs e)
        {
            const int packetSize = 8*1024;
            var client = new HttpClient(new TcpClient(e.Socket, packetSize));
            client.OnClientDisconnected += OnClientDisconnected;
            Clients.Add(client);
            client.Begin();
        }

        void OnClientDisconnected(object sender, HttpClientEventArgs e)
        {
            e.Client.OnClientDisconnected -= OnClientDisconnected;
            Clients.Remove(e.Client);
        }
    }
}
