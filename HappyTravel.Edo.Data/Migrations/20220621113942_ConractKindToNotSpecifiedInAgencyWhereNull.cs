using System;
using System.Linq;
using HappyTravel.Edo.Data.Agents;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HappyTravel.Edo.Data.Migrations
{
    public partial class ConractKindToNotSpecifiedInAgencyWhereNull : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var context = new EdoContextFactory().CreateDbContext(Array.Empty<string>());
            var agencies = context.Agencies.Where(a => a.ContractKind == null).ToList();
            agencies.ForEach(a =>
                {
                    a.ContractKind = ContractKind.NotSpecified;
                }
            );

            context.UpdateRange(agencies);
            context.SaveChanges();
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
