using System;

namespace Oxygen.IServerFlowControl.Configure
{
    /// <summary>
    /// 流控服务本地配置类型
    /// </summary>
    public class FlowContolConfiugreTypeInfo
    {
        public FlowContolConfiugreTypeInfo(Type classType, Type interfaceType)
        {
            InterFaceType = interfaceType;
            ClassType = classType;
        }
        /// <summary>
        /// 接口类类型
        /// </summary>
        public Type InterFaceType { get; set; }
        /// <summary>
        /// 实现类类型
        /// </summary>
        public Type ClassType { get; set; }
    }
}
