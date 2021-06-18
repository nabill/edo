using HappyTravel.Edo.Api.AdministratorServices.Models;

namespace HappyTravel.Edo.Api.Models.Management.AuditEvents
{
    public readonly struct DiscountEditEventData
    {
        public DiscountEditEventData(int agencyId, EditDiscountRequest editDiscountRequest)
        {
            AgencyId = agencyId;
            EditDiscountRequest = editDiscountRequest;
        }


        public int AgencyId { get; }

        public EditDiscountRequest EditDiscountRequest { get; }
    }
}
