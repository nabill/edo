using System.Collections.Generic;

namespace HappyTravel.Edo.Api.Models.Locations
{
    public readonly struct Region
    {
        public Region(int id, Dictionary<string, string> names)
        {
            Id = id;
            Names = names;
        }


        /// <summary>
        /// Region UN M.49 code.
        /// </summary>
        public int Id { get; }

        /// <summary>
        /// The dictionary of region names on supported languages.
        /// </summary>
        public Dictionary<string, string> Names { get; }
    }
}
