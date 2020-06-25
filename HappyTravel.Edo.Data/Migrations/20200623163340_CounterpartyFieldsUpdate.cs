using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace HappyTravel.Edo.Data.Migrations
{
    public partial class CounterpartyFieldsUpdate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ContractNumber",
                table: "Counterparties");


            migrationBuilder.AddColumn<string>(
                name: "BillingEmail",
                table: "Counterparties",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BillingEmail",
                table: "Counterparties");

            migrationBuilder.AddColumn<string>(
                name: "ContractNumber",
                table: "Counterparties",
                type: "text",
                nullable: true);
        }
    }
}
