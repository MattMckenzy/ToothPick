namespace ToothPick.Extensions
{
    public static partial class StringExtensions
    {
        public static string GetKey(this string inputString)
        {
            return Encoding.UTF8.GetBytes(inputString).ToBase62String();
        }
        
        public static string Cut(this string inputString, int startIndex, int count)
        {
            string returnString =  inputString[..startIndex] + inputString[(startIndex + count)..];
            return returnString;
        }

        public static string RemoveExtraSpaces(this string value)  
        {
            Regex regex = MultipleSpaceRegex();
            value = regex.Replace(value, " ");

            value = value.Trim(' ');

            return value;
        }

        [GeneratedRegex("[ ]{2,}", RegexOptions.None)]
        private static partial Regex MultipleSpaceRegex();
    }
}