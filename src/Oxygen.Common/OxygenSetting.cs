using Microsoft.Extensions.Configuration;

namespace Oxygen.Common
{
    public static class OxygenSetting
    {
        public static void Init(IConfiguration configuration)
        {
            ConsulServerName = configuration["Oxygen:ConsulServerName"];
            Consul = configuration["Oxygen:Consul"];
            MappingPort = int.Parse(configuration["Oxygen:MappingPort"]);
            ServerPort = int.Parse(configuration["Oxygen:ServerPort"]);
        }
        /// <summary>
        /// 映射端口
        /// </summary>
        public static int MappingPort { get; set; }
        /// <summary>
        /// 服务端口
        /// </summary>
        public static int ServerPort { get; set; }
        /// <summary>
        /// 服务名
        /// </summary>
        public static string ConsulServerName { get; set; }
        /// <summary>
        /// 注册中心地址
        /// </summary>
        public static string Consul { get; set; }
    }
}
