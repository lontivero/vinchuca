using System;

namespace DreamBot.Network.Protocol.Messages
{
    public class InvalidMessage : Message
    {
        public Exception Exception { get; set; }

        public InvalidMessage(Exception exception) 
        {
            Exception = exception;
        }
    }
}