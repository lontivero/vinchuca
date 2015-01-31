namespace DreamBot.Network.Comunication.TCP
{
    class TcpTunnel
    {
        private readonly TcpClient _client;
        private readonly TcpClient _server;

        public TcpTunnel(TcpClient client, TcpClient server)
        {
            _client = client;
            _server = server;

            _client.OnDataReceived += OnClientnDataReceived;
            _server.OnDataReceived += OnServerDataReceived;
            _server.OnClientDisconnected += OnClientDisconnected;
        }

        private void OnClientDisconnected(object sender, ClientEventArgs e)
        {
            _client.Disconnect();
        }

        private void OnServerDataReceived(object sender, DataEventArgs e)
        {
            _client.SendAsync(e.Buffer, e.Offset, e.Count);
            _server.ReceiveAsync();
        }

        private void OnClientnDataReceived(object sender, DataEventArgs e)
        {
            _server.SendAsync(e.Buffer, e.Offset, e.Count);
            _client.ReceiveAsync();
        }

        public void Start()
        {
            try
            {
                _client.ReceiveAsync();
                _server.ReceiveAsync();
            }
            catch
            {
            }
        }
    }
}