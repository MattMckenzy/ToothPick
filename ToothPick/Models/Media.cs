using System.ComponentModel.DataAnnotations.Schema;

namespace ToothPick.Models
{
    [PrimaryKey(nameof(LibraryName), nameof(SeriesName), nameof(Location))]
    
    public class Media
    {
        [JsonIgnore, NotMapped]
        public string DbKey { get { return $"{LibraryName}{SeriesName}{Location}"; } }

        public string LibraryName { get; set; } = string.Empty;
        [JsonIgnore]
        public virtual Library? Library { get; set; }

        public string SeriesName { get; set; } = string.Empty;
        [JsonIgnore]
        public virtual Series? Series { get; set; }

        public string Location { get; set; } = string.Empty;

        public string Title { get; set; } = string.Empty;

        public string Key { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public int? SeasonNumber { get; set; }

        public int? EpisodeNumber { get; set; }

        public float? Duration { get; set; }

        public string ThumbnailLocation { get; set; } = string.Empty;
        
        public DateTime? DatePublished { get; set; }
    }
}
