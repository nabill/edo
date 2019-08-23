using System;
using System.Linq;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Infrastructure;
using HappyTravel.Edo.Api.Models.Bookings;
using HappyTravel.Edo.Api.Services.Customers;
using HappyTravel.Edo.Common.Enums;
using HappyTravel.Edo.Data;
using HappyTravel.Edo.Data.Booking;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace HappyTravel.Edo.Api.Services.Accommodations
{
    public class AccommodationBookingManager : IAccommodationBookingManager
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
            var (_, isCustomerFailure, customer, customerError) = await _customerContext.GetCurrent();
            if (isCustomerFailure)
                return BuildFailResult(customerError);

            var (_, isCompanyFailure, companyId, companyError) = await GetCompanyId(bookingRequest.CompanyId, customer.Id);
            if(isCompanyFailure)
                return BuildFailResult(companyError);
            
            var availability = await GetSelectedAvailability(bookingRequest.AvailabilityId, bookingRequest.AgreementId);
            if(availability.Equals(default))
                return BuildFailResult("Could not find availability by given id");

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

            Result<AccommodationBookingDetails, ProblemDetails> BuildFailResult(string s)
            {
                return ProblemDetailsBuilder.BuildFailResult<AccommodationBookingDetails>(s);
            }

            Task SaveBookingResult(AccommodationBookingDetails confirmedBooking)
            {
                var booking = new AccommodationBooking()
                    .AddDate(_dateTimeProvider)
                    .AddCustomerInformation(customer, companyId)
                    .AddReferences(itn, referenceCode)
                    .AddRequestInfo(bookingRequest)
                    .AddConfirmedDetails(confirmedBooking)
                    .AddConditions(availability);

                _context.AccommodationBookings.Add(booking);
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

        private async Task<Result<int>> GetCompanyId(int? bookingRequestCompanyId, int customerId)
        {
            if (bookingRequestCompanyId.HasValue)
            {
                var companyId = bookingRequestCompanyId.Value;
                var company = await _context.CustomerCompanyRelations
                    .SingleOrDefaultAsync(cr => cr.CustomerId == customerId && cr.CompanyId == companyId);

                if (!(company is null))
                    return Result.Ok(companyId);
            }
            else
            {
                var relatedCompanies = await _context.CustomerCompanyRelations
                    .Where(cr => cr.CustomerId == customerId)
                    .ToListAsync();
                
                if(relatedCompanies.Count == 1)
                    return Result.Ok(relatedCompanies.Single().CompanyId);
            }
            
            return Result.Fail<int>("Could not get associated company");
        }

        private readonly EdoContext _context;
        private readonly IAvailabilityResultsCache _availabilityResultsCache;
        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly ICustomerContext _customerContext;
        private readonly IDataProviderClient _dataProviderClient;
        private readonly DataProviderOptions _options;
    }
}