using System;
using System.Collections.Generic;
using HappyTravel.Edo.Data.AccommodationMappings;
using Newtonsoft.Json;

namespace HappyTravel.Edo.Api.Models.Accommodations
{
    public readonly struct SlimAccommodationDuplicateReportInfo
    {
        [JsonConstructor]
        public SlimAccommodationDuplicateReportInfo(int id, DateTime created, AccommodationDuplicateReportState state, string agentName, List<ProviderAccommodationId> accommodations)
        {
            Id = id;
            Created = created;
            State = state;
            AgentName = agentName;
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
        /// Reporter agent name
        /// </summary>
        public string AgentName { get; }
        
        /// <summary>
        /// Reported accommodation ids
        /// </summary>
        public List<ProviderAccommodationId> Accommodations { get; }
    }
}