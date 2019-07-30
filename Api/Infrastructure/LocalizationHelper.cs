using System.Collections.Generic;

namespace HappyTravel.Edo.Api.Infrastructure
{
    public static class LocalizationHelper
    {
        public static string GetValue(Dictionary<string, string> source, string languageCode)
        {
            if (source.TryGetValue(languageCode, out var value))
            {
                if (!string.IsNullOrWhiteSpace(value))
                    return value;
            }

            if (source.TryGetValue(DefaultLanguageCode, out value))
            {
                if (!string.IsNullOrWhiteSpace(value))
                    return value;
            }

            foreach (var (_, val) in source)
            {
                if (!string.IsNullOrWhiteSpace(val))
                    return val;
            }

            return string.Empty;
        }


        public const string DefaultLanguageCode = "en";
    }

}
