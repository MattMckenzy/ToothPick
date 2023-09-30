namespace ToothPick.Models
{
    public class Library
    {
        [Required, Key]
        public string Name { get; set; }

        [JsonIgnore]
        public virtual List<Serie> Series { get; set; }
        [JsonIgnore]
        public virtual List<Location> Locations { get; set; }
        [JsonIgnore]
        public virtual List<Media> Medias { get; set; }
    }
}
