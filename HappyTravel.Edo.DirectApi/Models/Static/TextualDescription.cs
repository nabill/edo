using System.Text.Json.Serialization;
using HappyTravel.MapperContracts.Public.Accommodations.Enums;

namespace HappyTravel.Edo.DirectApi.Models.Static
{
    public readonly struct TextualDescription
    {
        [JsonConstructor]
        public TextualDescription(TextualDescriptionTypes type, string description)
        {
            Type = type;
            Description = description;
        }


        /// <summary>
        ///     The description.
        /// </summary>
        public string Description { get; }

        /// <summary>
        ///     The description type.
        /// </summary>
        public TextualDescriptionTypes Type { get; }


        public override bool Equals(object? obj) => obj is TextualDescription other && Equals(other);


        public bool Equals(in TextualDescription other) => (Type, Description).Equals((other.Type, other.Description));


        public override int GetHashCode() => (Type, Description).GetHashCode();
    }
}