using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace Oxygen.IServerFlowControl
{
    public interface IEndPointConfigureManager
    {
        /// <summary>
        /// 获取服务配置节
        /// </summary>
        /// <param name="pathName"></param>
        /// <returns></returns>
        ServiceConfigureInfo GetOrAddBreakerConfigure(string pathName);

        /// <summary>
        /// 更新服务配置节
        /// </summary>
        /// <param name="pathName"></param>
        /// <param name="servcieInfo"></param>
        void UpdateBreakerConfigure(string pathName, ServiceConfigureInfo servcieInfo);

        /// <summary>
        /// 更新熔断结束的配置文件
        /// </summary>
        /// <param name="servcieInfo"></param>
        void CleanBreakTimes(ServiceConfigureInfo servcieInfo);

        /// <summary>
        /// 根据服务路由更新配置节
        /// </summary>
        /// <param name="serviceInfo"></param>
        /// <param name="addrs"></param>
        void ReflushConfigureEndPoint(ServiceConfigureInfo serviceInfo, List<FlowControlEndPoint> addrs);

        /// <summary>
        /// 通过负载均衡返回一个ip地址
        /// </summary>
        /// <param name="lbEndPoints"></param>
        /// <param name="clientIp"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        IPEndPoint GetServieByLoadBalane(List<FlowControlEndPoint> lbEndPoints, IPEndPoint clientIp, LoadBalanceType type = LoadBalanceType.IPHash);

        /// <summary>
        /// 修改最小连接数
        /// </summary>
        /// <param name="lbEndPoints"></param>
        /// <param name="address"></param>
        /// <param name="IsPlus"></param>
        void ChangeConnectCount(List<FlowControlEndPoint> lbEndPoints, IPEndPoint address, bool IsPlus);

    }
}
