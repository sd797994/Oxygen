using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using System;

namespace Oxygen.CommonTool.Logger
{
    /// <summary>
    /// 控制台日志
    /// </summary>
    public class OxygenConsoleLogger: IOxygenLogger
    {

        private readonly ILogger _logger;
        public OxygenConsoleLogger(ILogger<ConsoleLoggerProvider> logger
        )
        {
            _logger = logger;
        }
        /// <summary>
        /// 普通信息
        /// </summary>
        /// <param name="message"></param>
        public void LogInfo(string message)
        {
            _logger.LogInformation($"|{DateTime.Now}|OXYGEN_INFO|{message}");
        }
        /// <summary>
        /// 异常信息
        /// </summary>
        /// <param name="message"></param>
        public void LogError(string message)
        {
            _logger.LogError($"|{DateTime.Now}|OXYGEN_ERROR|{message}");
        }
    }
}
