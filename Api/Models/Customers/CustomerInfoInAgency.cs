using System.Collections.Generic;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Extensions;
using HappyTravel.Edo.Common.Enums;

namespace HappyTravel.Edo.Api.Models.Customers
{
    public readonly struct CustomerInfoInAgency
    {
        public CustomerInfoInAgency(int customerId, string firstName, string lastName, string email,
            string title, string position, int counterpartyId, string counterpartyName, int agencyId, string agencyName,
            bool isMaster, List<InCounterpartyPermissions> inCounterpartyPermissions)
        {
            CustomerId = customerId;
            FirstName = firstName;
            LastName = lastName;
            Email = email;
            Title = title;
            Position = position;
            CounterpartyId = counterpartyId;
            CounterpartyName = counterpartyName;
            AgencyId = agencyId;
            AgencyName = agencyName;
            IsMaster = isMaster;
            InCounterpartyPermissions = inCounterpartyPermissions;
        }


        /// <summary>
        ///     Customer ID.
        /// </summary>
        public int CustomerId { get; }

        /// <summary>
        ///     First name.
        /// </summary>
        public string FirstName { get; }

        /// <summary>
        ///     Last name.
        /// </summary>
        public string LastName { get; }

        /// <summary>
        ///     Customer e-mail.
        /// </summary>
        public string Email { get; }

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
        ///     Indicates whether the customer is master or regular customer.
        /// </summary>
        public bool IsMaster { get; }

        /// <summary>
        ///     Title (Mr., Mrs etc).
        /// </summary>
        public string Title { get; }

        /// <summary>
        ///     Customer position in counterparty.
        /// </summary>
        public string Position { get; }

        /// <summary>
        ///     Permissions of the customer.
        /// </summary>
        public List<InCounterpartyPermissions> InCounterpartyPermissions { get; }
    }
}