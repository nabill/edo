using System.Collections.Generic;
using Newtonsoft.Json;

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


        public static string GetValueFromSerializedString(string source, string languageCode)
        {
            if (string.IsNullOrEmpty(source))
                return string.Empty;

            var json = JsonConvert.DeserializeObject<Dictionary<string, string>>(source);
            return GetValue(json, languageCode);
        }


        public static string GetDefaultValueFromSerializedString(string source) => GetValueFromSerializedString(source, DefaultLanguageCode);


        public static Dictionary<string, string> GetValues(string source) => JsonConvert.DeserializeObject<Dictionary<string, string>>(source);
        

        public const string DefaultLanguageCode = "en";
    }
}