using System.Net.Sockets;
using DreamBot.Utils;

namespace DreamBot.Network
{
    public static class Sockets
    {
        internal static readonly BlockingPool<SocketAsyncEventArgs> ConnectSaeaPool =
            new BlockingPool<SocketAsyncEventArgs>(() => new SocketAsyncEventArgs());
    }
}