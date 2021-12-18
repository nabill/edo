using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.AdministratorServices;
using HappyTravel.Edo.Api.Infrastructure.Options;
using HappyTravel.Edo.Api.Models.Bookings;
using HappyTravel.Edo.Api.Services.Accommodations;
using HappyTravel.Edo.Api.Services.Accommodations.Availability.Mapping;
using HappyTravel.Edo.Api.Services.Accommodations.Bookings.Documents;
using HappyTravel.Edo.Api.Services.Agents;
using HappyTravel.Edo.Api.Services.Documents;
using HappyTravel.Edo.Api.Services.Files;
using HappyTravel.Edo.Common.Enums;
using HappyTravel.Edo.Data.Bookings;
using HappyTravel.Edo.Data.Documents;
using HappyTravel.Edo.UnitTests.Utility;
using HappyTravel.MapperContracts.Public.Accommodations;
using HappyTravel.MapperContracts.Public.Accommodations.Enums;
using HappyTravel.MapperContracts.Public.Accommodations.Internals;
using HappyTravel.Money.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;
using ImageInfo = HappyTravel.MapperContracts.Public.Accommodations.Internals.ImageInfo;

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

            var accommodation = new Accommodation(htId: string.Empty,
                name: string.Empty,
                accommodationAmenities: new List<string>(0),
                additionalInfo: new Dictionary<string, string>(),
                category: string.Empty,
                contacts: new ContactInfo(),
                location: new LocationInfo(countryCode: string.Empty,
                    countryHtId: string.Empty,
                    country: string.Empty,
                    localityHtId: string.Empty,
                    locality: string.Empty,
                    localityZoneHtId: string.Empty,
                    localityZone: string.Empty,
                    coordinates: new GeoPoint(0d, 0d),
                    address: string.Empty,
                    locationDescriptionCode: new LocationDescriptionCodes(),
                    new List<PoiInfo>(0)),
                photos: new List<ImageInfo>(),
                AccommodationRatings.NotRated,
                schedule: new ScheduleInfo(checkInTime: string.Empty,
                    checkOutTime: string.Empty),
                textualDescriptions: new List<TextualDescription>(0),
                type: PropertyTypes.Hotels,
                suppliers: new List<SupplierInfo>(0),
                modified: DateTime.UtcNow);

            var invoiceServiceMock = new Mock<IInvoiceService>();
            invoiceServiceMock.Setup(i => i.Get<BookingInvoiceData>(It.IsAny<ServiceTypes>(), It.IsAny<ServiceSource>(), It.IsAny<string>()))
                .ReturnsAsync(invoices);

            var accommodationMapperClient = new Mock<IAccommodationMapperClient>();
            accommodationMapperClient.Setup(c => c.GetAccommodation(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(accommodation);

            return new BookingDocumentsService(
                edoContext.Object,
                Mock.Of<IOptions<BankDetails>>(),
                accommodationMapperClient.Object,
                invoiceServiceMock.Object,
                Mock.Of<IReceiptService>(),
                Mock.Of<IImageFileService>(),
                Mock.Of<IAdminAgencyManagementService>());
        }
    }
}