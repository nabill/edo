using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.AdministratorServices;
using HappyTravel.Edo.Api.Infrastructure.Options;
using HappyTravel.Edo.Api.Models.Bookings;
using HappyTravel.Edo.Api.Services.Accommodations.Availability.Mapping;
using HappyTravel.Edo.Api.Services.Accommodations.Bookings.Documents;
using HappyTravel.Edo.Api.Services.Documents;
using HappyTravel.Edo.Api.Services.Files;
using HappyTravel.Edo.Common.Enums;
using HappyTravel.Edo.Data.Bookings;
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
        [Fact]
        public async Task When_invoice_not_found_should_fail()
        {
            var booking = new Booking
            {
                Id = 1,
                AgentId = 1,
                Status = BookingStatuses.Pending
            };
            
            var bookingDocumentsService = CreateBookingDocumentsService(false);

            var (isSuccess, _) = await bookingDocumentsService.GetActualInvoice(booking);

            Assert.False(isSuccess);
        }


        private static BookingDocumentsService CreateBookingDocumentsService(bool hasInvoices)
        {
            var edoContext = MockEdoContextFactory.Create();

            var invoices = hasInvoices
                ? new List<(DocumentRegistrationInfo Metadata, BookingInvoiceData Data)>
                {
                    (
                        new DocumentRegistrationInfo(It.IsAny<string>(), It.IsAny<DateTime>()),
                        new BookingInvoiceData(
                            new BookingInvoiceData.BuyerInfo(),
                            new BookingInvoiceData.SellerInfo(),
                            It.IsAny<string>(),
                            It.IsAny<string>(),
                            new List<BookingInvoiceData.InvoiceItemInfo>(),
                            new MoneyAmount(),
                            It.IsAny<DateTime>(),
                            It.IsAny<DateTime>(),
                            It.IsAny<DateTime>(),
                            It.IsAny<DateTime?>())
                    )
                }
                : new List<(DocumentRegistrationInfo Metadata, BookingInvoiceData Data)>();

            var invoiceServiceMock = new Mock<IInvoiceService>();
            invoiceServiceMock.Setup(i => i.Get<BookingInvoiceData>(It.IsAny<ServiceTypes>(), It.IsAny<ServiceSource>(), It.IsAny<string>()))
                .ReturnsAsync(invoices);

            return new BookingDocumentsService(
                edoContext.Object,
                Mock.Of<IOptions<BankDetails>>(),
                Mock.Of<IAccommodationMapperClient>(),
                invoiceServiceMock.Object,
                Mock.Of<IReceiptService>(),
                Mock.Of<IImageFileService>(),
                Mock.Of<IAdminAgencyManagementService>());
        }
    }
}