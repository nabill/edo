namespace HappyTravel.Edo.Api.Models.Agencies
{
    public readonly struct BalanceNotificationSettingInfo
    {
        public int AgencyAccountId { get; init; }
        public int[] Thresholds { get; init; }
    }
}
