using System;

namespace DreamBot.Network.Protocol.Peers
{
    public class DesparateModeActivatedEventArgs : EventArgs
    {
        private readonly BotIdentifier[] _bots;

        public DesparateModeActivatedEventArgs(BotIdentifier[] bots)
        {
            _bots = bots;
        }

        public BotIdentifier[] Bots
        {
            get { return _bots; }
        }
    }
}
