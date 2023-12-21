namespace ToothPick.Models
{
    public class Status
    {
        public required Series Series { get; set; }

        public required CancellationTokenSource SerieCancellationTokenSource { get; set; }
    }
}
