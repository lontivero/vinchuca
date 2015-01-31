using System;
using System.IO;
using DreamBot.Crypto;

namespace DreamBot.Network.Protocol.Messages.Command
{
    class CommandMessage : Message
    {
        public Guid Nonce { get; set; }

        public override void Decode(BinaryReader br)
        {
            base.Decode(br);
            br.BaseStream.Seek(BotHeader.Size, SeekOrigin.Begin);
            var signedPayload = br.ReadBytes(10);
            var sign = new Signature();
            sign.Verify(signedPayload);
        }

        public void xxx()
        {
            var privKeyStr = 
@"MIICXQIBAAKBgQDEuMdX757iaBkrxN51IQMILf+o7nJhfddEQ8gurVNYgoGxg19N
ZtrJaegAiv6lbFO4jhkKzLX9mHXZrvxL+UD5lr458BA8vwm+ws7lxu+10exq0Xns
H26ekvBveDcbndhX+fbc34HhZlJCib6MKMzUGebwckGbZK3oz4WCZc3QoQIDAQAB
AoGBALBWWgpPNwMIARgk3qvrrYYVhYhuRYVyghYlFDoTEBTU12DBvBnrys6k6lwi
O+UY12slpPjzS2dI1MsOegW4Ji5/6w/rZC8BoyvOADSVWKc71eaQ8d9IqjjWKp0+
cL9hgI8s/DDYUtqmrqsxqPGkxOx8NSD7VcSID7xbbqK/v5uJAkEA+hurMyJUPRCD
Uyfie1m+cfQKjc4j4zN32KrbceG25QJq8Y/lpQSq0POSsmvaIxGkRJtTIoB6T/co
TYvox/kUiwJBAMlbJvyjm2KY12CTFjN7apmnMIRoYmI9z8n4hLl2h8852s+Ljdjn
MrCOcsL0T0kvxLlAKDwAeVWtjil3A9/DGQMCQAJPERSGw5pQtbWlz5xt5qkspJBM
j95AEmIoqZ/ygnq4u/4A4xDT6zPEm90Ty865EfgkKu9NmlN0p6WXng2CiiMCQEND
nQmGihDs5/4OBLub/edob4+74ynYZkKdL5FZJFM4i30LrI4J5egPHg08WgQj3f7Y
jNhGfEH/4V6+sF+eqAsCQQCTl/A2JqF6CThIdk6zLF40cJGRFOkAcRqsrV3Q64XD
C2rpXXqnjJlqysIFEmnUmxm64ckMyg96b1CxeW0F4Tr7";
            var privKey = Convert.FromBase64String(privKeyStr);
            var prvRsa = RsaUtils.LoadPrivateKey(privKey);
        }
    }
}
