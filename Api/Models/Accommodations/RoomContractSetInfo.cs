using System;
using System.Collections.Generic;
using HappyTravel.Edo.Common.Enums;
using HappyTravel.EdoContracts.Accommodations;
using HappyTravel.EdoContracts.Accommodations.Internals;
using HappyTravel.EdoContracts.General;

namespace HappyTravel.Edo.Api.Models.Accommodations
{
    public readonly struct RoomContractSetInfo
    {
        private RoomContractSetInfo(
            Guid id,
            in Price price,
            Deadline deadline,
            List<RoomContract> rooms,
            bool isAdvancedPurchaseRate,
            Suppliers? dataProvider)
        {
            Id = id;
            Price = price;
            Deadline = deadline;
            Rooms = rooms ?? new List<RoomContract>(0);
            IsAdvancedPurchaseRate = isAdvancedPurchaseRate;
            DataProvider = dataProvider;
        }
        
        
        public static RoomContractSetInfo FromRoomContractSet(in RoomContractSet roomContractSet, Suppliers? dataProvider)
        {
            return new RoomContractSetInfo(roomContractSet.Id,
                roomContractSet.Price,
                roomContractSet.Deadline,
                roomContractSet.RoomContracts,
                roomContractSet.IsAdvancedPurchaseRate,
                dataProvider);
        }
        
        
        public Guid Id { get; }
        public Price Price { get; }
        public Deadline Deadline { get; }
        public List<RoomContract> Rooms { get; }
        public bool IsAdvancedPurchaseRate { get; }
        public Suppliers? DataProvider { get; }
    }
}