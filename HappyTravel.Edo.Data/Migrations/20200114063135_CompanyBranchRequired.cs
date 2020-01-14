using Microsoft.EntityFrameworkCore.Migrations;

namespace HappyTravel.Edo.Data.Migrations
{
    public partial class CompanyBranchRequired : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var insertBranchesForCompanies =
                "insert into \"Branches\" (\"CompanyId\", \"Title\", \"Created\", \"Modified\") \n select \"Id\", 'Default', \"Created\", \"Updated\" from \"Companies\"";
            migrationBuilder.Sql(insertBranchesForCompanies);

            var bindUsersToBranches =
                "update \"CustomerCompanyRelations\" as cr \n set \"BranchId\" = (select \"Branches\".\"Id\" from \"Branches\" join \"Companies\" on \"Companies\".\"Id\" = \"Branches\".\"CompanyId\" where \"CompanyId\" = cr.\"CompanyId\")";
            migrationBuilder.Sql(bindUsersToBranches);
            
            migrationBuilder.AlterColumn<int>(
                name: "BranchId",
                table: "CustomerCompanyRelations",
                nullable: false,
                oldClrType: typeof(int),
                oldNullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDefault",
                table: "Branches",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "ReferenceCode",
                table: "AccountBalanceAuditLogs",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsDefault",
                table: "Branches");

            migrationBuilder.DropColumn(
                name: "ReferenceCode",
                table: "AccountBalanceAuditLogs");

            migrationBuilder.AlterColumn<int>(
                name: "BranchId",
                table: "CustomerCompanyRelations",
                nullable: true,
                oldClrType: typeof(int));
            
            var deleteAllBranchesSql = "delete from \"Branches\"";
            migrationBuilder.Sql(deleteAllBranchesSql);

            var clearAllCustomerBranchBindings = "update \"CustomerCompanyRelations\" \n set \"BranchId\" = null;";
            migrationBuilder.Sql(clearAllCustomerBranchBindings);
        }
    }
}
