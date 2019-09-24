using System.Threading.Tasks;

namespace Oxygen.IServerFlowControl
{
    /// <summary>
    /// 令牌桶接口
    /// </summary>
    public interface ITokenBucket
    {
        /// <summary>
        /// 初始化桶
        /// </summary>
        /// <param name="capacity"></param>
        /// <param name="rate"></param>
        void InitTokenBucket(long capacity, long rate);
        /// <summary>
        /// 检查令牌是否充足
        /// </summary>
        /// <param name="pathName"></param>
        /// <param name="serviceInfo"></param>
        /// <returns></returns>
        Task<bool> Grant(string flowControlCfgKey, int defCapacity);
    }
}
