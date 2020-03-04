using CSharpFunctionalExtensions;
using HappyTravel.Edo.Common.Enums;

namespace HappyTravel.Edo.Api.Models.Customers
{
    public readonly struct CustomerInfo
    {
        public CustomerInfo(int customerId, string firstName, string lastName, string email,
            string title, string position, int companyId, string companyName, int branchId, bool isMaster,
            InCompanyPermissions inCompanyPermissions)
        {
            CustomerId = customerId;
            FirstName = firstName;
            LastName = lastName;
            Email = email;
            Title = title;
            Position = position;
            CompanyId = companyId;
            CompanyName = companyName;
            BranchId = branchId;
            IsMaster = isMaster;
            InCompanyPermissions = inCompanyPermissions;
        }


        public void Deconstruct(out int customerId, out int companyId, out int branchId, out bool isMaster)
        {
            customerId = CustomerId;
            companyId = CompanyId;
            branchId = BranchId;
            isMaster = IsMaster;
        }


        public bool Equals(CustomerInfo other)
            => (CustomerId, CompanyId, BranchId, IsMaster)
                == (other.CustomerId, other.CompanyId, other.BranchId, other.IsMaster);


        public override bool Equals(object obj) => obj is CustomerInfo other && Equals(other);


        public override int GetHashCode() => (CustomerId, CompanyId, BranchId, IsMaster).GetHashCode();

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
        ///     Indicates whether the customer is master or regular customer.
        /// </summary>
        public bool IsMaster { get; }

        /// <summary>
        ///     Permissions of the customer.
        /// </summary>
        public InCompanyPermissions InCompanyPermissions { get; }

        /// <summary>
        ///     Title (Mr., Mrs etc).
        /// </summary>
        public string Title { get; }

        /// <summary>
        ///     Customer position in company.
        /// </summary>
        public string Position { get; }
    }
}