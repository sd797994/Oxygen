﻿using Oxygen.IRpcProviderService;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Oxygen.ServerProxyFactory
{
    /// <summary>
    /// 本地管道AOP提供者
    /// </summary>
    public class LocalMethodAopProvider
    {
        static Func<object, Task> BeforeFunc;
        static Func<object, Task> AfterFunc;
        static Func<Exception, Task<object>> ExceptionFunc;
        /// <summary>
        /// 为管道注册匿名委托
        /// </summary>
        /// <param name="beforeFunc"></param>
        /// <param name="afterFunc"></param>
        /// <param name="exceptionFunc"></param>
        public static void RegisterPipelineHandler(Func<object, Task> beforeFunc = null, Func<object, Task> afterFunc = null, Func<Exception, Task<object>> exceptionFunc = null)
        {
            if (beforeFunc != null)
                BeforeFunc = beforeFunc;
            if (afterFunc != null)
                AfterFunc = afterFunc;
            if (exceptionFunc != null)
                ExceptionFunc = exceptionFunc;
        }
        /// <summary>
        /// 调用方法前后异常匿名委托
        /// </summary>
        /// <typeparam name="Tin"></typeparam>
        /// <typeparam name="Tout"></typeparam>
        /// <param name="param"></param>
        /// <param name="method"></param>
        /// <returns></returns>
        public static async Task<object> UsePipelineHandler<Tin, Tout>(Tin param, Func<Tin, Task<Tout>> method) where Tout : class
        {
            try
            {
                Tout result = default;
                if (BeforeFunc != null)
                    await BeforeFunc(param);
                result = await method(param);
                if (AfterFunc != null)
                    await AfterFunc(result);
                return result;
            }
            catch (Exception e)
            {
                if (ExceptionFunc != null)
                    return await ExceptionFunc(e);
            }
            return default;
        }
    }
}
