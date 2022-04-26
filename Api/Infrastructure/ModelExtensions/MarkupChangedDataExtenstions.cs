using Api.Models.Mailing;
using HappyTravel.Edo.Common.Enums.Markup;
using HappyTravel.Edo.Data.Markup;

namespace Api.Infrastructure.ModelExtensions
{
    public static class MarkupChangedDataExtenstions
    {
        public static MarkupChangedData FulfillChangedData(this MarkupChangedData data, MarkupPolicy policy)
            => data.OperationType != MarkupPolicyEventOperationType.Modified ?
                data :
                new MarkupChangedData(data, policy);
    }
}