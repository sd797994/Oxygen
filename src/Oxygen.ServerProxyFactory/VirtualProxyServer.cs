using Oxygen.IServerProxyFactory;
using System;
using System.Reflection;
using System.Threading.Tasks;

namespace Oxygen.ServerProxyFactory
{
    /// <summary>
    /// 虚拟代理类
    /// </summary>
    public class VirtualProxyServer: IVirtualProxyServer
    {
        private readonly IRemoteProxyGenerator _proxyGenerator;

        public VirtualProxyServer(IRemoteProxyGenerator proxyGenerator)
        {
            _proxyGenerator = proxyGenerator;
        }
        /// <summary>
        /// 初始化代理
        /// </summary>
        /// <param name="serverName"></param>
        /// <param name="pathName"></param>
        /// <param name="flowControlCfgKey"></param>
        public void Init(string serverName, string pathName, Type inputType, Type returnType)
        {
            ServerName = serverName;
            PathName = pathName;
            InputType = inputType;
            ReturnType = returnType;
        }
        public string ServerName { get; set; }
        public string PathName { get; set; }
        public Type InputType { get; set; }
        public Type ReturnType { get; set; }
        /// <summary>
        /// 通过虚拟代理发送请求
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public async Task<object> SendAsync(object input)
        {
            return await _proxyGenerator.SendObjAsync(input, ReturnType, ServerName, PathName);
        }
    }
}
