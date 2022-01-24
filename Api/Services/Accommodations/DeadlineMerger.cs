using System;
using System.Collections.Generic;
using System.Linq;
using HappyTravel.Edo.Api.Models.Accommodations;
using HappyTravel.Edo.Data.Bookings;

namespace HappyTravel.Edo.Api.Services.Accommodations
{
    public static class DeadlineMerger
    {
        public static Deadline CalculateMergedDeadline(List<EdoContracts.Accommodations.Internals.RoomContract> roomContracts)
        {
            var rooms = roomContracts.Select(r => r.ToRoomContract())
                .ToList();

            return CalculateMergedDeadline(rooms);
        }
        
        public static Deadline CalculateMergedDeadline(List<RoomContract> roomContracts)
        {
            var isFinal = roomContracts.All(p => p.Deadline.IsFinal);
            
            var contractsWithDeadline = roomContracts
                .Where(contract => contract.Deadline.Date.HasValue)
                .ToList();

            if (!contractsWithDeadline.Any())
                return new Deadline(null, new List<CancellationPolicy>(), new List<string>(), isFinal);
            
            var totalAmount = Convert.ToDouble(roomContracts.Sum(r => r.Rate.FinalPrice.Amount));
            var deadlineDate = contractsWithDeadline
                .Select(contract => contract.Deadline.Date.Value)
                .OrderByDescending(d => d)
                .FirstOrDefault();

            var policies = contractsWithDeadline
                .SelectMany(c => c.Deadline.Policies.Select(p => p.FromDate))
                .Distinct()
                .OrderBy(d => d)
                .Select(date =>
                {
                    var amount = contractsWithDeadline.Sum(contract 
                        => contract.Deadline.Policies
                            .Where(p => p.FromDate <= date)
                            .OrderByDescending(p => p.FromDate)
                            .Select(p => p.Percentage * Convert.ToDouble(contract.Rate.FinalPrice.Amount))
                            .FirstOrDefault()
                    );

                    return new CancellationPolicy(date, CalculatePercent(amount), null);    // TODO: Need clarify
                })
                .ToList();

            
            
            return new Deadline(deadlineDate, policies, new List<string>(), isFinal);
            
            
            double CalculatePercent(double amount) 
                => Math.Round(amount / totalAmount, 2, MidpointRounding.AwayFromZero);
        }
    }
}