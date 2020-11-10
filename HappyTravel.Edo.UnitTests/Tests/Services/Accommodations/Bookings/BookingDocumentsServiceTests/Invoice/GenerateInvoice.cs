using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
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

namespace HappyTravel.Edo.UnitTests.Tests.Services.Accommodations.Bookings.BookingDocumentsServiceTests.Invoice
{
    public class GenerateInvoice
    {
        public GenerateInvoice()
        {
            _bookings = GenerateBookings();

            var edoContext = MockEdoContextFactory.Create();
            edoContext.Setup(c => c.Bookings)
                .Returns(DbSetMockProvider.GetDbSetMock(_bookings));

            _agentContext = AgentInfoFactory.GetByAgentId(1);

            var bookingRecordManager = new BookingRecordsManager(
                edoContext.Object,
                Mock.Of<IDateTimeProvider>(),
                Mock.Of<ITagProcessor>(),
                Mock.Of<IAccommodationService>(),
                Mock.Of<IAccommodationBookingSettingsService>());

            var invoiceServiceMock = new Mock<IInvoiceService>();
            invoiceServiceMock.Setup(i => i.Get<BookingInvoiceData>(It.IsAny<ServiceTypes>(), It.IsAny<ServiceSource>(), It.IsAny<string>()))
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

            _bookingDocumentsService = new BookingDocumentsService(
                edoContext.Object,
                Mock.Of<IOptions<BankDetails>>(),
                bookingRecordManager,
                Mock.Of<IAccommodationService>(),
                Mock.Of<ICounterpartyService>(),
                invoiceServiceMock.Object,
                Mock.Of<IReceiptService>());
        }


        [Fact]
        public async Task generate_invoice_from_not_allowed_booking_status_should_error()
        {
            var ids = GetBookingsIds(b =>
                BookingDocumentsService.NotAvailableForInvoiceStatuses.Contains(b.Status));

            var tasks = ids.Select(i => _bookingDocumentsService.GetActualInvoice(i, _agentContext));
            var results = await Task.WhenAll(tasks);

            Assert.All(results, result => Assert.False(result.IsSuccess));
        }


        [Fact]
        public async Task generate_invoice_from_allowed_booking_status_should_success()
        {
            var ids = GetBookingsIds(b =>
                !BookingDocumentsService.NotAvailableForInvoiceStatuses.Contains(b.Status));

            var tasks = ids.Select(i => _bookingDocumentsService.GetActualInvoice(i, _agentContext));
            var results = await Task.WhenAll(tasks);

            Assert.All(results, result => Assert.True(result.IsSuccess));
        }

        private static List<Booking> GenerateBookings()
        {
            var bookings = new List<Booking>();
            var index = 0;

            foreach (var bookingStatus in (BookingStatuses[]) Enum.GetValues(typeof(BookingStatuses)))
            {
                bookings.Add(new Booking
                {
                    Id = index,
                    AgentId = 1,
                    PaymentStatus = It.IsAny<BookingPaymentStatuses>(),
                    Status = bookingStatus,
                    Rooms = new List<BookedRoom>()
                });

                index++;
            }

            return bookings;
        }


        private IEnumerable<int> GetBookingsIds(Expression<Func<Booking, bool>> filterExpression)
        {
            return _bookings.AsQueryable().Where(filterExpression).Select(b => b.Id);
        }


        private readonly List<Booking> _bookings;
        private readonly AgentContext _agentContext;
        private readonly BookingDocumentsService _bookingDocumentsService;
    }
}