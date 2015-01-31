using System;
using System.Net;
using DreamBot.System;

namespace DreamBot.Network.Protocol.Peers
{
    public class PeerInfo
    {
        public PeerInfo(BotIdentifier botId, IPEndPoint endpoint)
        {
            BotId = botId;
            EndPoint = endpoint;
            LastSeen = DateTimeProvider.UtcNow;
        }
        public BotIdentifier BotId { get; private set; }
        public IPEndPoint EndPoint { get; set; }
        public DateTime LastSeen { get; set; }
        public int Reputation { get; private set; }

        private TimeSpan InactiveFor
        {
            get { return DateTimeProvider.UtcNow - LastSeen; }
        }

        public bool IsUnknownBot
        {
            get { return Reputation < -10; }
        }

        public bool IsLazyBot
        {
            get { return InactiveFor > TimeSpan.FromMinutes(30); }
        }

        internal void Touch()
        {
            LastSeen = DateTimeProvider.UtcNow;
        }

        public void DecreseReputation()
        {
            Reputation--;
        }

        public override string ToString()
        {
            return string.Format("{0}@{1}", BotId, EndPoint);
        }

        public static PeerInfo Parse(string line)
        {
            var parts = line.Split(new[] {'@', ':'});
            var id = BotIdentifier.Parse(parts[0]);
            var ip = IPAddress.Parse(parts[1]);
            var port = int.Parse(parts[2]);

            return new PeerInfo(id, new IPEndPoint(ip, port));
        }
    }
}