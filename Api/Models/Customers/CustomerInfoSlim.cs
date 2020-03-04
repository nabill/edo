using HappyTravel.Edo.Api.Models.Markups;
using Newtonsoft.Json;

namespace HappyTravel.Edo.Api.Models.Customers
{
    public readonly struct CustomerInfoSlim
    {
        [JsonConstructor]
        public CustomerInfoSlim(int customerId, string firstName, string lastName,
            int companyId, string companyName, int branchId, string branchTitle, MarkupPolicySettings? markupSettings)
        {
            CustomerId = customerId;
            FirstName = firstName;
            LastName = lastName;
            CompanyId = companyId;
            CompanyName = companyName;
            BranchId = branchId;
            BranchTitle = branchTitle;
            MarkupSettings = markupSettings;
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
        ///     Title of the customer's branch.
        /// </summary>
        public string BranchTitle { get; }

        /// <summary>
        ///     Markup settings of the customer.
        /// </summary>
        public MarkupPolicySettings? MarkupSettings { get; }
    }
}