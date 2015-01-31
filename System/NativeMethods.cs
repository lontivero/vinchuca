using System.Runtime.InteropServices;

namespace DreamBot.System
{
    static class NativeMethods
    {
        [DllImport("kernel32")]
        public static extern bool IsDebuggerPresent();
    }
}
