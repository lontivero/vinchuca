using System.Net.Sockets;
using Vinchuca.Utils;

namespace Vinchuca.Network
{
    public static class Sockets
    {
        internal static readonly BlockingPool<SocketAsyncEventArgs> ConnectSaeaPool =
            new BlockingPool<SocketAsyncEventArgs>(() => new SocketAsyncEventArgs());
    }
}