using System.Collections.Generic;
using HappyTravel.Edo.Common.Enums;

namespace HappyTravel.Edo.Api.Models.Customers
{
    public readonly struct CustomerCounterpartyInfo
    {
        public CustomerCounterpartyInfo(int id, string name, int branchId, string branchName, bool isMaster, List<InCounterpartyPermissions> inCounterpartyPermissions)
        {
            Id = id;
            Name = name;
            BranchId = branchId;
            BranchName = branchName;
            IsMaster = isMaster;
            InCounterpartyPermissions = inCounterpartyPermissions;
        }


        /// <summary>
        ///     Id of the counterparty.
        /// </summary>
        public int Id { get; }

        /// <summary>
        ///     Name of the counterparty.
        /// </summary>
        public string Name { get; }

        /// <summary>
        ///     Id of the branch of the counterparty, to which the customer belongs.
        /// </summary>
        public int BranchId { get; }

        /// <summary>
        ///     Name of the branch.
        /// </summary>
        public string BranchName { get; }

        /// <summary>
        ///     Flag indicating that customer is master in this counterparty.
        /// </summary>
        public bool IsMaster { get; }

        /// <summary>
        ///     List of permissions in current counterparty.
        /// </summary>
        public List<InCounterpartyPermissions> InCounterpartyPermissions { get; }
    }
}