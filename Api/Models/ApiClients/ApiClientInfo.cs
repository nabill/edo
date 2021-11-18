using System.Collections.Generic;
using HappyTravel.SuppliersCatalog;

namespace HappyTravel.Edo.Api.Models.ApiClients
{
    public readonly struct ApiClientInfo
    {
        /// <summary>
        /// Agency name
        /// </summary>
        public string AgencyName { get; init; }
        
        /// <summary>
        /// Suppliers enabled for the client
        /// </summary>
        public List<Suppliers> EnabledSuppliers { get; init; }
        
        /// <summary>
        /// Is direct contracts only available for the client
        /// </summary>
        public bool HasDirectContractsFilter { get; init; }
    }
}