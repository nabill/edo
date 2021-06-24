using HappyTravel.Edo.Api.AdministratorServices.Models;

namespace HappyTravel.Edo.Api.Models.Management.AuditEvents
{
    public readonly struct DiscountDeleteEventData
    {
        public DiscountDeleteEventData(int agencyId, DiscountInfo deletedDiscountInfo)
        {
            AgencyId = agencyId;
            DeletedDiscountInfo = deletedDiscountInfo;
        }


        public int AgencyId { get; }

        public DiscountInfo DeletedDiscountInfo { get; }
    }
}
