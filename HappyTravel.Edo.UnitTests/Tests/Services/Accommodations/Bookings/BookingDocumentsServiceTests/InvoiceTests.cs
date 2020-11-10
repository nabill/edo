using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HappyTravel.Edo.Api.Infrastructure;
using HappyTravel.Edo.Api.Infrastructure.Options;
using HappyTravel.Edo.Api.Models.Agents;
using HappyTravel.Edo.Api.Models.Bookings;
using HappyTravel.Edo.Api.Services.Accommodations;
using HappyTravel.Edo.Api.Services.Accommodations.Availability;
using HappyTravel.Edo.Api.Services.Accommodations.Bookings;
using HappyTravel.Edo.Api.Services.Agents;
using HappyTravel.Edo.Api.Services.CodeProcessors;
using HappyTravel.Edo.Api.Services.Documents;
using HappyTravel.Edo.Common.Enums;
using HappyTravel.Edo.Data.Booking;
using HappyTravel.Edo.Data.Documents;
using HappyTravel.Edo.UnitTests.Utility;
using HappyTravel.Money.Models;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace HappyTravel.Edo.UnitTests.Tests.Services.Accommodations.Bookings.BookingDocumentsServiceTests
{
    public class InvoiceTests
    {
        public InvoiceTests()
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

            var invoiceServiceMock = new Mock<IInvoiceService>();
            invoiceServiceMock.Setup(i => i.Get<BookingInvoiceData>(It.IsAny<ServiceTypes>(), It.IsAny<ServiceSource>(), "1"))
                .ReturnsAsync(() => new List<(DocumentRegistrationInfo Metadata, BookingInvoiceData Data)>
                {
                    (
                        new DocumentRegistrationInfo(It.IsAny<string>(), It.IsAny<DateTime>()),
                        new BookingInvoiceData(
                            new BookingInvoiceData.BuyerInfo(),
                            new BookingInvoiceData.SellerInfo(),
                            It.IsAny<string>(),
                            new List<BookingInvoiceData.InvoiceItemInfo>(),
                            new MoneyAmount(),
                            It.IsAny<DateTime>())
                    )
                });
            invoiceServiceMock.Setup(i => i.Get<BookingInvoiceData>(It.IsAny<ServiceTypes>(), It.IsAny<ServiceSource>(), "2"))
                .ReturnsAsync(() => new List<(DocumentRegistrationInfo Metadata, BookingInvoiceData Data)>());

            _bookingDocumentsService = new BookingDocumentsService(
                edoContext.Object,
                Mock.Of<IOptions<BankDetails>>(),
                bookingRecordManager,
                Mock.Of<IAccommodationService>(),
                Mock.Of<ICounterpartyService>(),
                invoiceServiceMock.Object,
                Mock.Of<IReceiptService>());
        }


        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        public async Task When_booking_has_not_allowed_status_generation_invoice_should_error(int bookingId)
        {
            var result = await _bookingDocumentsService.GetActualInvoice(bookingId, _agentContext);
            Assert.False(result.IsSuccess);
        }


        [Theory]
        [InlineData(3)]
        [InlineData(4)]
        public async Task When_booking_has_allowed_status_generation_invoice_should_success(int bookingId)
        {
            var result = await _bookingDocumentsService.GetActualInvoice(bookingId, _agentContext);
            Assert.True(result.IsSuccess);
        }


        [Theory]
        [InlineData(5)]
        public async Task When_invoice_not_found_should_error(int bookingId)
        {
            var result = await _bookingDocumentsService.GetActualInvoice(bookingId, _agentContext);
            Assert.False(result.IsSuccess);
        }


        private static readonly List<Booking> Bookings = new List<Booking>
        {
            new Booking
            {
                Id = 1,
                AgentId = 1,
                Status = BookingStatuses.Cancelled,
                Rooms = new List<BookedRoom>(),
                ReferenceCode = "1"
            },
            new Booking
            {
                Id = 2,
                AgentId = 1,
                Status = BookingStatuses.Rejected,
                Rooms = new List<BookedRoom>(),
                ReferenceCode = "1"
            },
            new Booking
            {
                Id = 3,
                AgentId = 1,
                Status = BookingStatuses.Confirmed,
                Rooms = new List<BookedRoom>(),
                ReferenceCode = "1"
            },
            new Booking
            {
                Id = 4,
                AgentId = 1,
                Status = BookingStatuses.Pending,
                Rooms = new List<BookedRoom>(),
                ReferenceCode = "1"
            },
            new Booking
            {
                Id = 5,
                AgentId = 1,
                Status = BookingStatuses.Pending,
                Rooms = new List<BookedRoom>(),
                ReferenceCode = "2"
            }
        };

        private readonly AgentContext _agentContext;
        private readonly BookingDocumentsService _bookingDocumentsService;
    }
}