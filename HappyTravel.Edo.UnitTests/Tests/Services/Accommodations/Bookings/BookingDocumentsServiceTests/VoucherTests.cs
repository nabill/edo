using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.AdministratorServices;
using HappyTravel.Edo.Api.Infrastructure.Options;
using HappyTravel.Edo.Api.Services.Accommodations;
using HappyTravel.Edo.Api.Services.Accommodations.Bookings.Documents;
using HappyTravel.Edo.Api.Services.Agents;
using HappyTravel.Edo.Api.Services.Documents;
using HappyTravel.Edo.Api.Services.Files;
using HappyTravel.Edo.Common.Enums;
using HappyTravel.Edo.Data.Agents;
using HappyTravel.Edo.Data.Bookings;
using HappyTravel.Edo.UnitTests.Utility;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace HappyTravel.Edo.UnitTests.Tests.Services.Accommodations.Bookings.BookingDocumentsServiceTests
{
    public class VoucherTests
    {
        [Fact]
        public async Task When_booking_has_not_confirmed_status_generation_voucher_should_fail()
        {
            var booking = new Booking
            {
                Id = 1,
                AgentId = 1,
                Status = BookingStatuses.Cancelled,
                PaymentStatus = It.IsAny<BookingPaymentStatuses>()
            };
            
            var bookingDocumentsService = CreateBookingDocumentsService();

            var (isSuccess, _) = await bookingDocumentsService.GenerateVoucher(booking, default);

            Assert.False(isSuccess);
        }

        [Fact]
        public async Task When_booking_has_confirmed_status_and_not_payed_generation_voucher_should_fail()
        {
            var booking = new Booking
            {
                Id = 1,
                AgentId = 1,
                Status = BookingStatuses.Confirmed,
                PaymentStatus = BookingPaymentStatuses.NotPaid
            };
            
            var bookingDocumentsService = CreateBookingDocumentsService();

            var (isSuccess, _) = await bookingDocumentsService.GenerateVoucher(booking, default);

            Assert.False(isSuccess);
        }


        [Fact]
        public async Task When_booking_has_confirmed_status_and_payed_generation_voucher_should_succeed()
        {
            var booking = new Booking
            {
                Id = 1,
                AgentId = 1,
                Status = BookingStatuses.Confirmed,
                PaymentStatus = BookingPaymentStatuses.Authorized
            };
            
            var bookingDocumentsService = CreateBookingDocumentsService();

            var (isSuccess, _) = await bookingDocumentsService.GenerateVoucher(booking, default);

            Assert.True(isSuccess);
        }


        private static BookingDocumentsService CreateBookingDocumentsService()
        {
            var edoContext = MockEdoContextFactory.Create();
            edoContext.Setup(c => c.Agents)
                .Returns(DbSetMockProvider.GetDbSetMock(new[]
                {
                    new Agent
                    {
                        FirstName = "Test",
                        LastName = "Test",
                        Id = 1
                    }
                }));
            
            return new BookingDocumentsService(
                edoContext.Object,
                Mock.Of<IOptions<BankDetails>>(),
                Mock.Of<IAccommodationService>(),
                Mock.Of<IInvoiceService>(),
                Mock.Of<IReceiptService>(),
                Mock.Of<IImageFileService>(),
                Mock.Of<IAdminAgencyManagementService>());
        }
    }
}