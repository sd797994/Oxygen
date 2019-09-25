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
            return await DoSync(key, async (grain) => await grain.GetConfigure());
        }
        /// <summary>
        /// 设置同步配置节
        /// </summary>
        /// <param name="key"></param>
        /// <param name="newConfigure"></param>
        /// <returns></returns>
        public async Task SetConfigure(string key, ServiceConfigureInfo newConfigure)
        {
            await DoSync(key, false, async (grain) => await grain.SetConfigure(newConfigure));
        }
        /// <summary>
        /// 初始化同步配置节
        /// </summary>
        /// <param name="key"></param>
        /// <param name="newConfigure"></param>
        /// <returns></returns>
        public async Task InitConfigure(string key, ServiceConfigureInfo newConfigure)
        {
            await DoSync(key, true, async (grain) => await grain.SetConfigure(newConfigure));
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
            var grain = (await GetGrain(key));
            if (grain != null)
            {
                try
                {
                    return await func(grain);
                }
                catch (OrleansMessageRejectionException)
                {
                    grain = (await GetGrain(key, true));
                    if (grain != null)
                    {
                        return await func(grain);
                    }
                }
            }
            return default;
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
            ISyncServiceFlowControlConfigureGrain grain = default;
            if (loopCreateGrain)
            {
                while (loopCreateGrain)
                {
                    grain = (await GetGrain(key, true));
                    if (grain != null)
                    {
                        break;
                    }
                    Thread.Sleep(500);
                }
            }
            else
            {
                grain = (await GetGrain(key));
            }
            if (grain != null)
            {
                try
                {
                    func(grain);
                }
                catch (OrleansMessageRejectionException)
                {
                    grain = (await GetGrain(key, true));
                    if (grain != null)
                    {
                        func(grain);
                    }
                }
            }
        }
        /// <summary>
        /// 获取grain对象
        /// </summary>
        /// <param name="key"></param>
        /// <param name="reGetClient"></param>
        /// <returns></returns>
        async Task<ISyncServiceFlowControlConfigureGrain> GetGrain(string key, bool reGetClient = false)
        {
            try
            {
                var client = await OrleanClientProvider.GetClient(reGetClient);
                if (client != null)
                {
                    return client.GetGrain<ISyncServiceFlowControlConfigureGrain>(key);
                }
            }
            catch (Exception)
            {
                return null;
            }
            return null;
        }
        #endregion
    }
}
