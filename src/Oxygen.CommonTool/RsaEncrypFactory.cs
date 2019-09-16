using System;
using System.IO;
using System.Security.Cryptography;

namespace Oxygen.CommonTool
{
    /// <summary>
    /// 数字签名类型工厂
    /// </summary>
    public class RsaEncrypFactory
    {
        private static Lazy<RSA> PubProvider = new Lazy<RSA>(() =>
        {
            if (!string.IsNullOrEmpty(OxygenSetting.RsaPublicKey))
            {
                var _pubProvider = RSA.Create();
                _pubProvider.ImportParameters(ConvertFromPublicKey(OxygenSetting.RsaPublicKey));
                return _pubProvider;
            }
            return null;
        });
        private static Lazy<RSA> PrvProvider = new Lazy<RSA>(() =>
        {
            if (!string.IsNullOrEmpty(OxygenSetting.RsaPrivateKey))
            {
                var _prvProvider = RSA.Create();
                _prvProvider.ImportParameters(ConvertFromPrivateKey(OxygenSetting.RsaPrivateKey));
                return _prvProvider;
            }
            return null;
        });
        /// <summary>
        /// 创建签名程序
        /// </summary>
        /// <returns></returns>
        public static RSA CreateEncrypProvider()
        {
            return PubProvider.Value;
        }
        /// <summary>
        /// 创建验证程序
        /// </summary>
        /// <returns></returns>
        public static RSA CreateDecryptProvider()
        {
            return PrvProvider.Value;
        }
        #region 私有方法
        static RSAParameters ConvertFromPublicKey(string pemFileConent)
        {
            byte[] seqOid = { 0x30, 0x0D, 0x06, 0x09, 0x2A, 0x86, 0x48, 0x86, 0xF7, 0x0D, 0x01, 0x01, 0x01, 0x05, 0x00 };
            byte[] seq = new byte[15];
            var x509Key = Convert.FromBase64String(pemFileConent);
            byte[] modulus;
            byte[] exponent;
            using (MemoryStream mem = new MemoryStream(x509Key))
            {
                using (BinaryReader binr = new BinaryReader(mem))
                {
                    byte bt = 0;
                    ushort twobytes = 0;
                    twobytes = binr.ReadUInt16();
                    if (twobytes == 0x8130) 
                        binr.ReadByte();
                    else if (twobytes == 0x8230)
                        binr.ReadInt16(); 
                    else
                        throw new NullReferenceException();

                    seq = binr.ReadBytes(15);
                    if (!CompareBytearrays(seq, seqOid))
                        throw new NullReferenceException();

                    twobytes = binr.ReadUInt16();
                    if (twobytes == 0x8103)
                        binr.ReadByte();
                    else if (twobytes == 0x8203)
                        binr.ReadInt16(); 
                    else
                        throw new NullReferenceException();

                    bt = binr.ReadByte();
                    if (bt != 0x00)
                        throw new NullReferenceException();

                    twobytes = binr.ReadUInt16();
                    if (twobytes == 0x8130)
                        binr.ReadByte(); 
                    else if (twobytes == 0x8230)
                        binr.ReadInt16();
                    else
                        throw new NullReferenceException();

                    twobytes = binr.ReadUInt16();
                    byte lowbyte = 0x00;
                    byte highbyte = 0x00;

                    if (twobytes == 0x8102)
                        lowbyte = binr.ReadByte();
                    else if (twobytes == 0x8202)
                    {
                        highbyte = binr.ReadByte();
                        lowbyte = binr.ReadByte();
                    }
                    else
                        throw new NullReferenceException();
                    byte[] modint = { lowbyte, highbyte, 0x00, 0x00 };
                    int modsize = BitConverter.ToInt32(modint, 0);

                    int firstbyte = binr.PeekChar();
                    if (firstbyte == 0x00)
                    {
                        binr.ReadByte(); 
                        modsize -= 1; 
                    }
                    modulus = binr.ReadBytes(modsize);
                    if (binr.ReadByte() != 0x02)
                        throw new NullReferenceException();
                    int expbytes = (int)binr.ReadByte(); 
                    exponent = binr.ReadBytes(expbytes);
                }
            }
            return new RSAParameters
            {
                Modulus = modulus,
                Exponent = exponent
            };
        }
        static RSAParameters ConvertFromPrivateKey(string pemFileConent)
        {
            var rsaParameters = new RSAParameters();
            using (BinaryReader binr = new BinaryReader(new MemoryStream(Convert.FromBase64String(pemFileConent))))
            {
                byte bt = 0;
                ushort twobytes = 0;
                twobytes = binr.ReadUInt16();
                if (twobytes == 0x8130)
                    binr.ReadByte();
                else if (twobytes == 0x8230)
                    binr.ReadInt16();
                else
                    throw new Exception("Unexpected value read binr.ReadUInt16()");

                twobytes = binr.ReadUInt16();
                if (twobytes != 0x0102)
                    throw new Exception("Unexpected version");

                bt = binr.ReadByte();
                if (bt != 0x00)
                    throw new Exception("Unexpected value read binr.ReadByte()");

                rsaParameters.Modulus = binr.ReadBytes(GetIntegerSize(binr));
                rsaParameters.Exponent = binr.ReadBytes(GetIntegerSize(binr));
                rsaParameters.D = binr.ReadBytes(GetIntegerSize(binr));
                rsaParameters.P = binr.ReadBytes(GetIntegerSize(binr));
                rsaParameters.Q = binr.ReadBytes(GetIntegerSize(binr));
                rsaParameters.DP = binr.ReadBytes(GetIntegerSize(binr));
                rsaParameters.DQ = binr.ReadBytes(GetIntegerSize(binr));
                rsaParameters.InverseQ = binr.ReadBytes(GetIntegerSize(binr));
            }
            return rsaParameters;
        }
        static int GetIntegerSize(BinaryReader binr)
        {
            byte bt = 0;
            int count = 0;
            bt = binr.ReadByte();
            if (bt != 0x02)
                return 0;
            bt = binr.ReadByte();

            if (bt == 0x81)
                count = binr.ReadByte();
            else
            if (bt == 0x82)
            {
                var highbyte = binr.ReadByte();
                var lowbyte = binr.ReadByte();
                byte[] modint = { lowbyte, highbyte, 0x00, 0x00 };
                count = BitConverter.ToInt32(modint, 0);
            }
            else
            {
                count = bt;
            }

            while (binr.ReadByte() == 0x00)
            {
                count -= 1;
            }
            binr.BaseStream.Seek(-1, SeekOrigin.Current);
            return count;
        }
        static bool CompareBytearrays(byte[] a, byte[] b)
        {
            if (a.Length != b.Length)
                return false;
            int i = 0;
            foreach (byte c in a)
            {
                if (c != b[i])
                    return false;
                i++;
            }
            return true;
        }
        #endregion
    }
}
