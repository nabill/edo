namespace HappyTravel.Edo.Data.Agents
{
    public class BalanceNotificationSetting
    {
        public int Id { get; set; }
        public int AgencyAccountId { get; set; }
        public int[] Thresholds { get; set; }
    }
}
