using Newtonsoft.Json;

namespace Api.Models.Agencies
{
    public readonly struct AddAccountManagerRequest
    {
        [JsonConstructor]
        public AddAccountManagerRequest(int? accountManagerId)
        {
            AccountManagerId = accountManagerId;
        }

        /// <summary>
        ///    Admin's id who will be set as account manager.
        /// </summary>
        public int? AccountManagerId { get; }
    }
}