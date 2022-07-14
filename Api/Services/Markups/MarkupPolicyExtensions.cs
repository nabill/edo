using System.Linq;
using HappyTravel.DataFormatters;
using HappyTravel.Edo.Api.Infrastructure;
using HappyTravel.Edo.Api.Models.Markups;
using HappyTravel.Edo.Common.Enums.Markup;
using HappyTravel.Edo.Data;
using HappyTravel.Edo.Data.Markup;

namespace HappyTravel.Edo.Api.Services.Markups
{
    public static class MarkupPolicyExtensions
    {
        public static MarkupPolicySettings GetSettings(this MarkupPolicy policy, EdoContext context)
        {
            var locationScopeName = GetLocationScopeName(context, policy.SubjectScopeType, policy.SubjectScopeId);
            var destinationScopeName = GetDestinationScopeName(context, policy.DestinationScopeType, policy.DestinationScopeId);

            return new MarkupPolicySettings(policy.FunctionType,
                policy.Value,
                policy.Currency,
                policy.Description ?? string.Empty,
                policy.SubjectScopeId ?? string.Empty,
                locationScopeName,
                policy.SubjectScopeType,
                policy.DestinationScopeId ?? string.Empty,
                destinationScopeName,
                policy.DestinationScopeType,
                policy.SupplierCode);
        }


        public static string? GetLocationScopeName(EdoContext context, SubjectMarkupScopeTypes scopeType, string? subjectScopeId)
            => scopeType switch
            {
                SubjectMarkupScopeTypes.Agency => context.Agencies
                    .Where(a => a.Id.ToString() == subjectScopeId)
                    .Select(a => a.Name)
                    .SingleOrDefault(),

                SubjectMarkupScopeTypes.Agent => context.Agents
                    .Where(a => a.Id.ToString() == subjectScopeId)
                    .Select(a => PersonNameFormatters.ToMaskedName(a.FirstName, a.LastName, null))
                    .SingleOrDefault(),

                SubjectMarkupScopeTypes.Country => context.Countries
                    .Where(c => c.Code == subjectScopeId)
                    .Select(c => c.Names.GetValueOrDefault(LocalizationHelper.DefaultLanguageCode))
                    .SingleOrDefault(),

                SubjectMarkupScopeTypes.Market => context.Markets
                    .Where(m => m.Id.ToString() == subjectScopeId)
                    .Select(m => m.Names.GetValueOrDefault(LocalizationHelper.DefaultLanguageCode))
                    .SingleOrDefault(),

                _ => null
            };


        public static string? GetDestinationScopeName(EdoContext context, DestinationMarkupScopeTypes scopeType, string? destinationScopeId)
            => scopeType switch
            {
                DestinationMarkupScopeTypes.Country => context.Countries
                    .Where(c => c.Code == destinationScopeId)
                    .Select(c => c.Names.GetValueOrDefault(LocalizationHelper.DefaultLanguageCode))
                    .SingleOrDefault(),

                DestinationMarkupScopeTypes.Market => context.Markets
                    .Where(m => m.Id.ToString() == destinationScopeId)
                    .Select(m => m.Names.GetValueOrDefault(LocalizationHelper.DefaultLanguageCode))
                    .SingleOrDefault(),

                _ => null
            };
    }
}