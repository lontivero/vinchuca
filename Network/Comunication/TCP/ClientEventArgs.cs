using System;
using DreamBot.Actions.Socks5;

namespace DreamBot.Network.Comunication.TCP
{
    public class ClientEventArgs : EventArgs
    {
        public TcpClient Client { get; private set; }
        public ClientEventArgs(TcpClient client)
        {
            Client = client;
        }
    }
    public class SocksClientEventArgs : EventArgs
    {
        public SocksClient Client { get; private set; }
        public SocksClientEventArgs(SocksClient client)
        {
            Client = client;
        }
    }

    public class SocksAuthenticationEventArgs : EventArgs
    {
        public User User { get; private set; }
        public LoginStatus Status { get; set; }

        public SocksAuthenticationEventArgs(User loginInfo)
        {
            User = loginInfo;
        }
    }
}
