namespace ToothPick.Extensions
{
    public static class Helpers
    {
        private static CancellationTokenSource DelayCancelCancellationTokenSource = new();
        public static async Task DelayCancel(this Func<Task> action, int millisecondsDelay = 500)
        {
            DelayCancelCancellationTokenSource.Cancel();
            DelayCancelCancellationTokenSource = new();
            await Task.Delay(millisecondsDelay, DelayCancelCancellationTokenSource.Token).ContinueWith(async (state) =>
            {
                if (!state?.IsCanceled ?? false)
                {
                    await action();
                }
            });
        }

        public static async Task DelayCancel<T>(this Func<T, Task> function, T parameter, int millisecondsDelay = 500)
        {
            DelayCancelCancellationTokenSource.Cancel();
            DelayCancelCancellationTokenSource = new();
            await Task.Delay(millisecondsDelay, DelayCancelCancellationTokenSource.Token).ContinueWith(async (state) => 
            {
                if (!state?.IsCanceled ?? false)
                {
                    await function(parameter);
                }
            });
        }

        public static async Task DelayCancel<T1, T2>(this Func<T1, T2, Task> function, T1 parameter1, T2 parameter2, int millisecondsDelay = 500)
        {
            DelayCancelCancellationTokenSource.Cancel();
            DelayCancelCancellationTokenSource = new();
            await Task.Delay(millisecondsDelay, DelayCancelCancellationTokenSource.Token).ContinueWith(async (state) =>
            {
                if (!state?.IsCanceled ?? false)
                {
                    await function(parameter1, parameter2);
                }
            });
        }
    }
}
