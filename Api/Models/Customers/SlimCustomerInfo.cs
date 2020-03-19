using System;
using IdentityModel;
using Newtonsoft.Json;

namespace HappyTravel.Edo.Api.Models.Customers
{
    public readonly struct SlimCustomerInfo
    {
        [JsonConstructor]
        public SlimCustomerInfo(int customerId, string firstName, string lastName, DateTime created,
            int companyId, string companyName, int branchId, string branchName, string markupSettings)
        {
            CustomerId = customerId;
            Name = $"{firstName} {lastName}";
            Created = created.ToEpochTime();
            CompanyId = companyId;
            CompanyName = companyName;
            BranchId = branchId;
            BranchName = branchName;
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
        ///     ID of the customer's company.
        /// </summary>
        public int CompanyId { get; }

        /// <summary>
        ///     Name of the customer's company.
        /// </summary>
        public string CompanyName { get; }

        /// <summary>
        ///     ID of the customer's branch.
        /// </summary>
        public int BranchId { get; }

        /// <summary>
        ///     Name of the customer's branch.
        /// </summary>
        public string BranchName { get; }

        /// <summary>
        ///     Markup settings of the customer.
        /// </summary>
        public string MarkupSettings { get; }
    }
}