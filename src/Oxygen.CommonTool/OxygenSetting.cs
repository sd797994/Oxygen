using Microsoft.Extensions.Configuration;
using System.Net;

namespace Oxygen.CommonTool
{
    /// <summary>
    /// Oxygen本地配置
    /// </summary>
    public class OxygenSetting
    {
        /// <summary>
        /// 断路设置缓存KEY
        /// </summary>
        public static readonly string BreakerSettingKey = "RPCBREAKERSETTINGKEY";
        /// <summary>
        /// 限流令牌桶设置KEY
        /// </summary>
        public static readonly string TokenLimitSettingKey = "RPCTOKENLIMITSETTINGKEY";
        public OxygenSetting(IConfiguration configuration)
        {
            ServerName = configuration["Oxygen:Consul:ServerName"];
            ConsulAddress = $"http://{configuration["Oxygen:Consul:Address"]}:{configuration["Oxygen:Consul:Port"]}";
            if (!string.IsNullOrWhiteSpace(configuration["Oxygen:Consul:ServerPort"]))
            {
                ServerPort = int.Parse(configuration["Oxygen:Consul:ServerPort"]);
            }
            RedisAddress = $"{configuration["Oxygen:Redis:Address"]}:{configuration["Oxygen:Redis:Port"]}";
            RedisAddressEndPoint = new IPEndPoint(IPAddress.Parse(configuration["Oxygen:Redis:Address"]), int.Parse(configuration["Oxygen:Redis:Port"]));
            RsaPublicKey = configuration["Oxygen:Secret:PubKey"];
            RsaPrivateKey = configuration["Oxygen:Secret:PrvKey"];
        }
        #region 注册中心相关
        /// <summary>
        /// 服务端口
        /// </summary>
        public static int? ServerPort;
        /// <summary>
        /// 服务名
        /// </summary>
        public static string ServerName;
        /// <summary>
        /// 注册中心地址
        /// </summary>
        public static string ConsulAddress;
        #endregion
        #region 缓存相关
        /// <summary>
        /// redis地址
        /// </summary>
        public static string RedisAddress;
        /// <summary>
        /// redis地址
        /// </summary>
        public static IPEndPoint RedisAddressEndPoint;
        #endregion
        #region 传输安全相关
        /// <summary>
        /// 数字签名公钥
        /// </summary>
        public static string RsaPublicKey;
        /// <summary>
        /// 数字签名私钥
        /// </summary>
        public static string RsaPrivateKey;
        #endregion
    }
}
