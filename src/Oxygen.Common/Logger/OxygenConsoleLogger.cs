using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using System;

namespace Oxygen.Common.Logger
{
    public class OxygenConsoleLogger: IOxygenLogger
    {

        private readonly ILogger _logger;
        public OxygenConsoleLogger(ILogger<ConsoleLoggerProvider> logger
        )
        {
            _logger = logger;
        }
        public void LogInfo(string message)
        {
            _logger.LogInformation($"|{DateTime.Now}|OXYGEN_INFO|{message}");
        }
        public void LogError(string message)
        {
            _logger.LogError($"|{DateTime.Now}|OXYGEN_ERROR|{message}");
        }
    }
}
