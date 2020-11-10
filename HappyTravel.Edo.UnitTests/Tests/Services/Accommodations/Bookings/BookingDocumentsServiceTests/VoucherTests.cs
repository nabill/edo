using System.Collections.Generic;
using System.Threading.Tasks;
using HappyTravel.Edo.Api.Infrastructure;
using HappyTravel.Edo.Api.Infrastructure.Options;
using HappyTravel.Edo.Api.Models.Agents;
using HappyTravel.Edo.Api.Services.Accommodations;
using HappyTravel.Edo.Api.Services.Accommodations.Availability;
using HappyTravel.Edo.Api.Services.Accommodations.Bookings;
using HappyTravel.Edo.Api.Services.Agents;
using HappyTravel.Edo.Api.Services.CodeProcessors;
using HappyTravel.Edo.Api.Services.Documents;
using HappyTravel.Edo.Common.Enums;
using HappyTravel.Edo.Data.Booking;
using HappyTravel.Edo.UnitTests.Utility;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace HappyTravel.Edo.UnitTests.Tests.Services.Accommodations.Bookings.BookingDocumentsServiceTests
{
    public class VoucherTests
    {
        public VoucherTests()
        {
            var edoContext = MockEdoContextFactory.Create();
            edoContext.Setup(c => c.Bookings)
                .Returns(DbSetMockProvider.GetDbSetMock(Bookings));

            _agentContext = AgentInfoFactory.GetByAgentId(1);

            var bookingRecordManager = new BookingRecordsManager(
                edoContext.Object,
                Mock.Of<IDateTimeProvider>(),
                Mock.Of<ITagProcessor>(),
                Mock.Of<IAccommodationService>(),
                Mock.Of<IAccommodationBookingSettingsService>());

            _bookingDocumentsService = new BookingDocumentsService(
                edoContext.Object,
                Mock.Of<IOptions<BankDetails>>(),
                bookingRecordManager,
                Mock.Of<IAccommodationService>(),
                Mock.Of<ICounterpartyService>(),
                Mock.Of<IInvoiceService>(),
                Mock.Of<IReceiptService>());
        }

        [Theory]
        [InlineData(1)]
        public async Task When_booking_has_not_confirmed_status_generation_voucher_should_error(int bookingId)
        {
            var result = await _bookingDocumentsService.GenerateVoucher(bookingId, _agentContext, default);
            Assert.False(result.IsSuccess);
        }

        [Theory]
        [InlineData(2)]
        public async Task When_booking_has_confirmed_status_and_not_payed_generation_voucher_should_error(int bookingId)
        {
            var result = await _bookingDocumentsService.GenerateVoucher(bookingId, _agentContext, default);
            Assert.False(result.IsSuccess);
        }


        [Theory]
        [InlineData(3)]
        public async Task When_booking_has_confirmed_status_and_not_payed_generation_voucher_should_succeed(int bookingId)
        {
            var result = await _bookingDocumentsService.GenerateVoucher(bookingId, _agentContext, default);
            Assert.True(result.IsSuccess);
        }

        private static readonly List<Booking> Bookings = new List<Booking>
        {
            new Booking
            {
                Id = 1,
                AgentId = 1,
                Status = BookingStatuses.Cancelled,
                PaymentStatus = It.IsAny<BookingPaymentStatuses>(),
                Rooms = new List<BookedRoom>()
            },
            new Booking
            {
                Id = 2,
                AgentId = 1,
                Status = BookingStatuses.Confirmed,
                PaymentStatus = BookingPaymentStatuses.NotPaid,
                Rooms = new List<BookedRoom>()
            },
            new Booking
            {
                Id = 3,
                AgentId = 1,
                Status = BookingStatuses.Confirmed,
                PaymentStatus = BookingPaymentStatuses.Authorized,
                Rooms = new List<BookedRoom>()
            }
        };

        private readonly AgentContext _agentContext;
        private readonly BookingDocumentsService _bookingDocumentsService;
    }
}