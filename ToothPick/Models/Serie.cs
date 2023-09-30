namespace ToothPick.Models
{
    public class Serie
    {
        [Required]
        public string LibraryName { get; set; }
        [JsonIgnore]
        public virtual Library Library { get; set; }

        [Required]
        public string Name { get; set; }

        public string Description { get; set; }

        public int? SeasonCount { get; set; }
        public int? EpisodeCount { get; set; }

        public string ThumbnailLocation { get; set; }
        public string PosterLocation { get; set; }
        public string BannerLocation { get; set; }
        public string LogoLocation { get; set; }

        public DateTime? DatePublished { get; set; }

        [MinLength(1)]
        [JsonIgnore]
        public virtual List<Location> Locations { get; set; }

        [JsonIgnore]
        public virtual List<Media> Medias { get; set; }
    }
}
