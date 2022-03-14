using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace HappyTravel.Edo.Data.Migrations
{
    public partial class AddNotificationTypeForRefund : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                "DefaultNotificationOptions", 
                "Type", 
                35, 
                "AgentEmailTemplateId", 
                "d-d1ff61c075d546d987806df68e975006"); // Payment refund 
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
