namespace Oxygen.IServerFlowControl
{
    /// <summary>
    /// 负载均衡类型
    /// </summary>
    public enum LoadBalanceType
    {
        /// <summary>
        /// 轮询
        /// </summary>
        Polling = 0,
        /// <summary>
        /// 随机
        /// </summary>
        Random = 1,
        /// <summary>
        /// IP哈希
        /// </summary>
        IPHash = 2,
        /// <summary>
        /// 最小连接
        /// </summary>
        MinConnections = 3
    }
}
