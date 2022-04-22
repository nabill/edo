using HappyTravel.Edo.Data.Markup;

namespace Api.Infrastructure.ModelExtensions
{
    public static class MarkupPolicyExtensions
    {
        public static MarkupPolicy Clone(this MarkupPolicy policy)
            => new MarkupPolicy()
            {
                Id = policy.Id,
                Description = policy.Description,
                Created = policy.Created,
                Modified = policy.Modified,
                Currency = policy.Currency,
                SubjectScopeType = policy.SubjectScopeType,
                SubjectScopeId = policy.SubjectScopeId,
                DestinationScopeType = policy.DestinationScopeType,
                DestinationScopeId = policy.DestinationScopeId,
                FunctionType = policy.FunctionType,
                Value = policy.Value
            };
    }
}