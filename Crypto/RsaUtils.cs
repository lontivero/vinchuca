using System;
using System.IO;
using System.Security.Cryptography;

namespace DreamBot.Crypto
{
    static class RsaUtils
    {
        public static RSACryptoServiceProvider LoadPublicKey(byte[] body)
        {
            byte[] modulus;
            byte[] exponent;
            using (var mem = new MemoryStream(body))
            {
                using (var binr = new BinaryReader(mem))
                {
                    var twobytes = binr.ReadUInt16();
                    switch (twobytes)
                    {
                        case 0x8130:
                            binr.ReadByte(); //advance 1 byte
                            break;
                        case 0x8230:
                            binr.ReadInt16(); //advance 2 bytes
                            break;
                        default:
                            return null;
                    }

                    binr.ReadBytes(15); //read the Sequence OID

                    twobytes = binr.ReadUInt16();
                    if (twobytes == 0x8103) //data read as little endian order (actual data order for Bit String is 03 81)
                        binr.ReadByte(); //advance 1 byte
                    else if (twobytes == 0x8203)
                        binr.ReadInt16(); //advance 2 bytes
                    else
                        return null;

                    var bt = binr.ReadByte();
                    if (bt != 0x00) //expect null byte next
                        return null;

                    twobytes = binr.ReadUInt16();
                    if (twobytes == 0x8130) //data read as little endian order (actual data order for Sequence is 30 81)
                        binr.ReadByte(); //advance 1 byte
                    else if (twobytes == 0x8230)
                        binr.ReadInt16(); //advance 2 bytes
                    else
                        return null;

                    twobytes = binr.ReadUInt16();
                    byte lowbyte;
                    byte highbyte = 0x00;

                    if (twobytes == 0x8102) //data read as little endian order (actual data order for Integer is 02 81)
                        lowbyte = binr.ReadByte(); // read next bytes which is bytes in modulus
                    else if (twobytes == 0x8202)
                    {
                        highbyte = binr.ReadByte(); //advance 2 bytes
                        lowbyte = binr.ReadByte();
                    }
                    else
                        return null;

                    byte[] modint = { lowbyte, highbyte, 0x00, 0x00 }; //reverse byte order since asn.1 key uses big endian order
                    var modsize = BitConverter.ToInt32(modint, 0);

                    var firstbyte = binr.PeekChar();
                    if (firstbyte == 0x00)
                    {
                        //if first byte (highest order) of modulus is zero, don't include it
                        binr.ReadByte(); //skip this null byte
                        modsize -= 1; //reduce modulus buffer size by 1
                    }

                    modulus = binr.ReadBytes(modsize);

                    if (binr.ReadByte() != 0x02) //expect an Integer for the exponent data
                        return null;

                    var expbytes = (int)binr.ReadByte(); // should only need one byte for actual exponent data (for all useful values)
                    exponent = binr.ReadBytes(expbytes);
                }
            }
            // ------- create RSACryptoServiceProvider instance and initialize with public key -----
            var rsa = new RSACryptoServiceProvider();
            var rsaKeyInfo = new RSAParameters { Modulus = modulus, Exponent = exponent };
            rsa.ImportParameters(rsaKeyInfo);

            return rsa;
        }

        public static RSACryptoServiceProvider LoadPrivateKey(byte[] body)
        {
            using (var ms = new MemoryStream(body))
            {
                using (var reader = new BinaryReader(ms))
                {
                    var tb = reader.ReadUInt16(); // LE: x30 x81      
                    switch (tb)
                    {
                        case 0x8130:
                            reader.ReadByte(); // fw 1      
                            break;
                        case 0x8230:
                            reader.ReadInt16(); // fw 2      
                            break;
                        default:
                            return null;
                    }

                    tb = reader.ReadUInt16(); // version     
                    if (tb != 0x0102)
                    {
                        return null;
                    }
                    if (reader.ReadByte() != 0x00)
                    {
                        return null;
                    }

                    var modulus = ReadInt(reader);
                    var e = ReadInt(reader);
                    var d = ReadInt(reader);
                    var p = ReadInt(reader);
                    var q = ReadInt(reader);
                    var dp = ReadInt(reader);
                    var dq = ReadInt(reader);
                    var iq = ReadInt(reader);

                    var result = new RSACryptoServiceProvider();
                    var param = new RSAParameters
                    {
                        Modulus = modulus,
                        Exponent = e,
                        D = d,
                        P = p,
                        Q = q,
                        DP = dp,
                        DQ = dq,
                        InverseQ = iq
                    };
                    result.ImportParameters(param);
                    return result;
                }
            }
        }

        private static byte[] ReadInt(BinaryReader r)
        {
            var s = GetIntSize(r);
            return r.ReadBytes(s);
        }

        private static int GetIntSize(BinaryReader r)
        {
            int c;
            var b = r.ReadByte();
            if (b != 0x02)
            { //int      
                return 0;
            }
            b = r.ReadByte();

            if (b == 0x81)
            {
                c = r.ReadByte(); //size      
            }
            else if (b == 0x82)
            {
                var hb = r.ReadByte();
                var lb = r.ReadByte();
                byte[] m = {lb, hb, 0x00, 0x00};
                c = BitConverter.ToInt32(m, 0);
            }
            else
            {
                c = b; //got size      
            }

            while (r.ReadByte() == 0x00)
            { //remove high zero      
                c -= 1;
            }
            r.BaseStream.Seek(-1, SeekOrigin.Current); // last byte is not zero, go back;      
            return c;
        }
    }
}
