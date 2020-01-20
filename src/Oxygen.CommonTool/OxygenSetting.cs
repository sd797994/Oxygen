﻿using Microsoft.Extensions.Configuration;
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
        }
        /// <summary>
        /// 服务端口
        /// </summary>
        public static int ServerPort;

        public static EnumProtocolType ProtocolType;
    }
}
