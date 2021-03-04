using System;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Models.Users;
using HappyTravel.Edo.Data.Bookings;

namespace HappyTravel.Edo.Api.Services.Accommodations.Bookings
{
    public interface IBookingStatusChangesProcessor
    {
        Task ProcessConfirmation(Edo.Data.Bookings.Booking booking);

        Task<Result> ProcessCancellation(Booking booking, DateTime cancellationDate, UserInfo user);

        Task ProcessRejection(Edo.Data.Bookings.Booking booking, UserInfo user);

        Task<Result> ProcessDiscarding(Edo.Data.Bookings.Booking booking, UserInfo user);
    }
}