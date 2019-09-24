using Oxygen.CommonTool;
using Oxygen.CommonTool.Logger;
using Oxygen.IServerFlowControl;
using Oxygen.IServerFlowControl.Configure;
using Oxygen.IServerRegisterManage;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace Oxygen.ServerFlowControl
{

    /// <summary>
    /// 流控中心
    /// </summary>
    public class FlowControlCenter : IFlowControlCenter
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
        /// <param name="flowControlCfgKey"></param>
        /// <param name="clientIp"></param>
        /// <returns></returns>
        public async Task<IPEndPoint> GetFlowControlEndPointByServicePath(string serviceName, string flowControlCfgKey, IPEndPoint clientIp)
        {
            //根据服务返回健康地址
            var healthNode = await _registerCenter.GetServieByName(serviceName);
            var checkConfigAny = await _endPointConfigure.CheckBreakerConfigureAny(flowControlCfgKey);
            if (healthNode != null && healthNode.Any())
            {
                if (checkConfigAny)
                {
                    //更新健康节点和缓存同步
                    await _endPointConfigure.ReflushConfigureEndPoint(flowControlCfgKey, healthNode);
                    //若配置流控，则进行熔断和限流检测
                    var point = await _breaker.CheckCircuitByEndPoint(flowControlCfgKey, clientIp);
                    if (point != null)
                    {
                        //将有效地址的熔断数据清空
                        await _endPointConfigure.CleanBreakTimes(flowControlCfgKey);
                        return point;
                    }
                }
                else
                {
                    //如果当前服务并未配置流控，则直接负载均衡返回节点
                    return _endPointConfigure.GetServieByLoadBalane(healthNode, clientIp, LoadBalanceType.IPHash);
                }
            }
            else
            {
                //删除所有地址并同步
                if (!checkConfigAny)
                {
                    await _endPointConfigure.RemoveAllNode(flowControlCfgKey);
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
        public async Task<T> ExcuteAsync<T>(string key, IPEndPoint endPoint, string flowControlCfgKey, Func<Task<T>> func) where T : class
        {
            var config = _endPointConfigure.GetBreakerConfigure(key);
            if (config == null)
            {
                //如果当前服务并未配置流控，则直接远程调用
                return await func();
            }
            else
            {
                //构造断路策略
                var policy = await _policyProvider.BuildPolicy<T>(key, endPoint);
                //启动polly进行调用检查
                try
                {
                    return await policy.ExecuteAsync(async () =>
                    {
                        //远程调用
                        var result = await func();
                        //消费结果集
                        AddQueueResult(new ResultQueueDto(key, endPoint, flowControlCfgKey, result));
                        return result;
                    }) as T;
                }
                catch (Exception e)
                {
                    //ignore
                }
            }
            return default;
        }


        /// <summary>
        /// 消费结果集
        /// </summary>
        public async Task RegisterConsumerResult()
        {
            while (true)
            {
                _event.Value.WaitOne();
                if (resultQueue.Value.TryDequeue(out ResultQueueDto dto))
                {
                    //更新请求时间(用于限流)
                    _policyProvider.PushTimeInReq(dto.Key, dto.EndPoint);
                    //更新连接数(用于负载均衡)
                    await _endPointConfigure.ChangeConnectCount(dto.FlowControlCfgKey, dto.EndPoint, false);
                    //更新缓存（用于缓存降级）
                    await _endPointConfigure.ReflushCache(dto.FlowControlCfgKey, dto.Result);
                    _event.Value.Set();
                }
            }
        }
        #region 私有方法
        static readonly Lazy<ConcurrentQueue<ResultQueueDto>> resultQueue = new Lazy<ConcurrentQueue<ResultQueueDto>>(() => new ConcurrentQueue<ResultQueueDto>());
        static readonly Lazy<EventWaitHandle> _event = new Lazy<EventWaitHandle>(() => new AutoResetEvent(false));
        /// <summary>
        /// 将结果集放入本地消费队列进行后续消费
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="endPoint"></param>
        /// <param name="flowControlCfgKey"></param>
        /// <param name="configureInfo"></param>
        /// <param name="result"></param>
        void AddQueueResult(ResultQueueDto result)
        {
            resultQueue.Value.Enqueue(result);
            _event.Value.Set();
        }
        #endregion
    }
}
