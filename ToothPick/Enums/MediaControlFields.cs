namespace ToothPick.Enums
{
    public enum MediaControlFields
    {        
        [Display(Name = "Library Name")]
        LibraryName,

        [Display(Name = "Series Name")]
        SeriesName,

        Url,

        Title,

        Description,

        [Display(Name = "Season Number")]
        SeasonNumber,

        [Display(Name = "Episode Number")]
        EpisodeNumber,

        Duration,

        [Display(Name = "Thumbnail Location")]
        ThumbnailLocation,
        
        [Display(Name = "Date Published")]
        DatePublished
    }
}
