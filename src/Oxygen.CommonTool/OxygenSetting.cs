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
            var mesh = new MeshInfo(configuration);
            ProtocolType = mesh.Info.ProtocolType;
            MeshType = mesh.Info.MeshType;
            switch (MeshType)
            {
                case EnumMeshType.None:
                default:
                    break;
                case EnumMeshType.Dapr:
                    OpenActor = ((DaprMeshInfo)mesh.Info).OpenActor;
                    break;
                case EnumMeshType.Istio:
                    CustomHeader = ((IstioMeshInfo)mesh.Info).CustomHeader;
                    break;
            }
        }
        /// <summary>
        /// 服务端口
        /// </summary>
        public static int ServerPort { get; set; } = 80;
        /// <summary>
        /// 协议0 = tcp 1 = http1.1 2 = http/2
        /// </summary>
        public static EnumProtocolType ProtocolType { get; set; } = EnumProtocolType.TCP;
        /// <summary>
        /// 追踪头（金丝雀）
        /// </summary>
        public static List<string> CustomHeader { get; set; } = new List<string>();
        /// <summary>
        /// 网格类型none istio dapr
        /// </summary>
        public static EnumMeshType MeshType { get; set; } = EnumMeshType.None;
        /// <summary>
        /// 开启actor(仅限于dapr网格)
        /// </summary>
        public static bool OpenActor { get; set; } = false;
    }
    class MeshInfo
    {
        internal MeshInfo(IConfiguration configuration)
        {
            if (bool.TryParse(configuration["Oxygen:Mesh:None:Open"], out bool noneOpen) && noneOpen)
            {
                Info = new NoneMeshInfo(configuration);
            }
            else if (bool.TryParse(configuration["Oxygen:Mesh:Dapr:Open"], out bool daprOpen) && daprOpen)
            {
                Info = new DaprMeshInfo(configuration);
            }
            else if (bool.TryParse(configuration["Oxygen:Mesh:Istio:Open"], out bool istioOpen) && istioOpen)
            {
                Info = new IstioMeshInfo(configuration);
            }
        }
        internal MeshBase Info { get; set; }
    }
    class MeshBase
    {
        internal EnumMeshType MeshType;
        internal EnumProtocolType ProtocolType;
    }
    class NoneMeshInfo : MeshBase
    {
        internal NoneMeshInfo(IConfiguration configuration)
        {
            base.MeshType = EnumMeshType.None;
            if (int.TryParse(configuration["Oxygen:Mesh:None:ProtocolType"], out int Protocol))
            {
                base.ProtocolType = (EnumProtocolType)Protocol;
            }
        }
    }
    class DaprMeshInfo : MeshBase
    {
        internal DaprMeshInfo(IConfiguration configuration)
        {
            base.MeshType = EnumMeshType.Dapr;
            if (int.TryParse(configuration["Oxygen:Mesh:Dapr:ProtocolType"], out int Protocol))
            {
                base.ProtocolType = (EnumProtocolType)Protocol;
            }
            if (bool.TryParse(configuration["Oxygen:Mesh:Dapr:OpenActor"], out bool OpenActor))
            {
                this.OpenActor = OpenActor;
            }
        }
        internal bool OpenActor { get; set; }
    }
    class IstioMeshInfo : MeshBase
    {
        internal IstioMeshInfo(IConfiguration configuration)
        {
            base.MeshType = EnumMeshType.Istio;
            if (int.TryParse(configuration["Oxygen:Mesh:Istio:ProtocolType"], out int Protocol))
            {
                base.ProtocolType = (EnumProtocolType)Protocol;
            }
            if (!string.IsNullOrWhiteSpace(configuration["Oxygen:Mesh:Istio:CustomHeader"]))
            {
                CustomHeader = configuration["Oxygen:Mesh:Istio:CustomHeader"].Split(',').ToList();
            }
        }
        internal List<string> CustomHeader;
    }
}