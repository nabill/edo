using System;
using System.Collections.Generic;
using HappyTravel.Edo.Data.Agents;
using HappyTravel.Edo.Data.Bookings;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HappyTravel.Edo.Data.Migrations
{
    public partial class FixedNullAccepted : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Url",
                table: "UploadedImages",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "FileName",
                table: "UploadedImages",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Rooms",
                table: "Bookings",
                type: "jsonb",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "jsonb",
                oldNullable: true);

            migrationBuilder.AlterColumn<AccommodationLocation>(
                name: "Location",
                table: "Bookings",
                type: "jsonb",
                nullable: false,
                oldClrType: typeof(AccommodationLocation),
                oldType: "jsonb",
                oldNullable: true);

            migrationBuilder.AlterColumn<List<CancellationPolicy>>(
                name: "CancellationPolicies",
                table: "Bookings",
                type: "jsonb",
                nullable: false,
                defaultValueSql: "'[]'::jsonb",
                oldClrType: typeof(List<CancellationPolicy>),
                oldType: "jsonb",
                oldNullable: true,
                oldDefaultValueSql: "'[]'::jsonb");

            migrationBuilder.AlterColumn<string>(
                name: "ReferenceCode",
                table: "AppliedBookingMarkups",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<AgentAccommodationBookingSettings>(
                name: "AccommodationBookingSettings",
                table: "AgentSystemSettings",
                type: "jsonb",
                nullable: false,
                oldClrType: typeof(AgentAccommodationBookingSettings),
                oldType: "jsonb",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "AgentRoles",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<int[]>(
                name: "AgentRoleIds",
                table: "AgentAgencyRelations",
                type: "integer[]",
                nullable: false,
                defaultValue: new int[0],
                oldClrType: typeof(int[]),
                oldType: "integer[]",
                oldNullable: true);

            migrationBuilder.AlterColumn<List<int>>(
                name: "Ancestors",
                table: "Agencies",
                type: "integer[]",
                nullable: false,
                oldClrType: typeof(List<int>),
                oldType: "integer[]",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "AdministratorRoles",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Url",
                table: "UploadedImages",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "FileName",
                table: "UploadedImages",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "Rooms",
                table: "Bookings",
                type: "jsonb",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "jsonb");

            migrationBuilder.AlterColumn<AccommodationLocation>(
                name: "Location",
                table: "Bookings",
                type: "jsonb",
                nullable: true,
                oldClrType: typeof(AccommodationLocation),
                oldType: "jsonb");

            migrationBuilder.AlterColumn<List<CancellationPolicy>>(
                name: "CancellationPolicies",
                table: "Bookings",
                type: "jsonb",
                nullable: true,
                defaultValueSql: "'[]'::jsonb",
                oldClrType: typeof(List<CancellationPolicy>),
                oldType: "jsonb",
                oldDefaultValueSql: "'[]'::jsonb");

            migrationBuilder.AlterColumn<string>(
                name: "ReferenceCode",
                table: "AppliedBookingMarkups",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<AgentAccommodationBookingSettings>(
                name: "AccommodationBookingSettings",
                table: "AgentSystemSettings",
                type: "jsonb",
                nullable: true,
                oldClrType: typeof(AgentAccommodationBookingSettings),
                oldType: "jsonb");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "AgentRoles",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<int[]>(
                name: "AgentRoleIds",
                table: "AgentAgencyRelations",
                type: "integer[]",
                nullable: true,
                oldClrType: typeof(int[]),
                oldType: "integer[]");

            migrationBuilder.AlterColumn<List<int>>(
                name: "Ancestors",
                table: "Agencies",
                type: "integer[]",
                nullable: true,
                oldClrType: typeof(List<int>),
                oldType: "integer[]");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "AdministratorRoles",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");
        }
    }
}
