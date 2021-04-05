using System;

namespace HappyTravel.Edo.Api.Models.Agencies
{
    public struct SlimChildAgencyInfo
    {
        /// <summary>
        /// Agency id
        /// </summary>
        public int Id { get; init; }
        
        /// <summary>
        /// Agency name
        /// </summary>
        public string Name { get; init;}
        
        /// <summary>
        /// Activity state
        /// </summary>
        public bool IsActive { get; init;}
        
        /// <summary>
        /// Created date
        /// </summary>
        public DateTime Created { get; init;}
    }
}