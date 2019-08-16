using Newtonsoft.Json;

namespace HappyTravel.Edo.Api.Models.Accommodations
{
    public readonly struct Picture
    {
        [JsonConstructor]
        public Picture(string source, string caption)
        {
            Caption = caption;
            Source = source;
        }


        public string Source { get; }
        public string Caption { get; }
    }
}
