using System;
using System.Linq;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Infrastructure;
using HappyTravel.Edo.Api.Services.Management;
using HappyTravel.Edo.Api.Services.Payments;
using HappyTravel.Edo.Data;
using HappyTravel.Edo.Data.Booking;
using HappyTravel.EdoContracts.General.Enums;
using Microsoft.AspNetCore.Mvc;
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

        public async Task<Result<Booking, ProblemDetails>> Confirm(int bookingId)
        {
            return await GetBooking()
                .Bind(SendReceipt)
                .Bind(SaveConfirmation);


            async Task<Result<Booking, ProblemDetails>> GetBooking()
            {
                var booking = await _edoContext.Bookings
                    .Include(b => b.CreditCardPaymentConfirmation)
                    .SingleOrDefaultAsync(b =>
                        b.Id == bookingId &&
                        b.PaymentMethod == PaymentMethods.CreditCard &&
                        b.CreditCardPaymentConfirmation != null);

                return booking ?? ProblemDetailsBuilder.Fail<Booking>($"Booking with Id {bookingId} not found");
            }

            async Task<Result<Booking, ProblemDetails>> SendReceipt(Booking booking)
            {
                var (_, isReceiptFailure, receiptInfo, receiptError) = await _documentsService.GenerateReceipt(booking.Id, booking.AgentId);
                if (isReceiptFailure)
                    return ProblemDetailsBuilder.Fail<Booking>(receiptError);

                var email = await _edoContext.Agents
                    .Where(a => a.Id == booking.AgentId)
                    .Select(a => a.Email)
                    .SingleOrDefaultAsync();

                await _notificationService.SendReceiptToCustomer(receiptInfo, email);
                return booking;
            }

            async Task<Result<Booking, ProblemDetails>> SaveConfirmation(Booking booking)
            {
                var (_, isFailure, administrator, error) = await _administratorContext.GetCurrent();

                if (isFailure)
                {
                    return ProblemDetailsBuilder.Fail<Booking>(error);
                }

                booking.CreditCardPaymentConfirmation = new CreditCardPaymentConfirmation
                {
                    BookingId = bookingId,
                    AdministratorId = administrator.Id,
                    ConfirmedAt = DateTime.UtcNow
                };

                await _edoContext.SaveChangesAsync();
                return booking;
            }
        }


        private readonly IAdministratorContext _administratorContext;
        private readonly EdoContext _edoContext;
        private readonly IBookingDocumentsService _documentsService;
        private readonly IPaymentNotificationService _notificationService;
    }
}