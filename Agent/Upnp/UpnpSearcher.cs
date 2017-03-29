using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Xml;

namespace Vinchuca.Upnp
{
    public class UpnpSearcher
    {
        private readonly IPAddressesProvider _ipprovider;

        private static readonly string[] ServiceTypes =
        {
            "WANIPConnection:2",
            "WANPPPConnection:2",
            "WANIPConnection:1",
            "WANPPPConnection:1"
        };

        private List<UdpClient> _sockets;
        public EventHandler<DeviceEventArgs> DeviceFound;
        private bool _searching;
        private object _sync = new object();

        public UpnpSearcher()
        {
            _ipprovider = new IPAddressesProvider();
        }

        public void Search()
        {
            ThreadPool.QueueUserWorkItem(_ =>
            {
                _searching = true;
                var attempts = 0;
                _sockets = CreateSockets();
                while (_searching && attempts++ < 5)
                {
                    Discover();
                    Receive();
                    Thread.Sleep(50);
                }
                CloseSockets();
            });
        }

        public void Stop()
        {
            lock (_sync)
            {
                _searching = false;
            }
        }

        private List<UdpClient> CreateSockets()
        {
            var clients = new List<UdpClient>();
            try
            {
                var ips = _ipprovider.UnicastAddresses();

                foreach (var ipAddress in ips)
                {
                    try
                    {
                        clients.Add(new UdpClient(new IPEndPoint(ipAddress, 0)));
                    }
                    catch (Exception)
                    {
                    }
                }
            }
            catch (Exception)
            {
                clients.Add(new UdpClient(0));
            }
            return clients;
        }

        private void Discover(UdpClient client)
        {
            var searchEndpoint = new IPEndPoint(
                IPAddress.Parse("239.255.255.250")
                /*IPAddress.Broadcast*/
                , 1900);

            foreach (var serviceType in ServiceTypes)
            {
                const string s = "M-SEARCH * HTTP/1.1\r\n"
                                 + "HOST: 239.255.255.250:1900\r\n"
                                 + "MAN: \"ssdp:discover\"\r\n"
                                 + "MX: 3\r\n"
                                 + "ST: urn:schemas-upnp-org:service:{0}\r\n\r\n";
                var datax = string.Format(CultureInfo.InvariantCulture, s, serviceType);
                var data = Encoding.ASCII.GetBytes(datax);

                for (var i = 0; i < 3; i++)
                {
                    client.Send(data, data.Length, searchEndpoint);
                }
            }
        }

        private UpnpNatDevice AnalyseReceivedResponse(IPAddress localAddress, byte[] response, IPEndPoint endpoint)
        {
            try
            {
                var dataString = Encoding.UTF8.GetString(response);
                var message = new DiscoveryResponseMessage(dataString);
                var serviceType = message["ST"];

                if (!IsValidControllerService(serviceType))
                {
                    return null;
                }
                var location = message["Location"] ?? message["AL"];
                var locationUri = new Uri(location);

                var deviceInfo = BuildUpnpNatDeviceInfo(localAddress, locationUri);
                return new UpnpNatDevice(deviceInfo);
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        private static bool IsValidControllerService(string serviceType)
        {
            foreach (var serviceName in ServiceTypes)
            {
                var serviceUrn = string.Format("urn:schemas-upnp-org:service:{0}", serviceName);
                if (serviceType.IndexOf(serviceUrn, StringComparison.OrdinalIgnoreCase) >= 0)
                    return true;
            }
            return false;
        }

        private UpnpNatDeviceInfo BuildUpnpNatDeviceInfo(IPAddress localAddress, Uri location)
        {
            WebResponse response = null;
            try
            {
                var request = WebRequest.Create(location);
                request.Proxy = null;
                request.Headers.Add("ACCEPT-LANGUAGE", "en");
                request.Method = "GET";

                response = request.GetResponse();

                var httpresponse = response as HttpWebResponse;

                if (httpresponse != null && httpresponse.StatusCode != HttpStatusCode.OK)
                {
                    var message = string.Format("Couldn't get services list: {0} {1}", httpresponse.StatusCode,
                        httpresponse.StatusDescription);
                    throw new Exception(message);
                }

                var xmldoc = ReadXmlResponse(response);

                var ns = new XmlNamespaceManager(xmldoc.NameTable);
                ns.AddNamespace("ns", "urn:schemas-upnp-org:device-1-0");
                var services = xmldoc.SelectNodes("//ns:service", ns);

                foreach (XmlNode service in services)
                {
                    var serviceType = Utils.GetXmlElementText(service, "serviceType");
                    if (!IsValidControllerService(serviceType)) continue;
                    var serviceControlUrl = Utils.GetXmlElementText(service, "controlURL");
                    return new UpnpNatDeviceInfo(localAddress, location, serviceControlUrl, serviceType);
                }

                throw new Exception("No valid control service was found in the service descriptor document");
            }
            finally
            {
                response?.Close();
            }
        }

        private static XmlDocument ReadXmlResponse(WebResponse response)
        {
            using (var reader = new StreamReader(response.GetResponseStream(), Encoding.UTF8))
            {
                var servicesXml = reader.ReadToEnd();
                var xmldoc = new XmlDocument();
                xmldoc.LoadXml(servicesXml);
                return xmldoc;
            }
        }

        private void Discover()
        {
            foreach (var socket in _sockets)
            {
                try
                {
                    Discover(socket);
                }
                catch (Exception e)
                {
                }
            }
        }

        private void Receive()
        {
            foreach (var client in _sockets)
            {
                if (client.Available <= 0) continue;

                var localHost = ((IPEndPoint) client.Client.LocalEndPoint).Address;
                var receivedFrom = new IPEndPoint(IPAddress.None, 0);
                var buffer = client.Receive(ref receivedFrom);
                var device = AnalyseReceivedResponse(localHost, buffer, receivedFrom);

                if (device != null) RaiseDeviceFound(device);
            }
        }

        public void CloseSockets()
        {
            foreach (var udpClient in _sockets)
            {
                udpClient.Close();
            }
        }

        private void RaiseDeviceFound(UpnpNatDevice device)
        {
            var handler = DeviceFound;
            handler?.Invoke(this, new DeviceEventArgs(device));
        }
    }

    public class DeviceEventArgs : EventArgs
    {
        public DeviceEventArgs(UpnpNatDevice device)
        {
            Device = device;
        }

        public UpnpNatDevice Device { get; private set; }
    }

    class DiscoveryResponseMessage
    {
        private readonly IDictionary<string, string> _headers = new Dictionary<string, string>();

        public DiscoveryResponseMessage(string message)
        {
            var lines = message.Split(new[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 1; i < lines.Length; i++)
            {
                var h = lines[i];
                var c = h.Split(':');
                var key = c[0];
                var value = string.Empty;
                if (c.Length > 1)
                {
                    value = string.Join(":", c, 1, c.Length - 1);
                }
                _headers.Add(key.ToUpperInvariant(), value.Trim());
            }
        }

        public string this[string key]
        {
            get { return this._headers[key.ToUpperInvariant()]; }
        }
    }

}
