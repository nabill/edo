using System;
using Newtonsoft.Json;

namespace HappyTravel.Edo.Api.Models.Agencies
{
    public readonly struct SlimUploadedImage
    {
        [JsonConstructor]
        public SlimUploadedImage(string fileName, string link, DateTime created, DateTime updated)
        {
            FileName = fileName;
            Link = link;
            Created = created;
            Updated = updated;
        }

        /// <summary>
        /// File name of the image
        /// </summary>
        public string FileName { get; }

        /// <summary>
        /// Download link of the image
        /// </summary>
        public string Link { get; }

        /// <summary>
        /// DateTime when the image with this name was uploaded first time
        /// </summary>
        public DateTime Created { get; }

        /// <summary>
        /// DateTime when the image with this name was uploaded last time
        /// </summary>
        public DateTime Updated { get; }
    }
}
