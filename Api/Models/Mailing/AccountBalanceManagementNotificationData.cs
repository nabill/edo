namespace HappyTravel.Edo.Api.Models.Mailing
{
    public class AccountBalanceManagementNotificationData : DataWithCompanyInfo
    {
        public string AgencyName { get; set; }
        public int AgencyId { get; set; }
        public int Threshold { get; set; }
        public int AgencyAccountId { get; set; }
        public string Currency { get; set; }
        public string NewAmount { get; set; }
    }
}
