using DreamBot.System;
using Microsoft.Win32;

namespace DreamBot.Utils
{
    public class RegistryUtils
    {
        private static RegistryKey OpenKey(string key, bool writeable, out string kval)
        {
            var ios = key.LastIndexOfAny(new[] { '\\' });
            var kkey = key.Substring(0, ios);
            kval = key.Substring(ios + 1, key.Length - ios - 1);
            RegistryKey baseReg;

            if (key.StartsWith("HKLM"))
            {
                baseReg = Registry.LocalMachine;
                kkey = kkey.Substring(5);
            }
            else if (key.StartsWith("HKCU"))
            {
                baseReg = Registry.CurrentUser;
                kkey = kkey.Substring(5);
            }
            else
            {
                baseReg = SystemInfo.IsAdministrator
                              ? Registry.LocalMachine
                              : Registry.CurrentUser;
            }

            return baseReg.OpenSubKey(kkey, writeable);
        }

        public static void Write(string key, object value)
        {
            try
            {
                string kval;
                using (var regKey = OpenKey(key, true, out kval))
                {
                    regKey.SetValue(kval, value);
                }
            }
            catch
            {
            }
        }

        public static string Read(string key)
        {
            string kval;
            using (var regKey = OpenKey(key, false, out kval))
            {
                return "" + regKey.GetValue(kval);
            }
        }
    }
}
