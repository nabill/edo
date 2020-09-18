using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace HappyTravel.Edo.Common.Enums
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum DisplayedPaymentOptionsSettings
    {
        CreditCardAndBankTransfer = 0,
        CreditCard = 1,
    }
}
