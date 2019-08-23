using System;
using System.Linq;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Infrastructure;
using HappyTravel.Edo.Api.Models.Bookings;
using HappyTravel.Edo.Api.Services.Customers;
using HappyTravel.Edo.Common.Enums;
using HappyTravel.Edo.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace HappyTravel.Edo.Api.Services.Accommodations
{
    internal class AccommodationBookingManager : IAccommodationBookingManager
    {
        public AccommodationBookingManager(IOptions<DataProviderOptions> options,
            IDataProviderClient dataProviderClient, 
            EdoContext context,
            IAvailabilityResultsCache availabilityResultsCache,
            IDateTimeProvider dateTimeProvider,
            ICustomerContext customerContext)
        {
            _dataProviderClient = dataProviderClient;
            _options = options.Value;
            _context = context;
            _availabilityResultsCache = availabilityResultsCache;
            _dateTimeProvider = dateTimeProvider;
            _customerContext = customerContext;
        }

        public async Task<Result<AccommodationBookingDetails, ProblemDetails>> Book(AccommodationBookingRequest bookingRequest,
            string languageCode)
        {
            var (_, isCustomerFailure, customer, customerError) = await _customerContext.GetCustomer();
            if (isCustomerFailure)
                return ProblemDetailsBuilder.Fail<AccommodationBookingDetails>(customerError);

            var (_, isCompanyFailure, company, companyError) = await _customerContext.GetCompany();
            if(isCompanyFailure)
                return ProblemDetailsBuilder.Fail<AccommodationBookingDetails>(companyError);
            
            var availability = await GetSelectedAvailability(bookingRequest.AvailabilityId, bookingRequest.AgreementId);
            if(availability.Equals(default))
                return ProblemDetailsBuilder.Fail<AccommodationBookingDetails>("Could not find availability by given id");

            var itn = bookingRequest.ItineraryNumber ?? await _context.GetNextItineraryNumber();
            var referenceCode = ReferenceCodeGenerator.Generate(ServiceTypes.HTL,
                availability.SelectedResult.AccommodationDetails.Location.CountryCode,
                itn);
            
            return await ExecuteBookingRequest()
                .OnSuccess(async confirmedBooking => await SaveBookingResult(confirmedBooking));
            
            Task<Result<AccommodationBookingDetails, ProblemDetails>> ExecuteBookingRequest()
            {
                var innerRequest = new InnerAccommodationBookingRequest(bookingRequest,
                    availability, referenceCode);
                
                return _dataProviderClient.Post<InnerAccommodationBookingRequest, AccommodationBookingDetails>(
                    new Uri(_options.Netstorming + "hotels/booking", UriKind.Absolute),
                    innerRequest, languageCode);
            }

            Task SaveBookingResult(AccommodationBookingDetails confirmedBooking)
            {
                var booking = new AccommodationBookingBuilder()
                    .AddCustomerInformation(customer, company)
                    .AddTags(itn, referenceCode)
                    .AddRequestInfo(bookingRequest)
                    .AddConfirmedDetails(confirmedBooking)
                    .AddServiceDetails(availability.SelectedAgreement)
                    .AddCreatedDate(_dateTimeProvider.UtcNow())
                    .Build();

                _context.Bookings.Add(booking);
                return _context.SaveChangesAsync();
            }
        }
        
        private async ValueTask<BookingAvailabilityInfo> GetSelectedAvailability(int availabilityId, Guid agreementId)
        {
            var availabilityResponse = await _availabilityResultsCache.Get(availabilityId);
            if (availabilityResponse.Equals(default))
                return default;
                    
            return (from availabilityResult in availabilityResponse.Results
                    from agreement in availabilityResult.Agreements
                    where agreement.Id == agreementId
                    select new BookingAvailabilityInfo(availabilityResponse, availabilityResult, agreement))
                .SingleOrDefault();
        }

        private readonly EdoContext _context;
        private readonly IAvailabilityResultsCache _availabilityResultsCache;
        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly ICustomerContext _customerContext;
        private readonly IDataProviderClient _dataProviderClient;
        private readonly DataProviderOptions _options;
    }
}