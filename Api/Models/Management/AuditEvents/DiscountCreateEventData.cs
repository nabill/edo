using HappyTravel.Edo.Api.AdministratorServices.Models;

namespace HappyTravel.Edo.Api.Models.Management.AuditEvents
{
    public readonly struct DiscountCreateEventData
    {
        public DiscountCreateEventData(int agencyId, CreateDiscountRequest createDiscountRequest)
        {
            AgencyId = agencyId;
            CreateDiscountRequest = createDiscountRequest;
        }


        public int AgencyId { get; }

        public CreateDiscountRequest CreateDiscountRequest { get; }
    }
}
