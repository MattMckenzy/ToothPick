using System.ComponentModel.DataAnnotations.Schema;

namespace ToothPick.Models
{
    [PrimaryKey(nameof(LibraryName), nameof(SeriesName), nameof(Name))]

    public class Location
    {
        public string LibraryName { get; set; } = string.Empty;
        [JsonIgnore]
        public virtual Library? Library { get; set; }

        public string SeriesName { get; set; } = string.Empty;
        [JsonIgnore]
        public virtual Series? Series { get; set; }

        public string Name { get; set; } = string.Empty;

        public string Url { get; set; } = string.Empty;

        public int? FetchCount { get; set; }

        public string MatchFilters { get; set; } = string.Empty;

        public string DownloadFormat { get; set; } = string.Empty;

        public string Cookies { get; set; } = string.Empty;
    }
}
