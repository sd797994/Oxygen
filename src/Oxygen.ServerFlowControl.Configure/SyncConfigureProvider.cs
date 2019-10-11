using Orleans.Runtime;
using Oxygen.IServerFlowControl.Configure;
using Oxygen.ThreadSyncGenerator;
using Oxygen.ThreadSyncGenerator.Grains;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Oxygen.ServerFlowControl.Configure
{
    /// <summary>
    /// 同步配置节提供类
    /// </summary>
    public class SyncConfigureProvider : ISyncConfigureProvider
    {
        /// <summary>
        /// 获取同步配置节
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public async Task<ServiceConfigureInfo> GetConfigure(string key)
        {
            return await DoSync(key, async (grain) => await TryGetConfigure(grain));
        }
        async Task<ServiceConfigureInfo> TryGetConfigure(ISyncServiceFlowControlConfigureGrain grain)
        {
            try
            {
                return await grain.GetConfigure();
            }
            catch (Exception)
            {
                return null;
            }
        }
        /// <summary>
        /// 设置同步配置节
        /// </summary>
        /// <param name="key"></param>
        /// <param name="newConfigure"></param>
        /// <returns></returns>
        public async Task SetConfigure(string key, ServiceConfigureInfo newConfigure)
        {
            await DoSync(key, false, async (grain) => await TrySetConfigure(grain, newConfigure));
        }
        async Task TrySetConfigure(ISyncServiceFlowControlConfigureGrain grain, ServiceConfigureInfo newConfigure)
        {
            try
            {
                await grain.SetConfigure(newConfigure);
            }
            catch (Exception)
            {

            }
        }
        /// <summary>
        /// 初始化同步配置节
        /// </summary>
        /// <param name="key"></param>
        /// <param name="newConfigure"></param>
        /// <returns></returns>
        public async Task InitConfigure(string key, ServiceConfigureInfo newConfigure)
        {
            await DoSync(key, true, async (grain) => await TrySetConfigure(grain,newConfigure));
        }
        static Lazy<ConcurrentDictionary<string, FlowControlConfigureObserver>> observers = new Lazy<ConcurrentDictionary<string, FlowControlConfigureObserver>>(() => new ConcurrentDictionary<string, FlowControlConfigureObserver>());
        public async Task RegisterConfigureObserver(string key)
        {
            if (!observers.Value.TryGetValue(key, out FlowControlConfigureObserver observer))
            {
                await DoSync(key, true, async (grain) =>
                {
                    observer = new FlowControlConfigureObserver();
                    var reference = await (await OrleanClientProvider.GetClient()).CreateObjectReference<IFlowControlConfigureObserver>(observer);
                    if (await grain.RegisterObserver(reference))
                    {
                        observers.Value.TryAdd(key, observer);
                    }
                });
            }
        }
        #region 私有方法
        /// <summary>
        /// 执行回调函数
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="func"></param>
        /// <returns></returns>
        async Task<T> DoSync<T>(string key, Func<ISyncServiceFlowControlConfigureGrain, Task<T>> func)
        {
            try
            {
                var grain = (await OrleanClientProvider.GetGrain(key));
                if (grain != null)
                {
                    return await func(grain);
                }
                return default;
            }
            catch (Exception)
            {
                return default;
            }
        }
        /// <summary>
        /// 执行函数
        /// </summary>
        /// <param name="key"></param>
        /// <param name="loopCreateGrain"></param>
        /// <param name="func"></param>
        /// <returns></returns>
        async Task DoSync(string key, bool loopCreateGrain = false, Action<ISyncServiceFlowControlConfigureGrain> func = null)
        {
            try
            {
                ISyncServiceFlowControlConfigureGrain grain = default;
                if (loopCreateGrain)
                {
                    while (loopCreateGrain)
                    {
                        grain = (await OrleanClientProvider.GetGrain(key, true));
                        if (grain != null)
                        {
                            break;
                        }
                        Thread.Sleep(500);
                    }
                }
                else
                {
                    grain = (await OrleanClientProvider.GetGrain(key));
                }
                if (grain != null)
                {
                    func(grain);
                }
            }
            catch (Exception)
            {
                return;
            }
        }
        #endregion
    }
}
