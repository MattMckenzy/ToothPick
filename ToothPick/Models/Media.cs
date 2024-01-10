using System.ComponentModel.DataAnnotations.Schema;

namespace ToothPick.Models
{
    [PrimaryKey(nameof(LibraryName), nameof(SeriesName), nameof(Url))]

    public class Media
    {
        public string LibraryName { get; set; } = string.Empty;
        [JsonIgnore]
        public virtual Library? Library { get; set; }

        public string SeriesName { get; set; } = string.Empty;
        [JsonIgnore]
        public virtual Series? Series { get; set; }

        public string Url { get; set; } = string.Empty;

        public string Title { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public int? SeasonNumber { get; set; }

        public int? EpisodeNumber { get; set; }

        public float? Duration { get; set; }

        public string ThumbnailLocation { get; set; } = string.Empty;

        public DateTime? DatePublished { get; set; }
    }
}
