﻿using Autofac;
using Microsoft.Extensions.Hosting;
using Oxygen.CommonTool;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Oxygen
{
    /// <summary>
    /// OxygenClient服务
    /// </summary>
    public class OxygenClientService : IHostedService
    {
        private static bool _stopFlag = false;
        public OxygenClientService(ILifetimeScope container)
        {
            OxygenIocContainer.BuilderIocContainer(container);
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
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            OxygenIocContainer.DisposeIocContainer();
            await Task.WhenAny(_executingTask, Task.Delay(Timeout.Infinite, cancellationToken));
            _stopFlag = true;
        }

        public async void ProcessExit(object s, EventArgs e)
        {
            if (!_stopFlag)
            {
                await Task.WhenAny(_executingTask, Task.Delay(Timeout.Infinite));
            }
        }

        public async Task ExecuteAsync()
        {
            await Task.Delay(5000);
        }
    }
}
