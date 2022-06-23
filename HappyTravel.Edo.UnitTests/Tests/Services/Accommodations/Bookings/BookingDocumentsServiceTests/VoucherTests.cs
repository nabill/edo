using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.AdministratorServices;
using HappyTravel.Edo.Api.Infrastructure.Options;
using HappyTravel.Edo.Api.Services.Accommodations.Availability.Mapping;
using HappyTravel.Edo.Api.Services.Accommodations.Bookings.Documents;
using HappyTravel.Edo.Api.Services.Company;
using HappyTravel.Edo.Api.Services.Documents;
using HappyTravel.Edo.Api.Services.Files;
using HappyTravel.Edo.Common.Enums;
using HappyTravel.Edo.Data.Agents;
using HappyTravel.Edo.Data.Bookings;
using HappyTravel.Edo.UnitTests.Utility;
using HappyTravel.MapperContracts.Public.Accommodations;
using HappyTravel.MapperContracts.Public.Accommodations.Enums;
using HappyTravel.MapperContracts.Public.Accommodations.Internals;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;
using ImageInfo = HappyTravel.MapperContracts.Public.Accommodations.Internals.ImageInfo;

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
                PaymentStatus = BookingPaymentStatuses.Authorized,
                SpecialValues = new List<KeyValuePair<string, string>>() { }
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

            var accommodationMapperClient = new Mock<IAccommodationMapperClient>();
            accommodationMapperClient.Setup(c => c.GetAccommodation(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(accommodation);

            return new BookingDocumentsService(
                edoContext.Object,
                accommodationMapperClient.Object,
                Mock.Of<IInvoiceService>(),
                Mock.Of<IReceiptService>(),
                Mock.Of<IImageFileService>(),
                Mock.Of<IAdminAgencyManagementService>(),
                Mock.Of<ICompanyService>());
        }
    }
}