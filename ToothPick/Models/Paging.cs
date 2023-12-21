namespace ToothPick.Models
{
    public class GotifyPaging
    {
        public int? Limit { get; set; }
        public string? Next { get; set; }
        public int? Since { get; set; }
        public int? Size { get; set; }
    }
}