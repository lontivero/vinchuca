using System.Runtime.InteropServices;

namespace Vinchuca.System
{
    static class NativeMethods
    {
        [DllImport("kernel32")]
        public static extern bool IsDebuggerPresent();
    }
}
