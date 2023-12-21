using System.ComponentModel;

namespace ToothPick.Models
{
    public enum LocationControlFields
    {        
        [Display(Name = "Library Name")]
        LibraryName,

        [Display(Name = "Series Name")]
        SeriesName,

        Name,

        Url,
        
        [Display(Name = "Fetch Count")]
        FetchCount,

        [Display(Name = "Match Filters")]
        MatchFilters,

        [Display(Name = "Download Format")]
        DownloadFormat,

        Cookies
    }
}
