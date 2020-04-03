using Newtonsoft.Json;

namespace HappyTravel.Edo.Api.Models.Customers
{
    public struct RegisterCustomerWithCounterpartyRequest
    {
        [JsonConstructor]
        public RegisterCustomerWithCounterpartyRequest(CustomerEditableInfo customer, CounterpartyInfo counterparty)
        {
            Customer = customer;
            Counterparty = counterparty;
        }


        /// <summary>
        ///     Customer personal information.
        /// </summary>
        public CustomerEditableInfo Customer { get; }

        /// <summary>
        ///     Customer affiliated counterparty information.
        /// </summary>
        public CounterpartyInfo Counterparty { get; }
    }
}