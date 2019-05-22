using System.Threading;
using System.Threading.Tasks;

namespace Oxygen.CommonTool
{
    /// <summary>
    /// task类型扩展
    /// </summary>
    public static class TaskExtension
    {
        /// <summary>
        /// 超时等待回调
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="tcs"></param>
        /// <param name="ctok"></param>
        /// <returns></returns>
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
