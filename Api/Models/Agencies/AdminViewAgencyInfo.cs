using System;
using Api.Models.Management.Administrators;
using HappyTravel.Edo.Common.Enums;

namespace HappyTravel.Edo.Api.Models.Agencies
{
    public readonly struct AdminViewAgencyInfo
    {
        /// <summary>
        ///     Id of the agency.
        /// </summary>
        public int Id { get; init; }

        /// <summary>
        ///     Name of the agency.
        /// </summary>
        public string Name { get; init; }

        /// <summary>
        /// Country name.
        /// </summary>
        public string CountryName { get; init; }

        /// <summary>
        ///     City name.
        /// </summary>
        public string City { get; init; }

        /// <summary>
        /// Verification state of the agency
        /// </summary>
        public AgencyVerificationStates VerificationState { get; init; }

        /// <summary>
        /// Activity status
        /// </summary>
        public bool IsActive { get; init; }

        /// <summary>
        /// Account manager id
        /// </summary>
        public int? AccountManagerId { get; init; }

        /// <summary>
        /// Account manager full name
        /// </summary>
        public string? AccountManagerName { get; init; }

        /// <summary>
        /// Agency creation date
        /// </summary>
        public DateTime Created { get; init; }
    }
}
