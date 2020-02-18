using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace HappyTravel.Edo.Api.Models.Customers
{
    public struct UpdateCompanyRequest
    {
        [JsonConstructor]
        public UpdateCompanyRequest(CompanyRegistrationInfo company)
        {
            Company = company;
        }

        /// <summary>
        ///     Company information.
        /// </summary>
        public CompanyRegistrationInfo Company { get; }
    }
}
