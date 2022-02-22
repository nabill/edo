using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace HappyTravel.Edo.DirectApi.Models.Static
{
    public readonly struct ContactInfo
    {
        [JsonConstructor]
        public ContactInfo(List<string>? emails, List<string>? phones, List<string>? webSites, List<string>? faxes)
        {
            Emails = emails ?? new List<string>(0);
            Faxes = faxes ?? new List<string>(0);
            Phones = phones ?? new List<string>(0);
            WebSites = webSites ?? new List<string>(0);
        }


        /// <summary>
        ///     Email address of the accommodation
        /// </summary>
        public List<string> Emails { get; }

        /// <summary>
        ///     Fax number of the accommodation
        /// </summary>
        public List<string> Faxes { get; }

        /// <summary>
        ///     Phone number of the accommodation
        /// </summary>
        public List<string> Phones { get; }

        /// <summary>
        ///     Web site of the accommodation
        /// </summary>
        public List<string> WebSites { get; }
    }
}