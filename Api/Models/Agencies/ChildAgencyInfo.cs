using System;

namespace HappyTravel.Edo.Api.Models.Agencies
{
    public readonly struct ChildAgencyInfo
    {
        public ChildAgencyInfo(int id, string name, bool isActive, DateTime created)
        {
            Id = id;
            Name = name;
            IsActive = isActive;
            Created = created;
        }

        /// <summary>
        /// Agency id
        /// </summary>
        public int Id { get; }
        
        /// <summary>
        /// Agency name
        /// </summary>
        public string Name { get; }
        
        /// <summary>
        /// Activity state
        /// </summary>
        public bool IsActive { get; }
        
        /// <summary>
        /// Created date
        /// </summary>
        public DateTime Created { get; }
    }
}