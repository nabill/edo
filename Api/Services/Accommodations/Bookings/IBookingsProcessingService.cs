using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Models.Bookings;
using HappyTravel.Edo.Data.Management;

namespace HappyTravel.Edo.Api.Services.Accommodations.Bookings
{
    public interface IBookingsProcessingService
    {
        Task<List<int>> GetForCapture(DateTime date);

        Task<Result<ProcessResult>> Capture(List<int> bookingIds, ServiceAccount serviceAccount);
        
        Task<List<int>> GetForNotification(DateTime date);

        Task<Result<ProcessResult>> NotifyDeadlineApproaching(List<int> bookingIds, ServiceAccount serviceAccount);

        Task<List<int>> GetForCancellation();

        Task<Result<ProcessResult>> Cancel(List<int> bookingIds, ServiceAccount serviceAccount);
    }
}