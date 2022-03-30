namespace HappyTravel.Edo.Api.Models.Locations
{
    public readonly struct Country
    {
        public Country(string code, string name, int marketId)
        {
            Code = code;
            Name = name;
            MarketId = marketId;
        }


        /// <summary>
        ///     Country Alpha-2 code.
        /// </summary>
        public string Code { get; }

        /// <summary>
        ///     The dictionary of country names on supported languages.
        /// </summary>
        public string Name { get; }

        /// <summary>
        ///     Country's market id.
        /// </summary>
        public int MarketId { get; }
    }
}