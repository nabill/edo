using System.Text.Json.Serialization;

namespace HappyTravel.Edo.DirectApi.Models.Static
{
    public readonly struct ImageInfo
    {
        /// <summary>
        ///     Picture of a service (a room or accommodation)
        /// </summary>
        /// <param name="sourceUrl">URL of the image</param>
        /// <param name="caption">Caption of the image</param>
        [JsonConstructor]
        public ImageInfo(string sourceUrl, string caption)
        {
            Caption = caption;
            SourceUrl = sourceUrl;
        }


        /// <summary>
        ///     Caption of the image
        /// </summary>
        public string Caption { get; }

        /// <summary>
        ///     URL of the image
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