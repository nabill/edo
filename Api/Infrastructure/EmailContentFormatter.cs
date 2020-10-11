using System;

namespace HappyTravel.Edo.Api.Infrastructure
{
    public class EmailContentFormatter
    {
        // TODO: Move to MailSender package NIJO-973
        public static string FromDate(DateTime? date)
        {
            return date.HasValue
                ? date.Value.ToString("dd-MMM-yy")
                : string.Empty;
        }
    }
}