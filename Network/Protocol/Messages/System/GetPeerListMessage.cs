namespace DreamBot.Network.Protocol.Messages.System
{
    public class GetPeerListMessage : Message
    {
    }

    public enum MessageType
    {
        Request,
        Reply,
        System
    }
}