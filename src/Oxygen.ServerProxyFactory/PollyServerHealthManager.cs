using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Oxygen.IServerProxyFactory;

namespace Oxygen.ServerProxyFactory
{
    /// <summary>
    /// 服务治理管理器类
    /// </summary>
    public class PollyServerHealthManager : IServerHealthManager
    {
        /// <summary>
        /// 通过服务治理策略返回远程结果集
        /// </summary>
        /// <typeparam name="TIn"></typeparam>
        /// <typeparam name="TOut"></typeparam>
        /// <param name="input"></param>
        /// <returns></returns>
        public async Task<TOut> GetResultByHealthNode<TIn, TOut>(Action<TIn> input)
        {
            return default(TOut);
        }
    }
}
