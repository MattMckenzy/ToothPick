namespace ToothPick.Models
{
    public class Location
    {
        [Required]
        public string LibraryName { get; set; }
        [JsonIgnore]
        public virtual Library Library { get; set; }

        [Required]
        public string SerieName { get; set; }
        [JsonIgnore]
        public virtual Serie Serie { get; set; }

        [Required]
        public string Url { get; set; }

        [Required]
        public int FetchCount { get; set; } = 10;

        public string MatchFilters { get; set; }

        public string DownloadFormat { get; set; }

        public string Cookies { get; set; }
    }
}
