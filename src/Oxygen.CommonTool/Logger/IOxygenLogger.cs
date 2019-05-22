namespace Oxygen.CommonTool.Logger
{
    /// <summary>
    /// 日志接口
    /// </summary>
    public interface IOxygenLogger
    {
        /// <summary>
        /// 异常日志
        /// </summary>
        /// <param name="message"></param>
        void LogError(string message);
        /// <summary>
        /// 信息日志
        /// </summary>
        /// <param name="message"></param>
        void LogInfo(string message);
    }
}
