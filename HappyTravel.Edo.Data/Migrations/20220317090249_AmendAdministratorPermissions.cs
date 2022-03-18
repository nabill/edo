using System;
using HappyTravel.Edo.Common.Enums.Administrators;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace HappyTravel.Edo.Data.Migrations
{
    public partial class AmendAdministratorPermissions : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Delete BookingAuditor role
            migrationBuilder.DeleteData("AdministratorRoles", "Name", "Booking auditor");
            
            // Update AccountManager role
            migrationBuilder.UpdateData("AdministratorRoles", "Name", "Account manager", "Permissions",
                AdministratorPermissions.ViewAgencies
                | AdministratorPermissions.ViewAgents
                | AdministratorPermissions.AgencyVerification
                | AdministratorPermissions.AgencyBalanceObservation
                | AdministratorPermissions.AgencyManagement
                | AdministratorPermissions.MarketingReportGeneration
                | AdministratorPermissions.BookingReportGeneration);
            
            // Update FinanceManager role
            migrationBuilder.UpdateData("AdministratorRoles", "Name", "Finance manager", "Permissions",
                AdministratorPermissions.AccountReplenish
                | AdministratorPermissions.OfflinePayment
                | AdministratorPermissions.ViewAgencies
                | AdministratorPermissions.ViewAgents
                | AdministratorPermissions.AgencyBalanceObservation
                | AdministratorPermissions.AgencyBalanceReplenishAndSubtract
                | AdministratorPermissions.PaymentLinkGeneration
                | AdministratorPermissions.BalanceManualCorrection
                | AdministratorPermissions.BookingReportGeneration
                | AdministratorPermissions.FinanceReportGeneration
                | AdministratorPermissions.MarketingReportGeneration
                | AdministratorPermissions.ManageBookingByReferenceCode);
            
            // Update PaymentLinkManager
            migrationBuilder.UpdateData("AdministratorRoles", "Name", "Payment link manager", "Permissions",
                AdministratorPermissions.PaymentLinkGeneration);
            
            // Update BookingManager
            migrationBuilder.UpdateData("AdministratorRoles", "Name", "Booking manager", "Permissions",
                AdministratorPermissions.BookingManagement
                | AdministratorPermissions.BookingReportGeneration);
            
            // Update Accommodation Mapping Manager
            migrationBuilder.UpdateData("AdministratorRoles", "Name", "Accommodation Mapping Manager", "Permissions",
                AdministratorPermissions.AccommodationsMerge
                | AdministratorPermissions.AccommodationsManagement
                | AdministratorPermissions.LocationsManagement);
            
            // Add super-admin role
            migrationBuilder.InsertData("AdministratorRoles", new string[] { "Name", "Permissions" },
                new object[,]
                {
                    { (string) "Super-admin", 
                        (AdministratorPermissions) (AdministratorPermissions.MarkupManagement
                        | AdministratorPermissions.BookingManagement
                        | AdministratorPermissions.BookingReportGeneration
                        | AdministratorPermissions.FinanceReportGeneration
                        | AdministratorPermissions.MarketingReportGeneration
                        | AdministratorPermissions.AdministratorInvitation
                        | AdministratorPermissions.AgencyVerification
                        | AdministratorPermissions.AgencyBalanceObservation
                        | AdministratorPermissions.AgencyManagement
                        | AdministratorPermissions.AgentManagement
                        | AdministratorPermissions.AdministratorManagement
                        | AdministratorPermissions.ViewAgencies
                        | AdministratorPermissions.ViewAgents
                        | AdministratorPermissions.PaymentLinkGeneration
                        | AdministratorPermissions.AccountReplenish
                        | AdministratorPermissions.OfflinePayment
                        | AdministratorPermissions.AgencyBalanceReplenishAndSubtract
                        | AdministratorPermissions.BalanceManualCorrection) 
                    }
                });
            
            // Add reports manager role
            migrationBuilder.InsertData("AdministratorRoles", new string[] { "Name", "Permissions" },
                new object[,]
                {
                    { (string) "Reports manager", 
                        (AdministratorPermissions) (AdministratorPermissions.BookingReportGeneration
                        | AdministratorPermissions.FinanceReportGeneration
                        | AdministratorPermissions.MarketingReportGeneration
                        | AdministratorPermissions.GeneratePaymentLinkReport) 
                    }
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
        }
    }
}
