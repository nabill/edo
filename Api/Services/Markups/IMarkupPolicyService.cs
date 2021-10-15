using System.Collections.Generic;
using System.Threading.Tasks;
using HappyTravel.Edo.Api.Services.Markups.Abstractions;
using HappyTravel.Edo.Common.Enums.Markup;
using HappyTravel.Edo.Data.Markup;

namespace HappyTravel.Edo.Api.Services.Markups
{
    public interface IMarkupPolicyService
    {
        List<MarkupPolicy> Get(MarkupSubjectInfo subjectInfo, MarkupObjectInfo objectInfo, MarkupPolicyTarget policyTarget, List<int> agencyTreeIds);
    }
}