namespace HappyTravel.Edo.Api.Models.Agencies
{
    public readonly struct BalanceNotificationSettingInfo
    {
        public int AccountId { get; init; }
        public int[] Thresholds { get; init; }
    }
}
