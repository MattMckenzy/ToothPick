namespace ToothPick.Extensions
{
    public static partial class Helpers
    {
        [GeneratedRegex("(?:season |s)([0-9]+)|([0-9])(?:[0-9]{2})", RegexOptions.IgnoreCase, "en-CA")]
        public static partial Regex SeasonRegex();
        [GeneratedRegex("(?:[0-9]([0-9]{2}))|(?:episode |e|ep\\.|ep\\. )([0-9]+)", RegexOptions.IgnoreCase, "en-CA")]
        public static partial Regex EpisodeRegex();

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
