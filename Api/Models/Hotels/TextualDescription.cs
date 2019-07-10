using System.Collections.Generic;
using Newtonsoft.Json;

namespace HappyTravel.Edo.Api.Models.Hotels
{
    public readonly struct TextualDescription
    {
        [JsonConstructor]
        public TextualDescription(TextualDescriptionTypes type, Dictionary<string, string> descriptions)
        {
            Type = type;
            Descriptions = descriptions;
        }


        public TextualDescriptionTypes Type { get; }
        public Dictionary<string, string> Descriptions { get; }
    }
}
