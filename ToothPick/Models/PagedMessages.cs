namespace ToothPick.Models
{
    public class GotifyPagedMessages
    {
        public IEnumerable<GotifyMessage> Messages { get; set; }

        public GotifyPaging Paging { get; set; }
    }
}
