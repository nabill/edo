using HappyTravel.Edo.Api.Models.Markups;
using Newtonsoft.Json;

namespace HappyTravel.Edo.Api.Models.Customers
{
    public readonly struct CustomerInfoInSearch
    {
        [JsonConstructor]
        public CustomerInfoInSearch(int customerId, string firstName, string lastName,
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

        public int CustomerId { get; }
        public string FirstName { get; }
        public string LastName { get; }
        public int CompanyId { get; }
        public string CompanyName { get; }
        public int BranchId { get; }
        public string BranchTitle { get; }
        public MarkupPolicySettings? MarkupSettings { get; }
    }
}