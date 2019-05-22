using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Oxygen.IServerProxyFactory
{
    /// <summary>
    /// 服务治理管理器接口
    /// </summary>
    public interface IServerHealthManager
    {
        /// <summary>
        /// 通过服务治理策略返回远程结果集
        /// </summary>
        /// <typeparam name="TIn"></typeparam>
        /// <typeparam name="TOut"></typeparam>
        /// <param name="input"></param>
        /// <returns></returns>
        Task<TOut> GetResultByHealthNode<TIn, TOut>(Action<TIn> input);
    }
}
