using System.ComponentModel.DataAnnotations.Schema;

namespace ToothPick.Models
{
    [PrimaryKey(nameof(LibraryName), nameof(Name))]
    public class Series
    {
        [JsonIgnore, NotMapped]
        public string DbKey { get { return $"{LibraryName}{Name}"; } }

        public string LibraryName { get; set; } = string.Empty;
        [JsonIgnore]
        public virtual Library? Library { get; set; }

        public string Name { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public string ThumbnailLocation { get; set; } = string.Empty;
        public string PosterLocation { get; set; } = string.Empty;
        public string BannerLocation { get; set; } = string.Empty;
        public string LogoLocation { get; set; } = string.Empty;

        [JsonIgnore]
        public virtual List<Location> Locations { get; set; } = [];

        [JsonIgnore]
        public virtual List<Media> Medias { get; set; } = [];
    }
}
