using System.Collections.Generic;

namespace HappyTravel.Edo.Api.Infrastructure.Options
{
    public class BookingMailingOptions
    {
        public string VoucherTemplateId { get; set; }
        public string InvoiceTemplateId { get; set; }
        public string BookingCancelledTemplateId { get; set; }
        public string DeadlineNotificationTemplateId { get; set; }
        public string ReservationsBookingFinalizedTemplateId { get; set; }
        public string BookingSummaryTemplateId { get; set; }
        public List<string> CcNotificationAddresses { get; set; }
    }
}