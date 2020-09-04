using System;
using System.Collections.Generic;
using HappyTravel.Edo.Data.AccommodationMappings;
using HappyTravel.EdoContracts.Accommodations;
using Newtonsoft.Json;

namespace HappyTravel.Edo.Api.Models.Accommodations
{
    public readonly struct AccommodationDuplicateReportInfo
    {
        [JsonConstructor]
        public AccommodationDuplicateReportInfo(int id, DateTime created, AccommodationDuplicateReportState state,
            List<ProviderData<Accommodation>> accommodations)
        {
            Id = id;
            Created = created;
            State = state;
            Accommodations = accommodations;
        }
        
        /// <summary>
        /// Report id
        /// </summary>
        public int Id { get; }
        
        /// <summary>
        /// Created date
        /// </summary>
        public DateTime Created { get; }
        
        /// <summary>
        /// Report approval state
        /// </summary>
        public AccommodationDuplicateReportState State { get; }
        
        /// <summary>
        /// Reported accommodations
        /// </summary>
        public List<ProviderData<Accommodation>> Accommodations { get; }
    }
}