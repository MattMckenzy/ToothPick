namespace ToothPick.Models
{
    public class Media
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
        public string Location { get; set; }

        [Required]
        public string Title { get; set; }

        [Required]
        public string Key { get; set; }

        public string Description { get; set; }

        public int? SeasonNumber { get; set; }

        public int? EpisodeNumber { get; set; }

        public float? Duration { get; set; }

        public string ThumbnailLocation { get; set; }
        
        public DateTime? DatePublished { get; set; }
    }
}
