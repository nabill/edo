using System.Text.Json.Serialization;

namespace HappyTravel.Edo.DirectApi.Models.Static
{
    public readonly struct ImageInfo
    {
        /// <summary>
        ///     The picture of a service i.e. a room or an accommodation
        /// </summary>
        /// <param name="sourceUrl">The URL of an image.</param>
        /// <param name="caption">The caption for an image.</param>
        [JsonConstructor]
        public ImageInfo(string sourceUrl, string caption)
        {
            Caption = caption;
            SourceUrl = sourceUrl;
        }


        /// <summary>
        ///     The caption for an image.
        /// </summary>
        public string Caption { get; }

        /// <summary>
        ///     The URL of an image.
        /// </summary>
        public string SourceUrl { get; }


        public override bool Equals(object? obj) 
            => obj is ImageInfo other && Equals(other);

        public bool Equals(in ImageInfo other) 
            => (Caption, SourceUrl).Equals((other.Caption, other.SourceUrl));

        public override int GetHashCode() 
            => (Caption, SourceUrl).GetHashCode();
    }
}