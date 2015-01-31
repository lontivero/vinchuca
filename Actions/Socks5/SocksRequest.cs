using System;
using System.Net;
using System.Text;

namespace DreamBot.Actions.Socks5
{
    public class SocksRequest
    {
        public AddressType Type { get; set; }
        public StreamTypes StreamType { get; private set; }
        public string Address { get; set; }
        public int Port { get; set; }
        public SocksError Error { get; set; }
        public SocksRequest(StreamTypes type, AddressType addrtype, string address, int port)
        {
            Type = addrtype;
            StreamType = type;
            Address = address;
            Port = port;
        }

        public IPAddress IP
        {
            get
            {
                if (Type == AddressType.IP)
                {
                    try
                    {
                        return IPAddress.Parse(Address);
                    }
                    catch { Error = SocksError.NotSupported; return null; }
                }
                
                if (Type == AddressType.Domain)
                {
                    try
                    {
                        foreach (var p in Dns.GetHostAddresses(Address))
                            if (p.AddressFamily == global::System.Net.Sockets.AddressFamily.InterNetwork)
                                return p;
                        return null;
                    }
                    catch
                    {
                        Error = SocksError.HostUnreachable;
                        return null;
                    }
                }
                return null;
            }
        }

        public byte[] GetData()
        {
            byte[] data;
            var port = IPAddress.NetworkToHostOrder(Port);

            switch (Type)
            {
                case AddressType.IP: 
                    data = new byte[10];
                    var content = IP.ToString().Split('.');
                    for (var i = 4; i < content.Length + 4; i++)
                        data[i] = Convert.ToByte(Convert.ToInt32(content[i - 4]));
                    Buffer.BlockCopy(BitConverter.GetBytes(port), 0, data, 8, 2);
                    break;

                case AddressType.Domain:
                    data = new byte[Address.Length + 7];
                    data[4] = Convert.ToByte(Address.Length);
                    Buffer.BlockCopy(Encoding.ASCII.GetBytes(Address), 0, data, 5, Address.Length);
                    Buffer.BlockCopy(BitConverter.GetBytes(port), 0, data, data.Length - 2, 2);
                    break;

                default:
                    return null;
            }
            data[0] = 0x05;                
            data[1] = (byte)Error;
            data[2] = 0x00;
            data[3] = (byte)Type;
            return data;
        }
    }
}