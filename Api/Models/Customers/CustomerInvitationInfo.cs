using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace HappyTravel.Edo.Api.Models.Customers
{
    public readonly struct CustomerInvitationInfo
    {
        [JsonConstructor]
        public CustomerInvitationInfo(CustomerEditableInfo registrationInfo, int counterpartyId, string email)
        {
            RegistrationInfo = registrationInfo;
            CounterpartyId = counterpartyId;
            Email = email;
        }


        /// <summary>
        ///     Regular customer personal information.
        /// </summary>
        [Required]
        public CustomerEditableInfo RegistrationInfo { get; }

        /// <summary>
        ///     Related counterparty id.
        /// </summary>
        [Required]
        public int CounterpartyId { get; }

        /// <summary>
        ///     E-mail for invitation.
        /// </summary>
        [Required]
        public string Email { get; }
    }
}