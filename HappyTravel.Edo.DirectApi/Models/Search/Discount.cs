using System.Text.Json.Serialization;

namespace HappyTravel.Edo.DirectApi.Models.Search
{
    public readonly struct Discount
    {
        [JsonConstructor]
        public Discount(double percent, string? description = null)
        {
            Percent = percent;
            Description = description;
        }
        
        
        /// <summary>
        ///     Description of discount
        /// </summary>
        public string? Description { get; }

        /// <summary>
        ///     Percentage of the discount
        /// </summary>
        public double Percent { get; }
    }
}