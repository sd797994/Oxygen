using System.Threading;
using System.Threading.Tasks;

namespace Oxygen.CommonTool
{
    public static class TaskExtension
    {
        public static async Task WaitAsync<T>(this TaskCompletionSource<T> tcs, CancellationToken ctok)
        {
            CancellationTokenSource linkedCts = null;
            try
            {
                var cts = new CancellationTokenSource();
                linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cts.Token, ctok);
                var exitTok = linkedCts.Token;
                async Task ListenForCancelTaskFnc()
                {
                    await Task.Delay(-1, exitTok).ConfigureAwait(false);
                }
                var cancelTask = ListenForCancelTaskFnc();
                await Task.WhenAny(tcs.Task, cancelTask).ConfigureAwait(false);
                cts.Cancel();
            }
            finally
            {
                linkedCts?.Dispose();
            }
        }
    }
}
