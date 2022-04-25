using HappyTravel.Edo.Api.Models.Mailing;
using HappyTravel.Edo.Data.Markup;
using HappyTravel.DataFormatters;
using HappyTravel.Edo.Common.Enums.Markup;

namespace Api.Models.Mailing
{
    public class MarkupChangedData : DataWithCompanyInfo
    {
        public MarkupChangedData(MarkupPolicy policy, MarkupPolicyEventOperationType operationType)
        {
            MarkupId = policy.Id.ToString();
            OldDescription = policy.Description;
            OldPercent = policy.Value.ToString();
            LocationScopeId = policy.SubjectScopeId;
            LocationScopeType = EnumFormatters.FromDescription(policy.SubjectScopeType);
            DestinationScopeId = policy.DestinationScopeId;
            DestinationScopeType = EnumFormatters.FromDescription(policy.DestinationScopeType);
            OperationType = operationType;
            Modified = DateTimeFormatters.ToDateString(policy.Modified);
        }


        public MarkupChangedData(MarkupChangedData changedData, MarkupPolicy policy)
        {
            MarkupId = changedData.MarkupId;
            OldDescription = changedData.OldDescription;
            OldPercent = changedData.OldPercent;
            LocationScopeId = changedData.LocationScopeId;
            LocationScopeType = changedData.LocationScopeType;
            DestinationScopeId = changedData.DestinationScopeId;
            DestinationScopeType = changedData.DestinationScopeType;
            OperationType = changedData.OperationType;

            Modified = DateTimeFormatters.ToDateString(policy.Modified);
            NewDescription = (OldDescription != policy.Description) ? policy.Description : null;
            NewPercent = (OldPercent != policy.Value.ToString()) ? policy.Value.ToString() : null;
        }


        public string MarkupId { get; set; } = string.Empty;
        public string? OldDescription { get; set; }
        public string? NewDescription { get; set; }
        public string? OldPercent { get; set; }
        public string? NewPercent { get; set; }
        public string? LocationScopeId { get; set; }
        public string? LocationScopeName { get; set; }
        public string? LocationScopeType { get; set; }
        public string? DestinationScopeId { get; set; }
        public string? DestinationScopeName { get; set; }
        public string? DestinationScopeType { get; set; }
        public string Modified { get; set; } = string.Empty;
        public MarkupPolicyEventOperationType OperationType { get; set; } = MarkupPolicyEventOperationType.None;
    }
}