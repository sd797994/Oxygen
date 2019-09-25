using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Oxygen.IServerFlowControl.Configure
{
    /// <summary>
    /// 同步配置节提供类
    /// </summary>
    public interface ISyncConfigureProvider
    {
        /// <summary>
        /// 获取同步配置节
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        Task<ServiceConfigureInfo> GetConfigure(string key);
        /// <summary>
        /// 设置同步配置节
        /// </summary>
        /// <param name="key"></param>
        /// <param name="newConfigure"></param>
        /// <returns></returns>
        Task SetConfigure(string key, ServiceConfigureInfo newConfigure);
        /// <summary>
        /// 初始化同步配置节
        /// </summary>
        /// <param name="key"></param>
        /// <param name="newConfigure"></param>
        /// <returns></returns>
        Task InitConfigure(string key, ServiceConfigureInfo newConfigure);
        /// <summary>
        /// 初始化客户端订阅配置节
        /// </summary>
        /// <param name="key"></param>
        /// <param name="newConfigure"></param>
        /// <returns></returns>
        Task RegisterConfigureObserver(string key);
    }
}
