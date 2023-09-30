namespace ToothPick.Extensions
{
    public static class CookieContainerExtensions
    {
        public static CookieContainer ParseFile(this CookieContainer cookieContainer, string path)
        {
            if (!File.Exists(path))
                return cookieContainer;

            IEnumerable<string> cookieFileLines = File.ReadLines(path);

            foreach (string cookieFileLine in cookieFileLines)
            {
                if (cookieFileLine.StartsWith("#"))
                    continue;

                IEnumerable<string> cookieItems = cookieFileLine.Split('\t', System.StringSplitOptions.RemoveEmptyEntries | System.StringSplitOptions.TrimEntries);

                if (cookieItems.Count() != 7)
                    continue;

                DateTime unixEpoch = new(1970, 1, 1);

                cookieContainer.Add(new Cookie
                {
                    Domain = cookieItems.ElementAt(0),
                    Path = cookieItems.ElementAt(2),
                    Secure = bool.TryParse(cookieItems.ElementAt(3), out bool secureResult) && secureResult,
                    Expires = int.TryParse(cookieItems.ElementAt(4), out int expiresResult) ? unixEpoch + TimeSpan.FromSeconds(expiresResult) : unixEpoch,
                    Name = cookieItems.ElementAt(5),
                    Value = cookieItems.ElementAt(6)
                });
            }

            return cookieContainer;
        }

    }
}
