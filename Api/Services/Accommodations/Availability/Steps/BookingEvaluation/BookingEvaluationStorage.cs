using System;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using FloxDc.CacheFlow;
using FloxDc.CacheFlow.Extensions;
using HappyTravel.Edo.Api.Models.Accommodations;
using HappyTravel.Edo.Api.Models.Availabilities;
using HappyTravel.Edo.Api.Models.Markups;
using HappyTravel.Edo.Data.Bookings;
using HappyTravel.SupplierOptionsProvider;
using AccommodationInfo = HappyTravel.Edo.Api.Models.Accommodations.AccommodationInfo;

namespace HappyTravel.Edo.Api.Services.Accommodations.Availability.Steps.BookingEvaluation
{
    public class BookingEvaluationStorage : IBookingEvaluationStorage
    {
        public BookingEvaluationStorage(IDoubleFlow doubleFlow, ISupplierOptionsStorage supplierOptionsStorage)
        {
            _doubleFlow = doubleFlow;
            _supplierOptionsStorage = supplierOptionsStorage;
        }


        public Task Set(Guid searchId, Guid roomContractSetId, string htId, DataWithMarkup<RoomContractSetAvailability> availability, Deadline agentDeadline,
            Deadline supplierDeadline, CreditCardRequirement? cardRequirement, string supplierAccommodationCode, AvailabilityRequest availabilityRequest)
        {
            var accommodation = availability.Data.Accommodation;
            var key = BuildKey(searchId, htId, roomContractSetId);
            var roomSetAvailability = availability.Data;
            var location = accommodation.Location;
            var (_, isFailure, supplier, _) = _supplierOptionsStorage.Get(availability.Data.RoomContractSet.SupplierCode);
            if (isFailure)
                return Task.CompletedTask;

            var bookingAvailabilityInfo = new BookingAvailabilityInfo(
                accommodationId: supplierAccommodationCode,
                accommodationName: accommodation.Name,
                accommodationInfo: new AccommodationInfo(accommodation.Photo),
                roomContractSet: roomSetAvailability.RoomContractSet,
                zoneName: location.LocalityZone,
                localityName: location.Locality,
                countryName: location.Country,
                countryCode: location.CountryCode,
                address: location.Address,
                coordinates: location.Coordinates,
                checkInDate: roomSetAvailability.CheckInDate,
                checkOutDate: roomSetAvailability.CheckOutDate,
                numberOfNights: roomSetAvailability.NumberOfNights,
                supplierCode: supplier.Code,
                appliedMarkups: availability.AppliedMarkups,
                convertedSupplierPrice: availability.ConvertedSupplierPrice,
                originalSupplierPrice: availability.OriginalSupplierPrice,
                availabilityId: roomSetAvailability.AvailabilityId,
                htId: htId,
                availablePaymentTypes: roomSetAvailability.AvailablePaymentMethods,
                isDirectContract: roomSetAvailability.RoomContractSet.IsDirectContract,
                agentDeadline: agentDeadline,
                supplierDeadline: supplierDeadline,
                cardRequirement: cardRequirement,
                availabilityRequest: availabilityRequest);
            
            return _doubleFlow.SetAsync(key, bookingAvailabilityInfo, CacheExpirationTime);
        }


        public async Task<Result<BookingAvailabilityInfo>> Get(Guid searchId, string htId, Guid roomContractSetId)
        {
            var key = BuildKey(searchId, htId, roomContractSetId);
            
            var result = await _doubleFlow.GetAsync<BookingAvailabilityInfo>(key, CacheExpirationTime);
            return result.Equals(default) 
                ? Result.Failure<BookingAvailabilityInfo>("Could not find evaluation result") 
                : result;
        }

        
        private string BuildKey(Guid searchId, string htId, Guid roomContractSetId) 
            => _doubleFlow.BuildKey(searchId.ToString(), htId, roomContractSetId.ToString());
        
        
        private static readonly TimeSpan CacheExpirationTime = TimeSpan.FromMinutes(15);
        private readonly IDoubleFlow _doubleFlow;
        private readonly ISupplierOptionsStorage _supplierOptionsStorage;
    }
}