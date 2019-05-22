using Oxygen.IServerProxyFactory;
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
        public string ServerName { get; set; }
        public string PathName { get; set; }

        /// <summary>
        /// 通过虚拟代理发送请求
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public async Task<object> SendAsync(object input)
        {
            return await _proxyGenerator.SendAsync<object, object>(input, ServerName, PathName);
        }
    }
}
