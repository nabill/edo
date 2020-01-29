using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Models.Bookings;

namespace HappyTravel.Edo.Api.Services.Accommodations
{
    public interface IBatchBookingProcessingService
    {
        Task<Result<List<int>>> GetBookingsForCancellation(DateTime deadlineDate);

        Task<Result<ProcessResult>> CancelBookings(List<int> bookingIds);
    }
}