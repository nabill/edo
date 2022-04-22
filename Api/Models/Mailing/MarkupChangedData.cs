using System;
using HappyTravel.Edo.Api.Models.Mailing;

namespace Api.Models.Mailing
{
    public class MarkupChangedData : DataWithCompanyInfo
    {
        public string PercentChanged { get; set; }
        public DateTimeOffset Modified { get; set; }
        public string? AgencyId { get; set; }
    }
}