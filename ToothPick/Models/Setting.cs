namespace ToothPick.Models
{
    public class Setting
    {
        [Key]
        public required string Name { get; set; }

        public string Value { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;
    }
}

