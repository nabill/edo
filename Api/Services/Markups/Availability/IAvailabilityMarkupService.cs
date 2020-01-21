using System.Threading.Tasks;
using HappyTravel.Edo.Api.Models.Customers;
using HappyTravel.Edo.Api.Models.Markups.Availability;
using HappyTravel.EdoContracts.Accommodations;

namespace HappyTravel.Edo.Api.Services.Markups.Availability
{
    public interface IAvailabilityMarkupService
    {
        Task<AvailabilityDetailsWithMarkup> Apply(CustomerInfo customerInfo, AvailabilityDetails supplierResponse);

        Task<SingleAccommodationAvailabilityDetailsWithMarkup> Apply(CustomerInfo customerInfo, SingleAccommodationAvailabilityDetails supplierResponse);
    }
}