using System;
using System.Net.Sockets;

namespace Vinchuca.Network.Listeners
{
    internal class NewConnectionEventArgs : EventArgs
    {
        public NewConnectionEventArgs(Socket socket)
        {
            Socket = socket;
        }

        internal Socket Socket { get; set; }
    }
}