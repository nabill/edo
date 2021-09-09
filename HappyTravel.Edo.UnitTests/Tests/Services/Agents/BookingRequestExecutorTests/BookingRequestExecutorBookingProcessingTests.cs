using System;
using System.Threading.Tasks;
using HappyTravel.Edo.Api.Infrastructure;
using HappyTravel.Edo.Api.Models.Users;
using HappyTravel.Edo.Api.Services.Accommodations.Bookings.BookingExecution;
using HappyTravel.Edo.Api.Services.Accommodations.Bookings.Management;
using HappyTravel.Edo.Api.Services.Accommodations.Bookings.ResponseProcessing;
using HappyTravel.Edo.Api.Services.Analytics;
using HappyTravel.Edo.Api.Services.Connectors;
using HappyTravel.Edo.Common.Enums;
using HappyTravel.Edo.Data.Bookings;
using HappyTravel.EdoContracts.Accommodations.Enums;
using HappyTravel.SuppliersCatalog;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace HappyTravel.Edo.UnitTests.Tests.Services.Agents.BookingRequestExecutorTests
{
    public class BookingRequestExecutorBookingProcessingTests
    {
        [Fact]
        public async Task When_success_booking_should_be_passed_to_process_response()
        {
            InitializeMocks();
            var service = CreateBookingRequestExecutor();
            var referenceCode = "RefCode12";
            var booking = new Booking { ReferenceCode = referenceCode };
            var request = Utility.CreateAccommodationBookingRequest();
            EdoContracts.Accommodations.Booking bookingPassedToProcessResponse = default;
            SaveResponseProcessorPassedParameter();
            Utility.SetupConnectorBookSuccess(_supplierConnectorMock);

            await service.Execute(request, default, booking, default, default);

            _responseProcessorMock
                .Verify(x => x.ProcessResponse(It.IsAny<EdoContracts.Accommodations.Booking>(), It.IsAny<ApiCaller>(), It.IsAny<BookingChangeEvents>()));
            Assert.Equal(referenceCode, bookingPassedToProcessResponse.ReferenceCode);


            void SaveResponseProcessorPassedParameter()
                => _responseProcessorMock
                    .Setup(x => x.ProcessResponse(It.IsAny<EdoContracts.Accommodations.Booking>(), It.IsAny<ApiCaller>(), It.IsAny<BookingChangeEvents>()))
                    .Callback<EdoContracts.Accommodations.Booking, ApiCaller, BookingChangeEvents>((b, _, _) => bookingPassedToProcessResponse = b);
        }


        [Fact]
        public async Task When_failure_change_status_to_invalid_should_be_called()
        {
            InitializeMocks();
            var service = CreateBookingRequestExecutor();
            var booking = new Booking();
            var request = Utility.CreateAccommodationBookingRequest();
            var problemDetails = Utility.CreateProblemDetailsWithFailureCode(true, BookingFailureCodes.ConnectorValidationFailed);
            Utility.SetupConnectorBookFailure(_supplierConnectorMock, problemDetails);

            await service.Execute(request, default, booking, default, default);

            _bookingRecordsUpdaterMock
                .Verify(x => x.ChangeStatus(It.IsAny<Booking>(), BookingStatuses.Invalid, It.IsAny<DateTime>(),
                    It.IsAny<ApiCaller>(), It.IsAny<BookingChangeReason>()));
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
#pragma warning restore CS8618
    }
}
