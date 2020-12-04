using Newtonsoft.Json;

namespace HappyTravel.Edo.Api.Models.Agents
{
    public struct AgentBoundedData<TData>
    {
        [JsonConstructor]
        public AgentBoundedData(SlimAgentDescription agent, TData data)
        {
            Agent = agent;
            Data = data;
        }

        // TODO: replace to readonly struct with init properties after upgrade to C# 9
        
        /// <summary>
        /// Nested data
        /// </summary>
        public TData Data { get; set; }
        
        /// <summary>
        /// Slim agent information
        /// </summary>
        public SlimAgentDescription Agent { get; set; }
    }
}