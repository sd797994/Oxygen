using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Linq;
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
            if (!string.IsNullOrWhiteSpace(configuration["Oxygen:ProtocolType"]))
            {
                ProtocolType = (EnumProtocolType)int.Parse(configuration["Oxygen:ProtocolType"]);
            }
            else
            {
                ProtocolType = EnumProtocolType.TCP;
            }
            if (!string.IsNullOrWhiteSpace(configuration["Oxygen:CustomHeader"]))
            {
                CustomHeader = configuration["Oxygen:CustomHeader"].Split(",").ToList();
            }
            else
            {
                CustomHeader = new List<string>();
            }
        }
        /// <summary>
        /// 服务端口
        /// </summary>
        public static int ServerPort;

        public static EnumProtocolType ProtocolType;

        public static List<string> CustomHeader;
    }
}
