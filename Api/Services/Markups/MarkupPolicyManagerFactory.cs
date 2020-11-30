using System;
using HappyTravel.Edo.Common.Enums.Markup;

namespace HappyTravel.Edo.Api.Services.Markups
{
    public class MarkupPolicyManagerFactory : IMarkupPolicyManagerFactory
    {
        public MarkupPolicyManagerFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }


        public IMarkupPolicyManager Get(MarkupPolicyManagerTypes type)
        {
            return type switch
            {
                MarkupPolicyManagerTypes.Agent => (IMarkupPolicyManager) _serviceProvider.GetService(typeof(AgentMarkupPolicyManager)),
                MarkupPolicyManagerTypes.Administrator => (IMarkupPolicyManager) _serviceProvider.GetService(typeof(AdminMarkupPolicyManager)),
                _ => throw new ArgumentOutOfRangeException($"Not supported type {type}")
            };
        }


        private readonly IServiceProvider _serviceProvider;
    }
}