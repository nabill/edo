using System;
using System.Linq;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Services.Management;
using HappyTravel.Edo.Api.Services.Payments;
using HappyTravel.Edo.Data;
using HappyTravel.Edo.Data.Booking;
using HappyTravel.EdoContracts.General.Enums;
using Microsoft.EntityFrameworkCore;

namespace HappyTravel.Edo.Api.Services.Accommodations.Bookings
{
    public class CreditCardPaymentConfirmationService : ICreditCardPaymentConfirmationService
    {
        public CreditCardPaymentConfirmationService(IAdministratorContext administratorContext, EdoContext edoContext, IBookingDocumentsService documentsService,
            IPaymentNotificationService notificationService)
        {
            _administratorContext = administratorContext;
            _edoContext = edoContext;
            _documentsService = documentsService;
            _notificationService = notificationService;
        }

        public async Task<Result> Confirm(int bookingId)
        {
            return await GetBooking()
                .Bind(SendReceipt)
                .Bind(SaveConfirmation);


            async Task<Result<Booking>> GetBooking()
            {
                var query = from booking in _edoContext.Bookings
                    join confirmation in _edoContext.CreditCardPaymentConfirmations on booking.Id equals confirmation.BookingId
                    where booking.Id == bookingId && booking.PaymentMethod == PaymentMethods.CreditCard
                    select booking;

                var data = await query.SingleOrDefaultAsync();

                return data ?? Result.Failure<Booking>($"Booking with Id {bookingId} not found");
            }


            async Task<Result> SendReceipt(Booking booking)
            {
                var (_, isReceiptFailure, receiptInfo, receiptError) = await _documentsService.GenerateReceipt(booking.Id, booking.AgentId);
                if (isReceiptFailure)
                    return Result.Failure<Booking>(receiptError);

                var email = await _edoContext.Agents
                    .Where(a => a.Id == booking.AgentId)
                    .Select(a => a.Email)
                    .SingleOrDefaultAsync();

                await _notificationService.SendReceiptToCustomer(receiptInfo, email);
                return Result.Success();
            }


            async Task<Result> SaveConfirmation()
            {
                var (_, isFailure, administrator, error) = await _administratorContext.GetCurrent();

                if (isFailure)
                    return Result.Failure(error);

                await _edoContext.CreditCardPaymentConfirmations.AddAsync(new CreditCardPaymentConfirmation
                {
                    BookingId = bookingId,
                    AdministratorId = administrator.Id,
                    ConfirmedAt = DateTime.UtcNow
                });
                await _edoContext.SaveChangesAsync();
                return Result.Success();
            }
        }


        private readonly IAdministratorContext _administratorContext;
        private readonly EdoContext _edoContext;
        private readonly IBookingDocumentsService _documentsService;
        private readonly IPaymentNotificationService _notificationService;
    }
}