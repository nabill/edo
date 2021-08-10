using HappyTravel.DataFormatters;
using HappyTravel.Edo.Api.Models.Reports;

namespace HappyTravel.Edo.Api.Services.Reports.Converters
{
    public class VccBookingDataConverter : IConverter<VccBookingData, VccBookingRow>
    {
        public VccBookingRow Convert(VccBookingData data)
            => new VccBookingRow
            {
                GuestName = data.GuestName,
                ReferenceCode = data.ReferenceCode,
                CheckingDate = DateTimeFormatters.ToDateString(data.CheckingDate),
                CheckOutDate = DateTimeFormatters.ToDateString(data.CheckOutDate),
                Amount = data.Amount,
                Currency = EnumFormatters.FromDescription(data.Currency),
                CardActivationDate = DateTimeFormatters.ToDateString(data.CardActivationDate),
                CardDueDate = DateTimeFormatters.ToDateString(data.CardDueDate),
                CardNumber = data.CardNumber,
                CardAmount = data.CardAmount
            };
    }
}