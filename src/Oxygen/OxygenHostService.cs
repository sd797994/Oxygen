using Microsoft.Extensions.Hosting;
using Oxygen.CommonTool;
using Oxygen.IRpcProviderService;
using Oxygen.IServerFlowControl;
using Oxygen.IServerRegisterManage;
using System;
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
        private readonly IRegisterCenter _registerCenter;
        private readonly IRemoteFlowContolConfiugreManage _configureManage;

        public OxygenHostService(IRpcServerProvider rpcServerProvider, IRegisterCenter registerCenter, IRemoteFlowContolConfiugreManage configureManage)
        {
            _rpcServerProvider = rpcServerProvider;
            _registerCenter = registerCenter;
            _configureManage = configureManage;
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
            var rpcEndPoint = await _rpcServerProvider.OpenServer();
            if (await _registerCenter.RegisterService(OxygenSetting.ServerName, rpcEndPoint))
            {
                await Task.CompletedTask;
            }
            _configureManage.SetCacheFromServices();
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            await _rpcServerProvider.CloseServer();
            await _registerCenter.UnRegisterService();
            await Task.WhenAny(_executingTask, Task.Delay(Timeout.Infinite, cancellationToken));
            _stopFlag = true;
        }

        public async void ProcessExit(object s, EventArgs e)
        {
            if (!_stopFlag)
            {
                await _rpcServerProvider.CloseServer();
                await _registerCenter.UnRegisterService();
                await Task.WhenAny(_executingTask, Task.Delay(Timeout.Infinite));
            }
        }

        public async Task ExecuteAsync()
        {
            await Task.Delay(5000);
        }
    }
}