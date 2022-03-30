namespace HappyTravel.Edo.Api.Models.Locations
{
    public readonly struct Market
    {
        public Market(int id, string name)
        {
            Id = id;
            Name = name;
        }


        /// <summary>
        ///     Market id.
        /// </summary>
        public int Id { get; }

        /// <summary>
        ///     The dictionary of market names on supported languages.
        /// </summary>
        public string Name { get; }
    }
}