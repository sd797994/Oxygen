using System.Threading.Tasks;

namespace Oxygen.IServerProxyFactory
{
    public interface IVirtualProxyServer
    {
        Task<object> SendAsync(object input);
    }
}
