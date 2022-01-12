using System;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using FloxDc.CacheFlow;
using FloxDc.CacheFlow.Extensions;
using HappyTravel.Edo.Api.Models.Accommodations;
using HappyTravel.Edo.Api.Models.Markups;
using HappyTravel.Edo.Api.Services.Accommodations.Availability.Steps.WideAvailabilitySearch;
using HappyTravel.Edo.Data.Bookings;
using AccommodationInfo = HappyTravel.Edo.Api.Models.Accommodations.AccommodationInfo;

namespace HappyTravel.Edo.Api.Services.Accommodations.Availability.Steps.BookingEvaluation
{
    public class BookingEvaluationStorage : IBookingEvaluationStorage
    {
        public BookingEvaluationStorage(IDoubleFlow doubleFlow, IAvailabilityRequestStorage availabilityRequestStorage)
        {
            _doubleFlow = doubleFlow;
            _availabilityRequestStorage = availabilityRequestStorage;
        }


        public async Task<Result> Set(Guid searchId, Guid roomContractSetId, string htId, DataWithMarkup<RoomContractSetAvailability> availability, Deadline agentDeadline,
            Deadline supplierDeadline, CreditCardRequirement? cardRequirement, string supplierAccommodationCode)
        {
            var accommodation = availability.Data.Accommodation;
            var key = BuildKey(searchId, htId, roomContractSetId);
            var roomSetAvailability = availability.Data;
            var location = accommodation.Location;
            var availabilityRequest = await _availabilityRequestStorage.Get(searchId);
            if (availabilityRequest.IsFailure)
                return Result.Failure(availabilityRequest.Error);

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
                supplier: availability.Data.RoomContractSet.Supplier.Value,
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
                availabilityRequest: availabilityRequest.Value);
            
            await _doubleFlow.SetAsync(key, bookingAvailabilityInfo, CacheExpirationTime);
            return Result.Success();
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
        private readonly IAvailabilityRequestStorage _availabilityRequestStorage;
    }
}