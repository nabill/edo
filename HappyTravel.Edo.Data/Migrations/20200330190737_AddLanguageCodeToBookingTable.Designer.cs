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
    [Migration("20200330190737_AddLanguageCodeToBookingTable")]
    partial class AddLanguageCodeToBookingTable
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("Npgsql:PostgresExtension:postgis", ",,")
                .HasAnnotation("Npgsql:PostgresExtension:uuid-ossp", ",,")
                .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn)
                .HasAnnotation("ProductVersion", "3.1.2")
                .HasAnnotation("Relational:MaxIdentifierLength", 63)
                .HasAnnotation("Relational:Sequence:.itn_seq", "'itn_seq', '', '1', '1', '', '', 'Int64', 'False'");

            modelBuilder.Entity("HappyTravel.Edo.Data.Booking.Booking", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

                    b.Property<string>("AgentReference")
                        .HasColumnType("text");

                    b.Property<DateTime>("BookingDate")
                        .HasColumnType("timestamp without time zone");

                    b.Property<string>("BookingDetails")
                        .HasColumnType("jsonb");

                    b.Property<string>("BookingRequest")
                        .IsRequired()
                        .HasColumnType("jsonb");

                    b.Property<int>("CompanyId")
                        .HasColumnType("integer");

                    b.Property<DateTime>("Created")
                        .HasColumnType("timestamp without time zone");

                    b.Property<int>("CustomerId")
                        .HasColumnType("integer");

                    b.Property<int>("DataProvider")
                        .HasColumnType("integer");

                    b.Property<string>("ItineraryNumber")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("LanguageCode")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("MainPassengerName")
                        .HasColumnType("text");

                    b.Property<string>("Nationality")
                        .HasColumnType("text");

                    b.Property<int>("PaymentMethod")
                        .HasColumnType("integer");

                    b.Property<int>("PaymentStatus")
                        .HasColumnType("integer");

                    b.Property<string>("ReferenceCode")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("Residency")
                        .HasColumnType("text");

                    b.Property<string>("ServiceDetails")
                        .IsRequired()
                        .HasColumnType("jsonb");

                    b.Property<int>("ServiceType")
                        .HasColumnType("integer");

                    b.Property<int>("Status")
                        .HasColumnType("integer");

                    b.HasKey("Id");

                    b.HasIndex("CompanyId");

                    b.HasIndex("CustomerId");

                    b.HasIndex("ItineraryNumber");

                    b.HasIndex("MainPassengerName");

                    b.HasIndex("ReferenceCode");

                    b.HasIndex("ServiceType");

                    b.ToTable("Bookings");
                });

            modelBuilder.Entity("HappyTravel.Edo.Data.Booking.BookingAuditLogEntry", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

                    b.Property<string>("BookingDetails")
                        .IsRequired()
                        .HasColumnType("jsonb");

                    b.Property<int>("BookingId")
                        .HasColumnType("integer");

                    b.Property<DateTime>("CreatedAt")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("timestamp without time zone")
                        .HasDefaultValueSql("NOW()");

                    b.Property<int>("CustomerId")
                        .HasColumnType("integer");

                    b.Property<string>("PreviousBookingDetails")
                        .IsRequired()
                        .HasColumnType("jsonb");

                    b.HasKey("Id");

                    b.HasIndex("BookingId");

                    b.HasIndex("CustomerId");

                    b.ToTable("BookingAuditLog");
                });

            modelBuilder.Entity("HappyTravel.Edo.Data.Customers.Branch", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

                    b.Property<int>("CompanyId")
                        .HasColumnType("integer");

                    b.Property<DateTime>("Created")
                        .HasColumnType("timestamp without time zone");

                    b.Property<bool>("IsDefault")
                        .HasColumnType("boolean");

                    b.Property<DateTime>("Modified")
                        .HasColumnType("timestamp without time zone");

                    b.Property<string>("Title")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.HasIndex("CompanyId");

                    b.ToTable("Branches");

                    b.HasData(
                        new
                        {
                            Id = -1,
                            CompanyId = -1,
                            Created = new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                            IsDefault = false,
                            Modified = new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                            Title = "Test branch"
                        });
                });

            modelBuilder.Entity("HappyTravel.Edo.Data.Customers.Company", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

                    b.Property<string>("Address")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("City")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("CountryCode")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<DateTime>("Created")
                        .HasColumnType("timestamp without time zone");

                    b.Property<string>("Fax")
                        .HasColumnType("text");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("Phone")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("PostalCode")
                        .HasColumnType("text");

                    b.Property<int>("PreferredCurrency")
                        .HasColumnType("integer");

                    b.Property<int>("PreferredPaymentMethod")
                        .HasColumnType("integer");

                    b.Property<int>("State")
                        .HasColumnType("integer");

                    b.Property<DateTime>("Updated")
                        .HasColumnType("timestamp without time zone");

                    b.Property<string>("VerificationReason")
                        .HasColumnType("text");

                    b.Property<DateTime?>("Verified")
                        .HasColumnType("timestamp without time zone");

                    b.Property<string>("Website")
                        .HasColumnType("text");

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
                            PreferredPaymentMethod = 2,
                            State = 0,
                            Updated = new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                            Website = "https://happytravel.com"
                        });
                });

            modelBuilder.Entity("HappyTravel.Edo.Data.Customers.Customer", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

                    b.Property<string>("AppSettings")
                        .HasColumnType("jsonb");

                    b.Property<DateTime>("Created")
                        .HasColumnType("timestamp without time zone");

                    b.Property<string>("Email")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("FirstName")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("IdentityHash")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("LastName")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("Position")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("Title")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("UserSettings")
                        .HasColumnType("jsonb");

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
                    b.Property<int>("CustomerId")
                        .HasColumnType("integer");

                    b.Property<int>("CompanyId")
                        .HasColumnType("integer");

                    b.Property<int>("Type")
                        .HasColumnType("integer");

                    b.Property<int>("BranchId")
                        .HasColumnType("integer");

                    b.Property<int>("InCompanyPermissions")
                        .HasColumnType("integer");

                    b.HasKey("CustomerId", "CompanyId", "Type");

                    b.ToTable("CustomerCompanyRelations");

                    b.HasData(
                        new
                        {
                            CustomerId = -1,
                            CompanyId = -1,
                            Type = 1,
                            BranchId = -1,
                            InCompanyPermissions = 0
                        });
                });

            modelBuilder.Entity("HappyTravel.Edo.Data.Customers.UserInvitation", b =>
                {
                    b.Property<string>("CodeHash")
                        .HasColumnType("text");

                    b.Property<DateTime>("Created")
                        .HasColumnType("timestamp without time zone");

                    b.Property<string>("Data")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("Email")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<int>("InvitationType")
                        .HasColumnType("integer");

                    b.Property<bool>("IsAccepted")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("boolean")
                        .HasDefaultValue(false);

                    b.HasKey("CodeHash");

                    b.ToTable("UserInvitations");
                });

            modelBuilder.Entity("HappyTravel.Edo.Data.Infrastructure.EntityLock", b =>
                {
                    b.Property<string>("EntityDescriptor")
                        .HasColumnType("text");

                    b.Property<string>("LockerInfo")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("Token")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("EntityDescriptor");

                    b.ToTable("EntityLock");
                });

            modelBuilder.Entity("HappyTravel.Edo.Data.Locations.Country", b =>
                {
                    b.Property<string>("Code")
                        .HasColumnType("text");

                    b.Property<string>("Names")
                        .IsRequired()
                        .HasColumnType("jsonb");

                    b.Property<int>("RegionId")
                        .HasColumnType("integer");

                    b.HasKey("Code");

                    b.ToTable("Countries");
                });

            modelBuilder.Entity("HappyTravel.Edo.Data.Locations.Location", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid")
                        .HasDefaultValueSql("uuid_generate_v4()");

                    b.Property<Point>("Coordinates")
                        .IsRequired()
                        .HasColumnType("geography (point)");

                    b.Property<string>("Country")
                        .IsRequired()
                        .HasColumnType("jsonb");

                    b.Property<string>("DataProviders")
                        .IsRequired()
                        .ValueGeneratedOnAdd()
                        .HasColumnType("jsonb")
                        .HasDefaultValue("[]");

                    b.Property<string>("DefaultCountry")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("DefaultLocality")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("DefaultName")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<int>("DistanceInMeters")
                        .HasColumnType("integer");

                    b.Property<string>("Locality")
                        .IsRequired()
                        .HasColumnType("jsonb");

                    b.Property<DateTime>("Modified")
                        .HasColumnType("timestamp without time zone");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("jsonb");

                    b.Property<int>("Source")
                        .HasColumnType("integer");

                    b.Property<int>("Type")
                        .HasColumnType("integer");

                    b.HasKey("Id");

                    b.ToTable("Locations");
                });

            modelBuilder.Entity("HappyTravel.Edo.Data.Locations.Region", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

                    b.Property<string>("Names")
                        .IsRequired()
                        .HasColumnType("jsonb");

                    b.HasKey("Id");

                    b.ToTable("Regions");
                });

            modelBuilder.Entity("HappyTravel.Edo.Data.Management.Administrator", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

                    b.Property<DateTime>("Created")
                        .HasColumnType("timestamp without time zone");

                    b.Property<string>("Email")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("FirstName")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("IdentityHash")
                        .HasColumnType("text");

                    b.Property<string>("LastName")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("Position")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<DateTime>("Updated")
                        .HasColumnType("timestamp without time zone");

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
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

                    b.Property<int>("AdministratorId")
                        .HasColumnType("integer");

                    b.Property<DateTime>("Created")
                        .HasColumnType("timestamp without time zone");

                    b.Property<string>("EventData")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<int>("Type")
                        .HasColumnType("integer");

                    b.HasKey("Id");

                    b.ToTable("ManagementAuditLog");
                });

            modelBuilder.Entity("HappyTravel.Edo.Data.Management.ServiceAccount", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

                    b.Property<string>("ClientId")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.ToTable("ServiceAccounts");
                });

            modelBuilder.Entity("HappyTravel.Edo.Data.Markup.AppliedMarkup", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

                    b.Property<DateTime>("Created")
                        .HasColumnType("timestamp without time zone");

                    b.Property<string>("Policies")
                        .IsRequired()
                        .HasColumnType("jsonb");

                    b.Property<string>("ReferenceCode")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<int>("ServiceType")
                        .HasColumnType("integer");

                    b.HasKey("Id");

                    b.HasIndex("ReferenceCode");

                    b.HasIndex("ServiceType");

                    b.ToTable("MarkupLog");
                });

            modelBuilder.Entity("HappyTravel.Edo.Data.Markup.MarkupPolicy", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

                    b.Property<int?>("BranchId")
                        .HasColumnType("integer");

                    b.Property<int?>("CompanyId")
                        .HasColumnType("integer");

                    b.Property<DateTime>("Created")
                        .HasColumnType("timestamp without time zone");

                    b.Property<int>("Currency")
                        .HasColumnType("integer");

                    b.Property<int?>("CustomerId")
                        .HasColumnType("integer");

                    b.Property<string>("Description")
                        .HasColumnType("text");

                    b.Property<DateTime>("Modified")
                        .HasColumnType("timestamp without time zone");

                    b.Property<int>("Order")
                        .HasColumnType("integer");

                    b.Property<int>("ScopeType")
                        .HasColumnType("integer");

                    b.Property<int>("Target")
                        .HasColumnType("integer");

                    b.Property<int>("TemplateId")
                        .HasColumnType("integer");

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
                        .HasColumnType("text");

                    b.Property<int>("CurrentNumber")
                        .HasColumnType("integer");

                    b.HasKey("ItineraryNumber");

                    b.ToTable("ItnNumerator");
                });

            modelBuilder.Entity("HappyTravel.Edo.Data.PaymentLinks.PaymentLink", b =>
                {
                    b.Property<string>("Code")
                        .HasColumnType("text");

                    b.Property<decimal>("Amount")
                        .HasColumnType("numeric");

                    b.Property<string>("Comment")
                        .HasColumnType("text");

                    b.Property<DateTime>("Created")
                        .HasColumnType("timestamp without time zone");

                    b.Property<int>("Currency")
                        .HasColumnType("integer");

                    b.Property<string>("Email")
                        .HasColumnType("text");

                    b.Property<DateTime?>("LastPaymentDate")
                        .HasColumnType("timestamp without time zone");

                    b.Property<string>("LastPaymentResponse")
                        .HasColumnType("jsonb");

                    b.Property<string>("ReferenceCode")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<int>("ServiceType")
                        .HasColumnType("integer");

                    b.HasKey("Code");

                    b.HasIndex("ReferenceCode");

                    b.ToTable("PaymentLinks");
                });

            modelBuilder.Entity("HappyTravel.Edo.Data.Payments.AccountBalanceAuditLogEntry", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

                    b.Property<int>("AccountId")
                        .HasColumnType("integer");

                    b.Property<decimal>("Amount")
                        .HasColumnType("numeric");

                    b.Property<DateTime>("Created")
                        .HasColumnType("timestamp without time zone");

                    b.Property<string>("EventData")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("ReferenceCode")
                        .HasColumnType("text");

                    b.Property<int>("Type")
                        .HasColumnType("integer");

                    b.Property<int>("UserId")
                        .HasColumnType("integer");

                    b.Property<int>("UserType")
                        .HasColumnType("integer");

                    b.HasKey("Id");

                    b.ToTable("AccountBalanceAuditLogs");
                });

            modelBuilder.Entity("HappyTravel.Edo.Data.Payments.CreditCard", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

                    b.Property<string>("ExpirationDate")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("HolderName")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("MaskedNumber")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<int>("OwnerId")
                        .HasColumnType("integer");

                    b.Property<int>("OwnerType")
                        .HasColumnType("integer");

                    b.Property<string>("Token")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.ToTable("CreditCards");
                });

            modelBuilder.Entity("HappyTravel.Edo.Data.Payments.CreditCardAuditLogEntry", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

                    b.Property<decimal>("Amount")
                        .HasColumnType("numeric");

                    b.Property<DateTime>("Created")
                        .HasColumnType("timestamp without time zone");

                    b.Property<int>("Currency")
                        .HasColumnType("integer");

                    b.Property<int>("CustomerId")
                        .HasColumnType("integer");

                    b.Property<string>("EventData")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("MaskedNumber")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("ReferenceCode")
                        .HasColumnType("text");

                    b.Property<int>("Type")
                        .HasColumnType("integer");

                    b.Property<int>("UserId")
                        .HasColumnType("integer");

                    b.Property<int>("UserType")
                        .HasColumnType("integer");

                    b.HasKey("Id");

                    b.ToTable("CreditCardAuditLogs");
                });

            modelBuilder.Entity("HappyTravel.Edo.Data.Payments.Payment", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

                    b.Property<int?>("AccountId")
                        .HasColumnType("integer");

                    b.Property<string>("AccountNumber")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<decimal>("Amount")
                        .HasColumnType("numeric");

                    b.Property<int>("BookingId")
                        .HasColumnType("integer");

                    b.Property<DateTime>("Created")
                        .HasColumnType("timestamp without time zone");

                    b.Property<string>("Currency")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("Data")
                        .IsRequired()
                        .HasColumnType("jsonb");

                    b.Property<DateTime>("Modified")
                        .HasColumnType("timestamp without time zone");

                    b.Property<int>("PaymentMethod")
                        .HasColumnType("integer");

                    b.Property<int>("Status")
                        .HasColumnType("integer");

                    b.HasKey("Id");

                    b.HasIndex("BookingId");

                    b.ToTable("Payments");
                });

            modelBuilder.Entity("HappyTravel.Edo.Data.Payments.PaymentAccount", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

                    b.Property<decimal>("AuthorizedBalance")
                        .HasColumnType("numeric");

                    b.Property<decimal>("Balance")
                        .HasColumnType("numeric");

                    b.Property<int>("CompanyId")
                        .HasColumnType("integer");

                    b.Property<DateTime>("Created")
                        .HasColumnType("timestamp without time zone");

                    b.Property<decimal>("CreditLimit")
                        .HasColumnType("numeric");

                    b.Property<int>("Currency")
                        .HasColumnType("integer");

                    b.HasKey("Id");

                    b.ToTable("PaymentAccounts");
                });

            modelBuilder.Entity("HappyTravel.Edo.Data.Suppliers.SupplierOrder", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

                    b.Property<DateTime>("Created")
                        .HasColumnType("timestamp without time zone");

                    b.Property<int>("DataProvider")
                        .HasColumnType("integer");

                    b.Property<DateTime>("Modified")
                        .HasColumnType("timestamp without time zone");

                    b.Property<decimal>("Price")
                        .HasColumnType("numeric");

                    b.Property<string>("ReferenceCode")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<int>("State")
                        .HasColumnType("integer");

                    b.Property<int>("Type")
                        .HasColumnType("integer");

                    b.HasKey("Id");

                    b.HasIndex("DataProvider");

                    b.HasIndex("ReferenceCode");

                    b.HasIndex("Type");

                    b.ToTable("SupplierOrders");
                });

            modelBuilder.Entity("HappyTravel.Edo.Data.Booking.BookingAuditLogEntry", b =>
                {
                    b.HasOne("HappyTravel.Edo.Data.Booking.Booking", null)
                        .WithMany()
                        .HasForeignKey("BookingId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("HappyTravel.Edo.Data.Customers.Customer", null)
                        .WithMany()
                        .HasForeignKey("CustomerId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });
#pragma warning restore 612, 618
        }
    }
}