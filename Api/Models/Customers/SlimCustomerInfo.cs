using System;
using IdentityModel;
using Newtonsoft.Json;

namespace HappyTravel.Edo.Api.Models.Customers
{
    public readonly struct SlimCustomerInfo
    {
        [JsonConstructor]
        public SlimCustomerInfo(int customerId, string firstName, string lastName, DateTime created,
            int counterpartyId, string counterpartyName, int agencyId, string agencyName, string markupSettings)
        {
            CustomerId = customerId;
            Name = $"{firstName} {lastName}";
            Created = created.ToEpochTime();
            CounterpartyId = counterpartyId;
            CounterpartyName = counterpartyName;
            AgencyId = agencyId;
            AgencyName = agencyName;
            MarkupSettings = markupSettings;
        }

        /// <summary>
        ///     Customer ID.
        /// </summary>
        public int CustomerId { get; }

        /// <summary>
        ///     First and last name.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Created date timestamp.
        /// </summary>
        public long Created { get; }

        /// <summary>
        ///     ID of the customer's counterparty.
        /// </summary>
        public int CounterpartyId { get; }

        /// <summary>
        ///     Name of the customer's counterparty.
        /// </summary>
        public string CounterpartyName { get; }

        /// <summary>
        ///     ID of the customer's agency.
        /// </summary>
        public int AgencyId { get; }

        /// <summary>
        ///     Name of the customer's agency.
        /// </summary>
        public string AgencyName { get; }

        /// <summary>
        ///     Markup settings of the customer.
        /// </summary>
        public string MarkupSettings { get; }
    }
}