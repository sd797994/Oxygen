using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Oxygen.IServerFlowControl
{
    /// <summary>
    /// 流控配置管理器
    /// </summary>
    public interface IRemoteFlowContolConfiugreManage
    {
        /// <summary>
        /// 初始化并更新配置节到缓存
        /// </summary>
        /// <param name="types"></param>
        void SetCacheFromServices();
    }
}
