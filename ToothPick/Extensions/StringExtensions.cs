namespace ToothPick.Extensions
{
    public static class StringExtensions
    {
        public static string GetKey(this string inputString)
        {
            return Encoding.UTF8.GetBytes(inputString).ToBase62String();
        }
    }
}