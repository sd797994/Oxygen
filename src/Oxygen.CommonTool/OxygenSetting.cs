using Microsoft.Extensions.Configuration;
using System.Net;

namespace Oxygen.CommonTool
{
    /// <summary>
    /// Oxygen本地配置
    /// </summary>
    public class OxygenSetting
    {
        public OxygenSetting(IConfiguration configuration)
        {
            if (!string.IsNullOrWhiteSpace(configuration["Oxygen:ServerPort"]))
            {
                ServerPort = int.Parse(configuration["Oxygen:ServerPort"]);
            }
            else
            {
                ServerPort = 80;
            }
        }
        #region 注册中心相关
        /// <summary>
        /// 服务端口
        /// </summary>
        public static int ServerPort;
        #endregion
    }
}
