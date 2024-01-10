namespace ToothPick.Models
{
    public class GotifyMessage
    {
        [JsonIgnore]
        public int InternalId { get; set; }

        public int? AppId { get; set; }

        public int? Id { get; set; }

        public DateTime? Date { get; set; }

        public int? Priority { get; set; }

        public string? Title { get; set; }

        public string? Message { get; set; }

    }
}
