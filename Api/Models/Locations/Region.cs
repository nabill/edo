namespace HappyTravel.Edo.Api.Models.Locations
{
    public class Region
    {
        public Region(int id, string name)
        {
            Id = id;
            Name = name;
        }


        /// <summary>
        ///     Region id.
        /// </summary>
        public int Id { get; }

        /// <summary>
        ///     The dictionary of Region names on supported languages.
        /// </summary>
        public string Name { get; }
    }
}