namespace HappyTravel.Edo.Api.Models.Locations
{
    public class CountrySlim
    {
        public CountrySlim(string code, string name)
        {
            Code = code;
            Name = name;
        }


        /// <summary>
        ///     Country Alpha-2 code.
        /// </summary>
        public string Code { get; }

        /// <summary>
        ///     The dictionary of country names on supported languages.
        /// </summary>
        public string Name { get; }
    }
}