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
        ///     The discount description.
        /// </summary>
        public string? Description { get; }

        /// <summary>
        ///     Discount percentage.
        /// </summary>
        public double Percent { get; }
    }
}