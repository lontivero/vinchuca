using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using DreamBot.Network.Comunication.TCP;

namespace DreamBot.Actions.WebInject
{
    internal class Session
    {
        private readonly Socket _clientSocket;
        private readonly Request _request;
        private readonly Response _response;
        private readonly ClientHandler _clientHandler;
        private readonly ServerHandler _serverHandler;

        public Session(Socket clientSocket)
        {
            _clientSocket = clientSocket;
            _request = new Request(this);
            _response = new Response(this);
            _clientHandler = new ClientHandler(this, _clientSocket);
            _serverHandler = new ServerHandler(this);
        }

        public async Task ReceiveRequestAsync()
        {
            await ClientHandler.ReceiveEntityAsync();
            var s = await Request.GetContentStreamAsync();
            var sr = new StreamReader(s);
            var body = await sr.ReadToEndAsync();
        }

        public string ErrorMessage { get; set; }
        public int ErrorStatus { get; set; }

        public Request Request
        {
            get { return _request; }
        }

        public Response Response
        {
            get { return _response; }
        }

        internal bool HaveError { get; set; }

        internal ClientHandler ClientHandler
        {
            get { return _clientHandler; }
        }

        internal async Task ReturnResponse()
        {
            var stream = new BufferedStream(new ConnectionStream(_clientConnection));
            var writer = new StreamWriter(stream);
            await writer.WriteAsync(_response.StatusLine.ResponseLine.ToCharArray());
            await writer.WriteAsync("\r\n".ToCharArray());
            await writer.WriteAsync(_response.Headers.ToCharArray());
            await writer.WriteAsync(_response.Body.ToCharArray());
            await writer.FlushAsync();
            stream.Close();
        }

        public async Task ResendRequestAsync()
        {
            await _serverHandler.ConnectToHostAsync();

        }
    }

    internal class ServerHandler
    {
        private readonly Session _session;
        private TcpClient _client;

        public ServerHandler(Session session)
        {
            _session = session;
        }

        public async void ConnectToHostAsync()
        {
            var uri = GetUriFromRequest();
            var dnsEndPoint = new DnsEndPoint(uri.DnsSafeHost, uri.Port);
            var ipAddresses = Dns.GetHostAddresses(dnsEndPoint);

            var ipAddr = ipAddresses[0];
            var socket = new Socket(ipAddr.AddressFamily, SocketType.Stream, ProtocolType.Tcp); ;
            try
            {
                socket.BeginConnect(new IPEndPoint(ipAddr, uri.Port), OnConnected, socket);
            }
            catch
            {
                socket.Close();
            }
        }

        private void OnConnected(IAsyncResult ar)
        {
            var socket = (Socket) ar.AsyncState;
            _client = new TcpClient(socket, 8 * 1024);
            _client.ReceiveAsync();
        }

        private Uri GetUriFromRequest()
        {
            var requestUri = _session.Request.RequestLine.Uri;
            var requestHost = _session.Request.Headers.Host;
            if (requestUri == "*")
            {
                return new Uri(requestHost, UriKind.Relative);
            }
            if (Uri.IsWellFormedUriString(requestUri, UriKind.Absolute))
            {
                return new Uri(requestUri, UriKind.Absolute);
            }
            if (Uri.IsWellFormedUriString(requestUri, UriKind.Relative))
            {
                return new Uri(new Uri(requestHost), requestUri);
            }
            throw new Exception();
        }
    }

    internal enum InputState
    {
        RequestLine,
        Headers,
    }

    internal enum LineState
    {
        None,
        LF,
        CR
    }

    internal class ClientHandler
    {
        private readonly Session _session;
        private readonly Socket _socket;
        private InputState _inputState;
        private readonly Stream _stream;
        private readonly HttpStreamReader _reader;

        public ClientHandler(Session session, Socket clientSocket)
        {
            _session = session;
            _socket = clientSocket;
            _stream = new ManualBufferedStream(new ConnectionStream(_connection), _session.BufferAllocator);
            _reader = new HttpStreamReader(_stream);
        }

        public void ReceiveEntityAsync()
        {
            while (! IsRequestComplete(_reader));
        }

        public Stream GetRequestStreamAsync(int contentLenght)
        {
            var result = new MemoryStream();
            var writer = new StreamWriter(result);
            var b = new char[contentLenght];
            _reader.ReadBlockAsync(b, 0, contentLenght);
            writer.WriteAsync(b);
            writer.FlushAsync();
            result.Seek(0, SeekOrigin.Begin);
            return result;
        }

        private bool IsRequestComplete(TextReader reader)
        {
            string line;

            try {
                line = reader.ReadLineAsync();
            } catch {
                _session.ErrorMessage = "Bad request";
                _session.ErrorStatus = 400;
                return true;
            }

            do {
                if (line == null)
                    break;
                if (line == "") {
                    if (_inputState == InputState.RequestLine)
                        continue;
                    return true;
                }

                if (_inputState == InputState.RequestLine) {
                    _session.Request.RequestLine = new RequestLine(line);
                    _inputState = InputState.Headers;
                } else {
                    try {
                        _session.Request.Headers.AddLine(line);
                    } catch (Exception e) {
                        _session.ErrorMessage = e.Message;
                        _session.ErrorStatus = 400;
                        return true;
                    }
                }

                if (_session.HaveError)
                    return true;

                try {
                    line = await reader.ReadLineAsync();
                } catch {
                    _session.ErrorMessage = "Bad request";
                    _session.ErrorStatus = 400;
                    return true;
                }
            } while (line != null);

            return false;
        }

        public void BuildAndReturnResponseAsync(int code, string description)
        {
            _session.Response.Headers = new HTTPResponseHeaders();
            _session.Response.StatusLine = new StatusLine(code.ToString(), description);
            _session.Response.Headers.Add("Date", DateTime.UtcNow.ToString("r"));
            _session.Response.Headers.Add("Content-Type", "text/html; charset=UTF-8");
            _session.Response.Headers.Add("Connection", "close");
            _session.Response.Headers.Add("Timestamp", DateTime.UtcNow.ToString("HH:mm:ss.fff"));
            await _session.ReturnResponse();
        }
    }

    internal class Request
    {
        private readonly Session _session;
        private readonly HttpRequestHeaders _headers = new HttpRequestHeaders();

        public RequestLine RequestLine { get; set; }

        public Request(Session session)
        {
            _session = session;
        }

        public HttpRequestHeaders Headers
        {
            get { return _headers; }
        }

        public Stream GetContentStreamAsync()
        {
            var contentLenght = _session.Request.Headers.ContentLength;
            if( !contentLenght.HasValue || contentLenght.Value == 0) return new MemoryStream(0);
            return await _session.ClientHandler.GetRequestStreamAsync(contentLenght.Value);
        }
    }

    public class RequestLine
    {
        public string Verb { get; private set; }
        public string Uri { get; private set; }
        public string Version { get; private set; }

        public RequestLine(string line)
        {
            var ifs = line.IndexOf(' ');
            var ils = line.LastIndexOf(' ');
            Verb = line.Substring(0, ifs);
            Uri = line.Substring(ifs + 1, ils - ifs-1);
            Version = line.Substring(ils + 1);
        }
    }

}
