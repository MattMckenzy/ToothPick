namespace ToothPick.Models
{
    public class Setting
    {
        [Required]
        public string Name { get; set; }

        [Required]
        public string Value { get; set; }
    }
}
