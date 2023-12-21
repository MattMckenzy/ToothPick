namespace ToothPick.Services
{
    public class StatusService
    {
        private int _processingPercent = -1;
        public int ProcessingPercent { get { return _processingPercent; } set { _processingPercent = value; } }
        public int IncrementProcessingPercent() { return Interlocked.Increment(ref _processingPercent); }

        public int TotalProcessingSeries { get; set; } = 0;

        public DateTime? NextProcessingTime { get; set; } = null;

        public CancellationTokenSource? ProcessingCancellationTokenSource { get; set; } = null;

        public ConcurrentDictionary<ComponentBase, Func<Task>> UpdateProcessingDelegates { get; set; }


        public ConcurrentDictionary<Series, Status> Statuses { get; set; }

        public ConcurrentDictionary<ComponentBase, Func<Task>> UpdateStatusesDelegates { get; set; }


        public StatusService()
        {
            UpdateProcessingDelegates = new ConcurrentDictionary<ComponentBase, Func<Task>>();
            Statuses = new ConcurrentDictionary<Series, Status>();
            UpdateStatusesDelegates = new ConcurrentDictionary<ComponentBase, Func<Task>>();
        }
    }
}
