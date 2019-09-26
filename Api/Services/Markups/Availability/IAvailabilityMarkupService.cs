using System.Threading.Tasks;
using HappyTravel.Edo.Api.Models.Availabilities;
using HappyTravel.Edo.Api.Services.Customers;

namespace HappyTravel.Edo.Api.Services.Markups.Availability
{
    public interface IAvailabilityMarkupService
    {
        Task<AvailabilityResponseWithMarkup> Apply(CustomerInfo customerInfo, AvailabilityResponse supplierResponse);
    }
}