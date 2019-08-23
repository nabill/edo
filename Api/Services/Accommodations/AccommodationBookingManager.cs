using System;
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
            IDateTimeProvider dateTimeProvider,
            ICustomerContext customerContext)
        {
            _dataProviderClient = dataProviderClient;
            _options = options.Value;
            _context = context;
            _dateTimeProvider = dateTimeProvider;
            _customerContext = customerContext;
        }

        public async Task<Result<AccommodationBookingDetails, ProblemDetails>> Book(AccommodationBookingRequest bookingRequest, 
            BookingAvailabilityInfo availability,
            string languageCode)
        {
            var (_, isCustomerFailure, customer, customerError) = await _customerContext.GetCustomer();
            if (isCustomerFailure)
                return ProblemDetailsBuilder.Fail<AccommodationBookingDetails>(customerError);

            var (_, isCompanyFailure, company, companyError) = await _customerContext.GetCompany();
            if(isCompanyFailure)
                return ProblemDetailsBuilder.Fail<AccommodationBookingDetails>(companyError);

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
        
        private readonly EdoContext _context;
        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly ICustomerContext _customerContext;
        private readonly IDataProviderClient _dataProviderClient;
        private readonly DataProviderOptions _options;
    }
}