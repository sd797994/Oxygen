using Autofac;
using Microsoft.Extensions.Hosting;
using Oxygen.CommonTool;
using Oxygen.IRpcProviderService;
using Oxygen.ServerProxyFactory;
using System;
using System.Runtime.Loader;
using System.Threading;
using System.Threading.Tasks;

namespace Oxygen
{
    /// <summary>
    /// OxygenHost服务
    /// </summary>
    public class OxygenHostService : IHostedService
    {
        private readonly IRpcServerProvider _rpcServerProvider;
        private static bool _stopFlag = false;

        public OxygenHostService(IRpcServerProvider rpcServerProvider, ILifetimeScope container)
        {
            OxygenIocContainer.BuilderIocContainer(container);
            _rpcServerProvider = rpcServerProvider;
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
            LocalProxyGenerator.LoadMethodDelegate();
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            await CloseOxygenService();
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

        public async Task CloseOxygenService()
        {
            try
            {
                await _rpcServerProvider.CloseServer();
            }
            catch (Exception)
            {

            }
        }
    }
}