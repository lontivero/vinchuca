using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using DreamBot.Network;
using DreamBot.Network.Comunication.TCP;
using DreamBot.Utils;
using TcpClient = DreamBot.Network.Comunication.TCP.TcpClient;

namespace DreamBot.Actions.WebInject
{
    public class HttpClient
    {
        public readonly TcpClient Client;
        public event EventHandler<HttpClientEventArgs> OnClientDisconnected;

        static HttpClient()
        {
            ServicePointManager.DefaultConnectionLimit = 64;
        }

        public HttpClient(TcpClient client)
        {
            Client = client;
        }

        public void Begin()
        {
            Client.OnClientDisconnected += ClientOnClientDisconnected;
            Client.OnDataReceived += ClientOnOnDataReceived;
            Client.ReceiveAsync();
        }

        private void ClientOnOnDataReceived(object sender, DataEventArgs e)
        {
            Client.OnDataReceived -= ClientOnOnDataReceived;
            var sr = new StreamReader(new MemoryStream(e.Buffer, e.Offset, e.Count, false));
            var firstLine = sr.ReadLine();
            var parts = firstLine.Split(new[] { ' ' });
            var verb = parts[0];
            var authority = parts[1];
            var ver = parts[2];
            var endpoint = ParseDestinationHostAndPort(authority);

            var sw = new StringWriter();
            sw.WriteLine("{0} 200 Connection established", ver);
            sw.WriteLine();
            sw.Flush();
            var b = Encoding.ASCII.GetBytes(sw.ToString());
            Client.SendAsync(b, 0, b.Length);

            var cert = CertificateProvider.GetCertificateForHost(endpoint.Host);
            Console.WriteLine(endpoint.Host);
            var tunnel = new HttpsTunnel(Client, endpoint.Host, cert);
            tunnel.Start();
        }

        protected Socket Connect(DnsEndPoint endpoint)
        {
            var ips = Dns.GetHostAddresses(endpoint.Host);
            Socket socket = null;
            foreach (var ip in ips)
            {
                try
                {
                    socket = new Socket(ip.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                    socket.Connect(ip, endpoint.Port);
                    break;
                }
                catch (Exception)
                {
                    if (ip.Equals(IPAddress.IPv6Loopback))
                        // Do not log that
                        continue;

                    if (socket != null)
                    {
                        socket.Close();
                        socket = null;
                    }
                }
            }

            return socket;
        }

        protected DnsEndPoint ParseDestinationHostAndPort(string authority)
        {
            string host = null;
            var port = 443;

            var c = authority.IndexOf(':');
            if (c < 0)
            {
                host = authority;
            }
            else if (c == authority.Length - 1)
            {
                host = authority.TrimEnd('/');
            }
            else
            {
                host = authority.Substring(0, c);
                port = int.Parse(authority.Substring(c + 1));
            }

            return new DnsEndPoint(host, port);
        }


        private void ClientOnClientDisconnected(object sender, ClientEventArgs e)
        {
            Events.Raise(OnClientDisconnected, this, new HttpClientEventArgs(this));
        }
    }

    internal class HttpsTunnel
    {
        private readonly TcpClient _client;
        private readonly string _host;
        private readonly X509Certificate2 _certificate;
        private readonly SslStream _ssl;

        public HttpsTunnel(TcpClient client, string host, X509Certificate2 cert)
        {
            _client = client;
            _host = host;
            _certificate = cert;

            _ssl = new SslStream(new NetworkStream(client.Sock, true), false);
        }

        public void Start()
        {
            _ssl.BeginAuthenticateAsServer(_certificate, false, SslProtocols.Default, true, OnAutenticatedAsServer, _ssl);
        }

        private void OnAutenticatedAsServer(IAsyncResult ar)
        {
            var ssl = ar.AsyncState as SslStream;
            try
            {
                ssl.EndAuthenticateAsClient(ar);
                ReadRequest(ssl);
            }
            catch(Exception)
            {
                ssl.Close();
            }
        }

        private void ReadRequest(SslStream ssl)
        {
            var sr = new StreamReader(ssl);
            var firstLine = sr.ReadLine();
            if (string.IsNullOrEmpty(firstLine)) return;
            var parts = firstLine.Split(new[] { ' ' });
            var verb = parts[0];
            var uri = parts[1];
            var ver = parts[2];

            var url = "https://" + _host + uri;
            var req = (HttpWebRequest)WebRequest.Create(url);

            req.Method = verb;
            req.Proxy = null;
            req.KeepAlive = false;
            req.AllowAutoRedirect = false;
            req.AutomaticDecompression = DecompressionMethods.None;
            req.ProtocolVersion = new Version(ver.Substring(5));

            var contentLength = ReadRequestHeaders(sr, req);

            if (verb == "POST")
            {
                var postBuffer = new char[contentLength];
                int bytesRead3;
                int totalBytesRead = 0;
                var sw2 = new StreamWriter(req.GetRequestStream());
                while (totalBytesRead < contentLength && (bytesRead3 = sr.ReadBlock(postBuffer, 0, contentLength)) > 0)
                {
                    totalBytesRead += bytesRead3;
                    sw2.Write(postBuffer, 0, bytesRead3);
                }

                sw2.Close();
            }

            HttpWebResponse response;
            try
            {
                response = (HttpWebResponse)req.GetResponse();
            }
            catch (WebException e)
            {
                response = e.Response as HttpWebResponse;
            }
            if (response == null) return;
            var sw = new StreamWriter(ssl, Encoding.UTF8);
            if (!_client.Sock.Connected) return;

            sw.WriteLine("HTTP/{0} {1} {2}", response.ProtocolVersion, (int)response.StatusCode, response.StatusDescription);

            var respHeaders = new StringReader(response.Headers.ToString());
            var line = respHeaders.ReadLine();
            while (!string.IsNullOrEmpty(line))
            {
                if (!line.StartsWith("Connection") &&
                    !line.StartsWith("Transfer-E"))
                    sw.WriteLine(line);
                line = respHeaders.ReadLine();
            }

            sw.WriteLine();
            sw.Flush();
            if(response.StatusCode == HttpStatusCode.NotModified)
            {
                response.Close(); ssl.Flush(); _client.Disconnect(); 
                return;
            }

            var srr = response.GetResponseStream();
            var tunnel = new StreamTunnel(srr, ssl);
            tunnel.Start(()=> { response.Close(); ssl.Flush(); _client.Disconnect(); });
        }



        private static int ReadRequestHeaders(StreamReader sr, HttpWebRequest webReq)
        {
            String httpCmd;
            int contentLen = 0;
            var dict = new Dictionary<string, Action<string>> {
                {"host", s => {} },
                {"user-agent", s => webReq.UserAgent = s },
                {"accept", s => webReq.Accept = s },
                {"referer", s => webReq.Referer = s },
                {"cookie", s => webReq.Headers["Cookie"] = s },
                {"proxy-connection", s => {} },
                {"connection", s => {} },
                {"keep-alive", s => {} },
                {"content-length", s => int.TryParse(s, out contentLen) },
                {"content-type", s => webReq.ContentType = s },
                {"if-modified-since", s => {
                    var sb = s.Split(new[] {';'});
                    DateTime d;
                    if (DateTime.TryParse(sb[0], out d))
                        webReq.IfModifiedSince = d;
                    } }
            };

            do
            {
                httpCmd = sr.ReadLine();
                if (String.IsNullOrEmpty(httpCmd))
                    return contentLen;
                var header = httpCmd.Split(new[]{':'}, 2, StringSplitOptions.None);

                var h = header[0].ToLower();
                Action<string> action;
                if(dict.TryGetValue(h, out action))
                {
                    action(header[1]);
                }
                else
                {
                    try
                    {
                        webReq.Headers.Add(header[0], header[1]);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Could not add header {0}.  Exception message:{1}", header[0], ex.Message);
                    }
                }
            } while (!String.IsNullOrEmpty(httpCmd));
            return contentLen;
        }
    }

    public class HttpClientEventArgs : EventArgs
    {
        private readonly HttpClient _httpClient;

        public HttpClientEventArgs(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public HttpClient Client
        {
            get { return _httpClient; }
        }
    }
}
