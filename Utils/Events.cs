using System;

namespace DreamBot.Utils
{
    internal static class Events
    {
        internal static void Raise<T>(EventHandler<T> handler, object sender, T args) where T : EventArgs
        {
            if (handler != null)
            {
                handler(sender, args);
            }
        }
    }
}