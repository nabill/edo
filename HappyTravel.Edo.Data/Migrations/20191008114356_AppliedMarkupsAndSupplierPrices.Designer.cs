﻿// <auto-generated />
using System;
using HappyTravel.Edo.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using NetTopologySuite.Geometries;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace HappyTravel.Edo.Data.Migrations
{
    [DbContext(typeof(EdoContext))]
    [Migration("20191008114356_AppliedMarkupsAndSupplierPrices")]
    partial class AppliedMarkupsAndSupplierPrices
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("Npgsql:PostgresExtension:postgis", ",,")
                .HasAnnotation("Npgsql:PostgresExtension:uuid-ossp", ",,")
                .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn)
                .HasAnnotation("ProductVersion", "2.2.4-servicing-10062")
                .HasAnnotation("Relational:MaxIdentifierLength", 63)
                .HasAnnotation("Relational:Sequence:.itn_seq", "'itn_seq', '', '1', '1', '', '', 'Int64', 'False'");

            modelBuilder.Entity("HappyTravel.Edo.Data.Booking.Booking", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("AgentReference");

                    b.Property<DateTime>("BookingDate");

                    b.Property<string>("BookingDetails")
                        .HasColumnType("jsonb");

                    b.Property<int>("CompanyId");

                    b.Property<DateTime>("Created");

                    b.Property<int>("CustomerId");

                    b.Property<string>("ItineraryNumber")
                        .IsRequired();

                    b.Property<string>("MainPassengerName")
                        .IsRequired();

                    b.Property<string>("Nationality");

                    b.Property<int>("PaymentMethod");

                    b.Property<string>("ReferenceCode")
                        .IsRequired();

                    b.Property<string>("Residency");

                    b.Property<string>("ServiceDetails")
                        .HasColumnType("jsonb");

                    b.Property<int>("ServiceType");

                    b.Property<int>("Status");

                    b.HasKey("Id");

                    b.HasIndex("CompanyId");

                    b.HasIndex("CustomerId");

                    b.HasIndex("ItineraryNumber");

                    b.HasIndex("MainPassengerName");

                    b.HasIndex("ReferenceCode");

                    b.HasIndex("ServiceType");

                    b.ToTable("Bookings");
                });

            modelBuilder.Entity("HappyTravel.Edo.Data.CurrencyExchange.CurrencyRate", b =>
                {
                    b.Property<int>("SourceCurrency");

                    b.Property<int>("TargetCurrency");

                    b.Property<DateTime?>("ValidTo");

                    b.Property<decimal>("Rate");

                    b.Property<DateTime>("ValidFrom");

                    b.HasKey("SourceCurrency", "TargetCurrency", "ValidTo");

                    b.ToTable("CurrencyRates");
                });

            modelBuilder.Entity("HappyTravel.Edo.Data.Customers.Branch", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<int>("CompanyId");

                    b.Property<DateTime>("Created");

                    b.Property<DateTime>("Modified");

                    b.Property<string>("Title")
                        .IsRequired();

                    b.HasKey("Id");

                    b.HasIndex("CompanyId");

                    b.ToTable("Branches");

                    b.HasData(
                        new
                        {
                            Id = -1,
                            CompanyId = -1,
                            Created = new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                            Modified = new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                            Title = "Test branch"
                        });
                });

            modelBuilder.Entity("HappyTravel.Edo.Data.Customers.Company", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Address")
                        .IsRequired();

                    b.Property<string>("City")
                        .IsRequired();

                    b.Property<string>("CountryCode")
                        .IsRequired();

                    b.Property<DateTime>("Created");

                    b.Property<string>("Fax");

                    b.Property<string>("Name")
                        .IsRequired();

                    b.Property<string>("Phone")
                        .IsRequired();

                    b.Property<string>("PostalCode");

                    b.Property<int>("PreferredCurrency");

                    b.Property<int>("PreferredPaymentMethod");

                    b.Property<int>("State");

                    b.Property<DateTime>("Updated");

                    b.Property<string>("VerificationReason");

                    b.Property<DateTime?>("Verified");

                    b.Property<string>("Website");

                    b.HasKey("Id");

                    b.ToTable("Companies");

                    b.HasData(
                        new
                        {
                            Id = -1,
                            Address = "Address",
                            City = "City",
                            CountryCode = "IT",
                            Created = new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                            Fax = "Fax",
                            Name = "Test company",
                            Phone = "Phone",
                            PostalCode = "400055",
                            PreferredCurrency = 1,
                            PreferredPaymentMethod = 1,
                            State = 0,
                            Updated = new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                            Website = "https://happytravel.com"
                        });
                });

            modelBuilder.Entity("HappyTravel.Edo.Data.Customers.Customer", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<DateTime>("Created");

                    b.Property<string>("Email")
                        .IsRequired();

                    b.Property<string>("FirstName")
                        .IsRequired();

                    b.Property<string>("IdentityHash")
                        .IsRequired();

                    b.Property<string>("LastName")
                        .IsRequired();

                    b.Property<string>("Position")
                        .IsRequired();

                    b.Property<string>("Title")
                        .IsRequired();

                    b.HasKey("Id");

                    b.ToTable("Customers");

                    b.HasData(
                        new
                        {
                            Id = -1,
                            Created = new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                            Email = "test@happytravel.com",
                            FirstName = "FirstName",
                            IdentityHash = "postman",
                            LastName = "LastName",
                            Position = "Position",
                            Title = "Mr."
                        });
                });

            modelBuilder.Entity("HappyTravel.Edo.Data.Customers.CustomerCompanyRelation", b =>
                {
                    b.Property<int>("CustomerId");

                    b.Property<int>("CompanyId");

                    b.Property<int>("Type");

                    b.Property<int?>("BranchId");

                    b.HasKey("CustomerId", "CompanyId", "Type");

                    b.ToTable("CustomerCompanyRelations");

                    b.HasData(
                        new
                        {
                            CustomerId = -1,
                            CompanyId = -1,
                            Type = 1,
                            BranchId = -1
                        });
                });

            modelBuilder.Entity("HappyTravel.Edo.Data.Customers.UserInvitation", b =>
                {
                    b.Property<string>("CodeHash")
                        .ValueGeneratedOnAdd();

                    b.Property<DateTime>("Created");

                    b.Property<string>("Data")
                        .IsRequired();

                    b.Property<string>("Email")
                        .IsRequired();

                    b.Property<int>("InvitationType");

                    b.Property<bool>("IsAccepted")
                        .ValueGeneratedOnAdd()
                        .HasDefaultValue(false);

                    b.HasKey("CodeHash");

                    b.ToTable("UserInvitations");
                });

            modelBuilder.Entity("HappyTravel.Edo.Data.Infrastructure.EntityLock", b =>
                {
                    b.Property<string>("EntityDescriptor")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("LockerInfo")
                        .IsRequired();

                    b.Property<string>("Token")
                        .IsRequired();

                    b.HasKey("EntityDescriptor");

                    b.ToTable("EntityLock");
                });

            modelBuilder.Entity("HappyTravel.Edo.Data.Locations.Country", b =>
                {
                    b.Property<string>("Code")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Names")
                        .IsRequired()
                        .HasColumnType("jsonb");

                    b.Property<int>("RegionId");

                    b.HasKey("Code");

                    b.ToTable("Countries");
                });

            modelBuilder.Entity("HappyTravel.Edo.Data.Locations.Location", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasDefaultValueSql("uuid_generate_v4()");

                    b.Property<Point>("Coordinates")
                        .IsRequired()
                        .HasColumnType("geography (point)");

                    b.Property<string>("Country")
                        .IsRequired()
                        .HasColumnType("jsonb");

                    b.Property<int>("DistanceInMeters");

                    b.Property<string>("Locality")
                        .IsRequired()
                        .HasColumnType("jsonb");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("jsonb");

                    b.Property<int>("Source");

                    b.Property<int>("Type");

                    b.HasKey("Id");

                    b.ToTable("Locations");
                });

            modelBuilder.Entity("HappyTravel.Edo.Data.Locations.Region", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Names")
                        .IsRequired()
                        .HasColumnType("jsonb");

                    b.HasKey("Id");

                    b.ToTable("Regions");
                });

            modelBuilder.Entity("HappyTravel.Edo.Data.Management.Administrator", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<DateTime>("Created");

                    b.Property<string>("Email")
                        .IsRequired();

                    b.Property<string>("FirstName")
                        .IsRequired();

                    b.Property<string>("IdentityHash");

                    b.Property<string>("LastName")
                        .IsRequired();

                    b.Property<string>("Position")
                        .IsRequired();

                    b.Property<DateTime>("Updated");

                    b.HasKey("Id");

                    b.HasIndex("IdentityHash");

                    b.ToTable("Administrators");

                    b.HasData(
                        new
                        {
                            Id = -1,
                            Created = new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                            Email = "testAdmin@happytravel.com",
                            FirstName = "FirstName",
                            IdentityHash = "postman",
                            LastName = "LastName",
                            Position = "Position",
                            Updated = new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified)
                        });
                });

            modelBuilder.Entity("HappyTravel.Edo.Data.Management.ManagementAuditLogEntry", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<int>("AdministratorId");

                    b.Property<DateTime>("Created");

                    b.Property<string>("EventData")
                        .IsRequired();

                    b.Property<int>("Type");

                    b.HasKey("Id");

                    b.ToTable("ManagementAuditLog");
                });

            modelBuilder.Entity("HappyTravel.Edo.Data.Markup.AppliedMarkup", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<DateTime>("Created");

                    b.Property<string>("Policies")
                        .IsRequired()
                        .HasColumnType("jsonb");

                    b.Property<string>("ReferenceCode")
                        .IsRequired();

                    b.Property<int>("ServiceType");

                    b.HasKey("Id");

                    b.HasIndex("ReferenceCode");

                    b.HasIndex("ServiceType");

                    b.ToTable("MarkupLog");
                });

            modelBuilder.Entity("HappyTravel.Edo.Data.Markup.MarkupPolicy", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<int?>("BranchId");

                    b.Property<int?>("CompanyId");

                    b.Property<DateTime>("Created");

                    b.Property<int>("Currency");

                    b.Property<int?>("CustomerId");

                    b.Property<string>("Description");

                    b.Property<DateTime>("Modified");

                    b.Property<int>("Order");

                    b.Property<int>("ScopeType");

                    b.Property<int>("Target");

                    b.Property<int>("TemplateId");

                    b.Property<string>("TemplateSettings")
                        .IsRequired()
                        .HasColumnType("jsonb");

                    b.HasKey("Id");

                    b.HasIndex("BranchId");

                    b.HasIndex("CompanyId");

                    b.HasIndex("CustomerId");

                    b.HasIndex("ScopeType");

                    b.HasIndex("Target");

                    b.ToTable("MarkupPolicies");
                });

            modelBuilder.Entity("HappyTravel.Edo.Data.Numeration.ItnNumerator", b =>
                {
                    b.Property<string>("ItineraryNumber")
                        .ValueGeneratedOnAdd();

                    b.Property<int>("CurrentNumber");

                    b.HasKey("ItineraryNumber");

                    b.ToTable("ItnNumerator");
                });

            modelBuilder.Entity("HappyTravel.Edo.Data.Payments.AccountBalanceAuditLogEntry", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<int>("AccountId");

                    b.Property<decimal>("Amount");

                    b.Property<DateTime>("Created");

                    b.Property<string>("EventData")
                        .IsRequired();

                    b.Property<int>("Type");

                    b.Property<int>("UserId");

                    b.Property<int>("UserType");

                    b.HasKey("Id");

                    b.ToTable("AccountBalanceAuditLogs");
                });

            modelBuilder.Entity("HappyTravel.Edo.Data.Payments.PaymentAccount", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<decimal>("Balance");

                    b.Property<int>("CompanyId");

                    b.Property<DateTime>("Created");

                    b.Property<decimal>("CreditLimit");

                    b.Property<int>("Currency");

                    b.HasKey("Id");

                    b.ToTable("PaymentAccounts");
                });

            modelBuilder.Entity("HappyTravel.Edo.Data.Suppliers.SupplierOrder", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<DateTime>("Created");

                    b.Property<int>("DataProvider");

                    b.Property<DateTime>("Modified");

                    b.Property<decimal>("Price");

                    b.Property<string>("ReferenceCode")
                        .IsRequired();

                    b.Property<int>("State");

                    b.Property<int>("Type");

                    b.HasKey("Id");

                    b.HasIndex("DataProvider");

                    b.HasIndex("ReferenceCode");

                    b.HasIndex("Type");

                    b.ToTable("SupplierOrders");
                });
#pragma warning restore 612, 618
        }
    }
}
