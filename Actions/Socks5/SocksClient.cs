using System;
using System.Net;
using DreamBot.Network.Comunication.TCP;
using DreamBot.Utils;

namespace DreamBot.Actions.Socks5
{
    public class SocksClient
    {
        public event EventHandler<SocksClientEventArgs> OnClientDisconnected;
        public event EventHandler<SocksAuthenticationEventArgs> OnClientAuthenticating;

        public TcpClient Client;
        public bool Authenticated { get; private set; }
        public bool Authentication = false;

        public SocksClient(TcpClient client)
        {
            Client = client;
        }

        public void Begin(int packetSize, int timeout)
        {
            Client.OnClientDisconnected += ClientOnClientDisconnected;
            var authtypes = Socks5.RequestAuth(this);
            if (authtypes.Count <= 0)
            {
                Client.Send(new byte[] { 0x00, 0xFF });
                Client.Disconnect();
                return;
            }
            
            if(Authentication && OnClientAuthenticating != null)
            {
                var user = Socks5.RequestLogin(this);
                if (user == null)
                {
                    Client.Disconnect();
                    return;
                }
                var authEventArgs = new SocksAuthenticationEventArgs(user);
                Events.Raise(OnClientAuthenticating, this, authEventArgs);
                var status = authEventArgs.Status;

                Client.Send(new [] { (byte)HeaderTypes.Socks5, (byte)status });
                if (status == LoginStatus.Denied)
                {
                    Client.Disconnect();
                    return;
                }
                
                if (status == LoginStatus.Correct)
                {
                    Authenticated = true;
                }
            }
            else
            {
                Authenticated = true;
                Client.Send(new[] { (byte)HeaderTypes.Socks5, (byte)HeaderTypes.Zero });
            }
            var requestTunnel = Socks5.RequestTunnel(this);
            if (requestTunnel == null)
            {
                Client.Disconnect(); 
                return;
            }

            var tunnel = new SocksTunnel(this, requestTunnel, packetSize);
            tunnel.Open();
        }

        private void ClientOnClientDisconnected(object sender, ClientEventArgs e)
        {
            Events.Raise(OnClientDisconnected, this, new SocksClientEventArgs(this));
        }
    }

    public enum LoginStatus
    {
        Denied = 0xFF,
        Correct = 0x00
    }

    public class User
    {
        public string Username { get; private set; }
        public string Password { get; private set; }
        public IPEndPoint IP { get; private set; }
        public User(string un, string pw, IPEndPoint ip)
        {
            Username = un;
            Password = pw;
            IP = ip;
        }
    }
}
