using System;
using HappyTravel.Edo.Notifications.Enums;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace HappyTravel.Edo.Data.Migrations
{
    public partial class SetEnabledProtocolsForPaymentRefunds : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData("DefaultNotificationOptions", "Type", (int) NotificationTypes.PaymentRefund, "EnabledProtocols",
                (int) (ProtocolTypes.Email | ProtocolTypes.WebSocket));
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
