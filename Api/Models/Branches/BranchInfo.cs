using Newtonsoft.Json;

namespace HappyTravel.Edo.Api.Models.Branches
{
    public readonly struct BranchInfo
    {
        [JsonConstructor]
        public BranchInfo(string title)
        {
            Title = title;
        }
        
        /// <summary>
        /// Title of branch.
        /// </summary>
        public string Title { get; }
    }
}