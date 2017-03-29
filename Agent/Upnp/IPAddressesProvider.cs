using System.Collections.Generic;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;

namespace Vinchuca.Upnp
{
    internal class IPAddressesProvider
    {
        public IEnumerable<IPAddress> UnicastAddresses()
        {

            return IPAddresses(p =>
            {
                var list = new List<IPAddress>();
                foreach (var addr in p.UnicastAddresses)
                {
                    list.Add(addr.Address);
                }
                return list;
            });
        }

        public delegate TResult Func<in T, out TResult>(T arg);

        private static IEnumerable<IPAddress> IPAddresses(
            Func<IPInterfaceProperties, IEnumerable<IPAddress>> ipExtractor)
        {
            foreach (var networkInterface in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (networkInterface.OperationalStatus == OperationalStatus.Up ||
                    networkInterface.OperationalStatus == OperationalStatus.Unknown)
                {
                    var properties = networkInterface.GetIPProperties();
                    foreach (var address in ipExtractor(properties))
                    {
                        if (address.AddressFamily == AddressFamily.InterNetwork)
                            yield return address;
                    }
                }
            }
        }
    }
}