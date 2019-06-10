using System.Collections.Generic;

namespace HappyTravel.Edo.Api.Models.Locations
{
    public readonly struct Country
    {
        public Country(string code, Dictionary<string, string> names, int regionId)
        {
            Code = code;
            Names = names;
            RegionId = regionId;
        }


        /// <summary>
        /// Country Alpha-2 code.
        /// </summary>
        public string Code { get; }

        /// <summary>
        /// The dictionary of country names on supported languages.
        /// </summary>
        public Dictionary<string, string> Names { get; }

        /// <summary>
        /// Country's region UN M.49 code.
        /// </summary>
        public int RegionId { get; }
    }
}
