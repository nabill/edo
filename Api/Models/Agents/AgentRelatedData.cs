using Newtonsoft.Json;

namespace HappyTravel.Edo.Api.Models.Agents
{
    public readonly struct AgentRelatedData<TData>
    {
        [JsonConstructor]
        public AgentRelatedData(SlimAgentDescription agent, TData data)
        {
            Agent = agent;
            Data = data;
        }
        
        /// <summary>
        /// Nested data
        /// </summary>
        public TData Data { get; }
        
        /// <summary>
        /// Slim agent information
        /// </summary>
        public SlimAgentDescription Agent { get; }
    }
}