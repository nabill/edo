using System.Collections.Generic;

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
        public List<string> EnabledSuppliers { get; init; }
        
        /// <summary>
        /// Is direct contracts only available for the client
        /// </summary>
        public bool HasDirectContractsFilter { get; init; }
    }
}