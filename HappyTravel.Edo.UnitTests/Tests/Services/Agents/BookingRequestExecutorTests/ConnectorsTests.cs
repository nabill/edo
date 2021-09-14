using System;
using System.Threading.Tasks;
using HappyTravel.Edo.Api.Infrastructure;
using HappyTravel.Edo.Api.Services.Accommodations.Bookings.BookingExecution;
using HappyTravel.Edo.Api.Services.Accommodations.Bookings.Management;
using HappyTravel.Edo.Api.Services.Accommodations.Bookings.ResponseProcessing;
using HappyTravel.Edo.Api.Services.Analytics;
using HappyTravel.Edo.Api.Services.Connectors;
using HappyTravel.Edo.Data.Bookings;
using HappyTravel.SuppliersCatalog;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using BookingRequest = HappyTravel.EdoContracts.Accommodations.BookingRequest;

namespace HappyTravel.Edo.UnitTests.Tests.Services.Agents.BookingRequestExecutorTests
{
    public class ConnectorsTests
    {
        [Theory]
        [InlineData(Suppliers.Illusions)]
        [InlineData(Suppliers.Etg)]
        [InlineData(Suppliers.Columbus)]
        public async Task Correct_supplier_should_be_passed_when_getting_supplier_connector(Suppliers supplier)
        {
            InitializeMocks();
            var service = CreateBookingRequestExecutor();
            var booking = new Booking { Supplier = supplier };

            await service.Execute(booking, default, default);

            _supplierConnectorManagerMock.Verify(x => x.Get(supplier));
        }


        [Theory]
        [InlineData("1", "2", "aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa", true)]
        [InlineData("3", "4", "aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaab", false)]
        [InlineData("5", "6", "aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaac", true)]
        public async Task Correct_request_should_be_passed_when_booking_on_connector(
            string availabilityId,
            string referenceCode,
            string roomContractSetIdString,
            bool rejectIfUnavailable)
        {
            InitializeMocks();
            var roomContractSetId = new Guid(roomContractSetIdString);
            var service = CreateBookingRequestExecutor();
            var booking = new Booking { ReferenceCode = referenceCode };
            BookingRequest actualInnerRequest = default;
            SetupRequestStorageMock();
            var setupResult = Utility.SetupConnectorBookSuccess(_supplierConnectorMock);
            SaveConnectorBookPassedParameter();

            await service.Execute(booking, default, default);

            _supplierConnectorMock.Verify(x => x.Book(It.IsAny<BookingRequest>(), It.IsAny<string>()));
            Assert.Equal(availabilityId, actualInnerRequest.AvailabilityId);
            Assert.Equal(roomContractSetId, actualInnerRequest.RoomContractSetId);
            Assert.Equal(rejectIfUnavailable, actualInnerRequest.RejectIfUnavailable);
            Assert.Equal(referenceCode, actualInnerRequest.ReferenceCode);


            void SaveConnectorBookPassedParameter() 
                => setupResult.Callback<BookingRequest, string>((req, _) => actualInnerRequest = req);


            void SetupRequestStorageMock()
            {
                var request = Utility.CreateAccommodationBookingRequest(roomContractSetId, rejectIfUnavailable);
                var availabilityInfo = Utility.CreateAvailabilityInfo(availabilityId);
                _requestStorageMock
                    .Setup(x => x.Get(It.IsAny<string>()))
                    .ReturnsAsync((request, availabilityInfo));
            }
        }


        private void InitializeMocks()
        {
            _supplierConnectorManagerMock = new Mock<ISupplierConnectorManager>();
            _responseProcessorMock = new Mock<IBookingResponseProcessor>();
            _bookingAnalyticsServiceMock = new Mock<IBookingAnalyticsService>();
            _bookingRecordsUpdaterMock = new Mock<IBookingRecordsUpdater>();
            _dateTimeProviderMock = new Mock<IDateTimeProvider>();
            _loggerMock = new Mock<ILogger<BookingRequestExecutor>>();
            _supplierConnectorMock = new Mock<ISupplierConnector>();
            _requestStorageMock = new Mock<IBookingRequestStorage>();
            
            var request = Utility.CreateAccommodationBookingRequest();

            _requestStorageMock
                .Setup(x => x.Get(It.IsAny<string>()))
                .ReturnsAsync((request, default));

            _supplierConnectorManagerMock
                .Setup(x => x.Get(It.IsAny<Suppliers>()))
                .Returns(_supplierConnectorMock.Object);
        }


        private BookingRequestExecutor CreateBookingRequestExecutor()
        {
            return new BookingRequestExecutor(
                _supplierConnectorManagerMock.Object,
                _responseProcessorMock.Object,
                _bookingAnalyticsServiceMock.Object,
                _bookingRecordsUpdaterMock.Object,
                _dateTimeProviderMock.Object,
                _requestStorageMock.Object,
                _loggerMock.Object);
        }


#pragma warning disable CS8618
        private Mock<ISupplierConnectorManager> _supplierConnectorManagerMock;
        private Mock<IBookingResponseProcessor> _responseProcessorMock;
        private Mock<IBookingAnalyticsService> _bookingAnalyticsServiceMock;
        private Mock<IBookingRecordsUpdater> _bookingRecordsUpdaterMock;
        private Mock<IDateTimeProvider> _dateTimeProviderMock;
        private Mock<ILogger<BookingRequestExecutor>> _loggerMock;
        private Mock<ISupplierConnector> _supplierConnectorMock;
        private Mock<IBookingRequestStorage> _requestStorageMock;
#pragma warning restore CS8618
   }
}
