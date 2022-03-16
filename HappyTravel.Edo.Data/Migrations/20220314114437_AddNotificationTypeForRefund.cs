using System;
using HappyTravel.Edo.Notifications.Enums;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace HappyTravel.Edo.Data.Migrations
{
    public partial class AddNotificationTypeForRefund : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData("DefaultNotificationOptions", new string[] { "Type", "EnabledProtocols", "IsMandatory", "EnabledReceivers", "AgentEmailTemplateId"},
                new object[,]
                {
                    { (int)NotificationTypes.PaymentRefund, (int)(ProtocolTypes.Email), true, (int)ReceiverTypes.None, "d-d1ff61c075d546d987806df68e975006" }
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
