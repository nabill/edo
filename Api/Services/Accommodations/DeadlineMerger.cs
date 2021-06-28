using System;
using System.Collections.Generic;
using System.Linq;
using HappyTravel.EdoContracts.Accommodations;
using HappyTravel.EdoContracts.Accommodations.Internals;

namespace HappyTravel.Edo.Api.Services.Accommodations
{
    public static class DeadlineMerger
    {
        public static Deadline CalculateMergedDeadline(List<RoomContract> roomContracts)
        {
            var contractsWithDeadline = roomContracts
                .Where(contract => contract.Deadline.Date.HasValue)
                .ToList();
            
            if (!contractsWithDeadline.Any())
                return default;
            
            var totalAmount = Convert.ToDouble(roomContracts.Sum(r => r.Rate.FinalPrice.Amount));
            var deadlineDate = contractsWithDeadline
                .Select(contract => contract.Deadline.Date.Value)
                .OrderByDescending(d => d)
                .FirstOrDefault();

            var policies = contractsWithDeadline
                .SelectMany(c => c.Deadline.Policies.Select(p => p.FromDate.Date))
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

                    return new CancellationPolicy(date, CalculatePercent(amount));
                })
                .ToList();

            var isFinal = contractsWithDeadline.All(p => p.Deadline.IsFinal);
            
            return new Deadline(deadlineDate, policies, new List<string>(), isFinal);
            
            
            double CalculatePercent(double amount) 
                => amount / totalAmount;
        }
    }
}