using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using DreamBot.Network.Comunication.TCP;

namespace DreamBot.Actions.Socks5
{
    class Socks5
    {
        public static List<AuthTypes> RequestAuth(SocksClient client)
        {
            var buff = Receive(client.Client);

            if (buff == null || (HeaderTypes)buff[0] != HeaderTypes.Socks5) return new List<AuthTypes>();

            var methods = Convert.ToInt32(buff[1]);
            var types = new List<AuthTypes>();
            for (var i = 2; i < methods + 2; i++)
            {
                switch ((AuthTypes)buff[i])
                {
                    case AuthTypes.Login:
                        types.Add(AuthTypes.Login);
                        break;
                    case AuthTypes.None:
                        types.Add(AuthTypes.None);
                        break;
                }
            }
            return types;
        }

        public static User RequestLogin(SocksClient client)
        {
            client.Client.Send(new[] { (byte)HeaderTypes.Socks5, (byte)HeaderTypes.Authentication });
            var buff = Receive(client.Client);

            if (buff == null || buff[0] != 0x01) return null;

            var numusername = Convert.ToInt32(buff[1]);
            var numpassword = Convert.ToInt32(buff[(numusername + 2)]);
            var username = Encoding.ASCII.GetString(buff, 2, numusername);
            var password = Encoding.ASCII.GetString(buff, numusername + 3, numpassword);

            return new User(username, password, (IPEndPoint)client.Client.Sock.RemoteEndPoint);
        }

        public static SocksRequest RequestTunnel(SocksClient client)
        {
            var buff = Receive(client.Client);
            if (buff == null || (HeaderTypes)buff[0] != HeaderTypes.Socks5) return null;

            switch ((StreamTypes)buff[1])
            {
                case StreamTypes.Stream:
                    var fwd = 4;
                    var address = string.Empty;
                    switch ((AddressType)buff[3])
                    {
                        case AddressType.IP:
                            for (var i = 4; i < 8; i++)
                            {
                                address += Convert.ToInt32(buff[i]).ToString() + (i != 7 ? "." : "");
                            }
                            fwd += 4;
                            break;
                        case AddressType.Domain:
                            var domainlen = Convert.ToInt32(buff[4]);
                            address += Encoding.ASCII.GetString(buff, 5, domainlen);
                            fwd += domainlen + 1;
                            break;
                        case AddressType.IPv6:
                            //can't handle IPV6 traffic just yet.
                            return null;
                    }
                    var po = new byte[2];
                    Array.Copy(buff, fwd, po, 0, 2);
                    var x = BitConverter.ToInt16(po, 0);
                    var port = Convert.ToInt32(IPAddress.NetworkToHostOrder(x));
                    port = (port < 1 ? port + 65536 : port);
                    return new SocksRequest(StreamTypes.Stream, (AddressType)buff[3], address, port);
                default:
                    //not supported.
                    return null;
            }
        }

        public static byte[] Receive(TcpClient client)
        {
            var buffer = new byte[2048];
            var received = client.Receive(buffer, 0, buffer.Length);
            return received != -1 ? buffer : null;
        }
    }

    public enum AuthTypes
    {
        Login = 0x02,
        None = 0x00
    }

    public enum HeaderTypes
    {
        Socks5 = 0x05,
        Authentication = 0x02,
        Zero = 0x00
    }

    public enum StreamTypes
    {
        Stream = 0x01,
        Bind = 0x02,
        UDP = 0x03
    }

    public enum AddressType
    {
        IP = 0x01,
        Domain = 0x03,
        IPv6 = 0x04
    }

    public enum SocksError
    {
        Granted = 0x00,
        Failure = 0x01,
        NotAllowed = 0x02,
        Unreachable = 0x03,
        HostUnreachable = 0x04,
        Refused = 0x05,
        Expired = 0x06,
        NotSupported = 0x07,
        AddressNotSupported = 0x08
    }
}
