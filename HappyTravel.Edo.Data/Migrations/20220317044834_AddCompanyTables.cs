using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace HappyTravel.Edo.Data.Migrations
{
    public partial class AddCompanyTables : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CompanyBanks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Address = table.Column<string>(type: "text", nullable: false),
                    RoutingCode = table.Column<string>(type: "text", nullable: false),
                    SwiftCode = table.Column<string>(type: "text", nullable: false),
                    Created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    Modified = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CompanyBanks", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CompanyAccounts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CompanyBankId = table.Column<int>(type: "integer", nullable: false),
                    Currency = table.Column<int>(type: "integer", nullable: false),
                    AccountNumber = table.Column<string>(type: "text", nullable: false),
                    Iban = table.Column<string>(type: "text", nullable: false),
                    IsDefault = table.Column<bool>(type: "boolean", nullable: false),
                    IntermediaryBankName = table.Column<string>(type: "text", nullable: true),
                    IntermediaryBankAccountNumber = table.Column<string>(type: "text", nullable: true),
                    IntermediaryBankSwiftCode = table.Column<string>(type: "text", nullable: true),
                    IntermediaryBankAbaNo = table.Column<string>(type: "text", nullable: true),
                    Created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    Modified = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CompanyAccounts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CompanyAccounts_CompanyBanks_CompanyBankId",
                        column: x => x.CompanyBankId,
                        principalTable: "CompanyBanks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CompanyAccounts_CompanyBankId",
                table: "CompanyAccounts",
                column: "CompanyBankId");

            var utcNow = DateTimeOffset.UtcNow;
            migrationBuilder.InsertData("CompanyBanks", new string[] { "Id", "Name", "Address", "RoutingCode", "SwiftCode", "Created", "Modified" }, new object[,]
                {
                    { 1, "Emirates NBD Bank", "Muraqqabat branch, Dubai, UAE", "302620122", "EBILAEAD", utcNow, utcNow },
                    { 2, "Abu Dhabi Commercial Bank", "Business Bay branch, Dubai, UAE", "600310101", "ADCBAEAA", utcNow, utcNow }
                });

            migrationBuilder.InsertData("CompanyAccounts", new string[] { "Id", "CompanyBankId", "Currency", "AccountNumber", "Iban", "IsDefault",
                "IntermediaryBankName", "IntermediaryBankAccountNumber", "IntermediaryBankSwiftCode", "IntermediaryBankAbaNo", "Created", "Modified" }, new object[,]
                {
                    { 1, 1, "AED", "1015674054701", "AE580260001015674054701", "true",
                        null, null, null, null, utcNow, utcNow },
                    { 2, 1, "EUR", "1025674054703", "AE390260001025674054703", "true",
                        null, null, null, null, utcNow, utcNow },
                    { 3, 1, "USD", "1025674054702", "AE660260001025674054702", "false",
                "Bank of America National Association, New York", "6550286074", "BOFAUS3N", "026009593", utcNow, utcNow },
                    { 4, 2, "USD", "11835739920001", "AE710030011835739920001", "true",
                        null, null, null, null, utcNow, utcNow },
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CompanyAccounts");

            migrationBuilder.DropTable(
                name: "CompanyBanks");
        }
    }
}
