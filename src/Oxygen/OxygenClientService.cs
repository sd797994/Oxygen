using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Oxygen.Common;
using Oxygen.IRpcProviderService;

namespace Oxygen
{
    public class OxygenClientService : IHostedService
    {
        private readonly IRpcServerProvider _rpcServerProvider;
        private static bool _stopFlag = false;

        public OxygenClientService(IRpcServerProvider rpcServerProvider)
        {
            _rpcServerProvider = rpcServerProvider;
            AppDomain.CurrentDomain.ProcessExit += ProcessExit;
        }

        private Task _executingTask;

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _executingTask = ExecuteAsync();
            if (_executingTask.IsCompleted)
            {
                await _executingTask;
            }
            await _rpcServerProvider.OpenServer();
            await Task.CompletedTask;
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            await _rpcServerProvider.CloseServer();
            await Task.WhenAny(_executingTask, Task.Delay(Timeout.Infinite, cancellationToken));
            _stopFlag = true;
        }

        public async void ProcessExit(object s, EventArgs e)
        {
            if (!_stopFlag)
            {
                await _rpcServerProvider.CloseServer();
                await Task.WhenAny(_executingTask, Task.Delay(Timeout.Infinite));
            }
        }

        public async Task ExecuteAsync()
        {
            await Task.Delay(5000);
        }
    }
}