namespace ToothPick.Models
{
    public class Library
    {
        [Key]
        public string Name { get; set; } = string.Empty;

        [JsonIgnore]
        public virtual List<Series> Series { get; set; } = [];
        [JsonIgnore]
        public virtual List<Location> Locations { get; set; } = [];
        [JsonIgnore]
        public virtual List<Media> Medias { get; set; } = [];
    }
}
