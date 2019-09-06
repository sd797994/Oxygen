using Oxygen.CommonTool.Logger;
using Oxygen.IServerFlowControl;
using Oxygen.IServerRegisterManage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Oxygen.ServerFlowControl
{

    /// <summary>
    /// 流控中心
    /// </summary>
    public class FlowControlCenter: IFlowControlCenter
    {
        private readonly IRegisterCenter _registerCenter;
        private readonly IEndPointConfigureManager _endPointConfigure;
        private readonly IEndPointCircuitBreaker _breaker;
        private readonly ICircuitPolicyProvider _policyProvider;
        private readonly IOxygenLogger _logger;
        public FlowControlCenter(IRegisterCenter registerCenter, IEndPointConfigureManager endPointConfigure,
            IEndPointCircuitBreaker breaker, ICircuitPolicyProvider policyProvider, IOxygenLogger logger)
        {
            _registerCenter = registerCenter;
            _endPointConfigure = endPointConfigure;
            _breaker = breaker;
            _policyProvider = policyProvider;
            _logger = logger;
        }

        /// <summary>
        /// 根据服务名返回IP地址
        /// </summary>
        /// <param name="serviceName"></param>
        /// <param name="path"></param>
        /// <param name="clientIp"></param>
        /// <returns></returns>
        public async Task<IPEndPoint> GetFlowControlEndPointByServicePath(string serviceName, string key, IPEndPoint clientIp)
        {
            //根据服务返回健康地址
            var healthNode = await _registerCenter.GetServieByName(serviceName);
            //根据服务返回流控配置节
            var flowcontrolSetting = _endPointConfigure.GetOrAddBreakerConfigure(key);
            if (healthNode != null && healthNode.Any())
            {
                if (flowcontrolSetting == null)
                {
                    //如果当前服务并未配置流控，则直接负载均衡返回节点
                    return _endPointConfigure.GetServieByLoadBalane(healthNode, clientIp, LoadBalanceType.IPHash);
                }
                else
                {
                    //更新健康节点和缓存同步
                    _endPointConfigure.ReflushConfigureEndPoint(flowcontrolSetting, healthNode);
                    //若配置流控，则进行熔断和限流检测
                    if (_breaker.CheckCircuitByEndPoint(key, clientIp, flowcontrolSetting, out IPEndPoint point))
                    {
                        //将有效地址的熔断数据清空
                        _endPointConfigure.CleanBreakTimes(flowcontrolSetting);
                        _endPointConfigure.UpdateBreakerConfigure(key, flowcontrolSetting);
                        return point;
                    }
                }
            }
            else
            {
                //删除所有地址并同步
                if (flowcontrolSetting != null)
                {
                    flowcontrolSetting.EndPoints = null;
                    _endPointConfigure.UpdateBreakerConfigure(key, flowcontrolSetting);
                }
                _logger.LogError($"没有找到健康的服务节点：{serviceName}");
            }
            return null;
        }

        /// <summary>
        /// 根据断路策略执行远程调用
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="serviceName"></param>
        /// <param name="path"></param>
        /// <param name="endPoint"></param>
        /// <param name="func"></param>
        /// <returns></returns>
        public async Task<T> ExcuteAsync<T>(string key, IPEndPoint endPoint, Func<Task<T>> func) where T :class
        {
            var flowcontrolSetting = _endPointConfigure.GetOrAddBreakerConfigure(key);
            if (flowcontrolSetting == null)
            {
                //如果当前服务并未配置流控，则直接执行函数
                return await func.Invoke();
            }
            else
            {
                //构造断路策略
                var policy = _policyProvider.BuildPolicy<T>(key, flowcontrolSetting, endPoint);
                //启动polly进行调用检查
                try
                {
                    return await policy.ExecuteAsync(async () =>
                    {
                        //获取远程调用结果
                        var result = await func.Invoke();
                        //更新请求时间(用于限流)
                        _policyProvider.PushTimeInReq(key, endPoint);
                        //更新连接数(用于负载均衡)
                        _endPointConfigure.ChangeConnectCount(flowcontrolSetting.EndPoints, endPoint, false);
                        //更新缓存（用于缓存降级）
                        flowcontrolSetting.ReflushCache(endPoint, result);
                        return result;
                    }) as T;
                }
                catch (Exception e)
                {

                }
            }
            return default;
        }
    }
}
