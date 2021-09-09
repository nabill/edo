namespace HappyTravel.Edo.Data.Agents
{
    public class BalanceNotificationSetting
    {
        public int Id { get; set; }
        public int AccountId { get; set; }
        public int[] Thresholds { get; set; }
    }
}
