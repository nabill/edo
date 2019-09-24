﻿using System;
using Newtonsoft.Json;

namespace HappyTravel.Edo.Api.Models.Accommodations
{
    public readonly struct RoomPrice
    {
        [JsonConstructor]
        public RoomPrice(DateTime fromDate, DateTime toDate, decimal gross, decimal nett, PriceTypes type)
        {
            FromDate = fromDate;
            ToDate = toDate;
            Gross = gross;
            Nett = nett;
            Type = type;
        }
        
        public RoomPrice(RoomPrice roomPrice, decimal gross, decimal nett)
        {
            FromDate = roomPrice.FromDate;
            ToDate = roomPrice.ToDate;
            Gross = gross;
            Nett = nett;
            Type = roomPrice.Type;
        }


        public DateTime FromDate { get; }
        public DateTime ToDate { get; }
        public decimal Gross { get; }
        public decimal Nett { get; }
        public PriceTypes Type { get; }
    }
}
