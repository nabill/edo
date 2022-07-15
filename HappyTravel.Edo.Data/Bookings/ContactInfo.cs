using System.Collections.Generic;
using Newtonsoft.Json;

namespace HappyTravel.Edo.Data.Bookings
{
    public class ContactInfo
    {
        // EF constructor
        private ContactInfo() { }
        
        [JsonConstructor]
        public ContactInfo(List<string> emails, List<string> phones, List<string> webSites, List<string> faxes)
        {
            Emails = emails ?? new List<string>(0);
            Faxes = faxes ?? new List<string>(0);
            Phones = phones ?? new List<string>(0);
            WebSites = webSites ?? new List<string>(0);
        }


        /// <summary>
        ///     The accommodation email.
        /// </summary>
        public List<string> Emails { get; }

        /// <summary>
        ///     The accommodation fax number.
        /// </summary>
        public List<string> Faxes { get; }

        /// <summary>
        ///     The accommodation phone.
        /// </summary>
        public List<string> Phones { get; }

        /// <summary>
        ///     The accommodation web site.
        /// </summary>
        public List<string> WebSites { get; }
    }
}