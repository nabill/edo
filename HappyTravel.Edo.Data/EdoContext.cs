﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;
using HappyTravel.Edo.Common.Enums;
using HappyTravel.Edo.Data.Agents;
using HappyTravel.Edo.Data.Bookings;
using HappyTravel.Edo.Data.Company;
using HappyTravel.Edo.Data.Documents;
using HappyTravel.Edo.Data.Infrastructure;
using HappyTravel.Edo.Data.Locations;
using HappyTravel.Edo.Data.Management;
using HappyTravel.Edo.Data.Markup;
using HappyTravel.Edo.Data.Notifications;
using HappyTravel.Edo.Data.Numeration;
using HappyTravel.Edo.Data.PaymentLinks;
using HappyTravel.Edo.Data.Payments;
using HappyTravel.Edo.Data.Suppliers;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using BookingRequest = HappyTravel.Edo.Data.Bookings.BookingRequest;

namespace HappyTravel.Edo.Data
{
    public class EdoContext : DbContext
    {
        public EdoContext(DbContextOptions<EdoContext> options) : base(options)
        { }


        private DbSet<ItnNumerator> ItnNumerators { get; set; }

        public virtual DbSet<Country> Countries { get; set; }
        public virtual DbSet<Agent> Agents { get; set; }
        public virtual DbSet<AgentAgencyRelation> AgentAgencyRelations { get; set; }
        public virtual DbSet<Market> Markets { get; set; }
        public virtual DbSet<Region> Regions { get; set; }
        public virtual DbSet<Bookings.Booking> Bookings { get; set; }

        public DbSet<BookingRequest> BookingRequests { get; set; }

        public DbSet<UserInvitation> UserInvitations { get; set; }

        public virtual DbSet<AgencyAccount> AgencyAccounts { get; set; }

        public virtual DbSet<Administrator> Administrators { get; set; }

        public DbSet<ManagementAuditLogEntry> ManagementAuditLog { get; set; }
        public virtual DbSet<Payment> Payments { get; set; }
        public virtual DbSet<AccountBalanceAuditLogEntry> AccountBalanceAuditLogs { get; set; }
        public DbSet<OfflinePaymentAuditLogEntry> OfflinePaymentAuditLogs { get; set; }
        public DbSet<MarkupPolicyAuditLogEntry> MarkupPolicyAuditLogs { get; set; }
        public DbSet<CreditCardAuditLogEntry> CreditCardAuditLogs { get; set; }

        public virtual DbSet<MarkupPolicy> MarkupPolicies { get; set; }

        public virtual DbSet<Agency> Agencies { get; set; }
        public DbSet<AppliedBookingMarkup> AppliedBookingMarkups { get; set; }
        public DbSet<MaterializationBonusLog> MaterializationBonusLogs { get; set; }

        public DbSet<SupplierOrder> SupplierOrders { get; set; }

        public DbSet<ServiceAccount> ServiceAccounts { get; set; }

        public virtual DbSet<PaymentLink> PaymentLinks { get; set; }

        public DbSet<BookingAuditLogEntry> BookingAuditLog { get; set; }

        public virtual DbSet<StaticData.StaticData> StaticData { get; set; }

        public virtual DbSet<Invoice> Invoices { get; set; }

        public virtual DbSet<Receipt> Receipts { get; set; }

        public virtual DbSet<AgentSystemSettings> AgentSystemSettings { get; set; }

        public virtual DbSet<AgencySystemSettings> AgencySystemSettings { get; set; }

        public DbSet<UploadedImage> UploadedImages { get; set; }

        public DbSet<ApiClient> ApiClients { get; set; }
        public virtual DbSet<DisplayMarkupFormula> DisplayMarkupFormulas { get; set; }
        public virtual DbSet<BookingStatusHistoryEntry> BookingStatusHistory { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<NotificationOptions> NotificationOptions { get; set; }

        public virtual DbSet<AgentRole> AgentRoles { get; set; }
        public virtual DbSet<AdministratorRole> AdministratorRoles { get; set; }
        public DbSet<DefaultNotificationOptions> DefaultNotificationOptions { get; set; }
        public virtual DbSet<BookingConfirmationHistoryEntry> BookingConfirmationHistory { get; set; }
        public virtual DbSet<BalanceNotificationSetting> BalanceNotificationSettings { get; set; }
        public virtual DbSet<NGeniusRefund> NGeniusRefunds { get; set; }
        public virtual DbSet<AgencyMarkupBonusesAccount> AgencyMarkupBonusesAccounts { get; set; }
        public DbSet<AgentDirectApiClientRelation> AgentDirectApiClientRelations { get; set; }

        public DbSet<CompanyBank> CompanyBanks { get; set; }
        public DbSet<CompanyAccount> CompanyAccounts { get; set; }


        public virtual Task<long> GetNextItineraryNumber() => ExecuteScalarCommand<long>($"SELECT nextval('{ItnSequence}')");


        public async virtual Task<int> GenerateNextItnMember(string itn)
        {
            var entityInfo = this.GetEntityInfo<ItnNumerator>();
            var currentNumberColumn = entityInfo.PropertyMapping[nameof(ItnNumerator.CurrentNumber)];
            var itnNumberColumn = entityInfo.PropertyMapping[nameof(ItnNumerator.ItineraryNumber)];

            return (await ItnNumerators
                    .FromSqlRaw(
                        $"UPDATE {entityInfo.Schema}.\"{entityInfo.Table}\" SET \"{currentNumberColumn}\" = \"{currentNumberColumn}\" + 1 WHERE \"{itnNumberColumn}\" = '{itn}' RETURNING *;",
                        itn)
                    // Materializing query here because EF cannot compose queries with 'UPDATE'
                    .ToListAsync())
                .Select(c => c.CurrentNumber)
                .Single();
        }


        public virtual Task RegisterItn(string itn)
        {
            ItnNumerators.Add(new ItnNumerator
            {
                ItineraryNumber = itn,
                CurrentNumber = 0
            });
            return SaveChangesAsync();
        }


        public async Task<bool> TryAddEntityLock(string lockId, string lockerInfo, string token)
        {
            var entityInfo = this.GetEntityInfo<EntityLock>();
            var lockIdColumn = entityInfo.PropertyMapping[nameof(EntityLock.EntityDescriptor)];
            var lockerInfoColumn = entityInfo.PropertyMapping[nameof(EntityLock.LockerInfo)];
            var tokenColumn = entityInfo.PropertyMapping[nameof(EntityLock.Token)];

            var sql = "WITH inserted AS " +
                $"(INSERT INTO {entityInfo.Schema}.\"{entityInfo.Table}\" (\"{lockIdColumn}\", \"{lockerInfoColumn}\", \"{tokenColumn}\") " +
                $"VALUES ('{lockId}', '{lockerInfo}', '{token}') ON CONFLICT (\"{lockIdColumn}\") DO NOTHING  RETURNING \"{tokenColumn}\") " +
                $"SELECT \"{tokenColumn}\" FROM inserted " +
                $"UNION SELECT \"{tokenColumn}\" FROM public.\"{entityInfo.Table}\" " +
                $"WHERE \"{lockIdColumn}\" = '{lockId}';";

            var currentLockToken = await ExecuteScalarCommand<string>(sql);
            return currentLockToken == token;
        }


        public Task RemoveEntityLock(string lockId)
        {
            var entityMapping = this.GetEntityInfo<EntityLock>();
            return ExecuteNonQueryCommand(
                $"DELETE FROM {entityMapping.Schema}.\"{entityMapping.Table}\" where \"{entityMapping.PropertyMapping[nameof(EntityLock.EntityDescriptor)]}\" = '{lockId}';");
        }


        private async Task<T> ExecuteScalarCommand<T>(string commandText)
        {
            using (var command = CreateCommand(commandText))
            {
                return (T)await command.ExecuteScalarAsync();
            }
        }


        private async Task ExecuteNonQueryCommand(string commandText)
        {
            using (var command = CreateCommand(commandText))
            {
                await command.ExecuteNonQueryAsync();
            }
        }


        private DbCommand CreateCommand(string commandText)
        {
            var command = Database.GetDbConnection().CreateCommand();
            command.CommandType = CommandType.Text;
            command.CommandText = commandText;

            if (command.Connection.State == ConnectionState.Closed)
                command.Connection.Open();

            return command;
        }


        public virtual void Detach(object entity)
        {
            Entry(entity).State = EntityState.Detached;
        }


        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.HasPostgresExtension("postgis")
                .HasPostgresExtension("uuid-ossp");

            builder.HasSequence<long>(ItnSequence)
                .StartsAt(1)
                .IncrementsBy(1);

            BuildCountry(builder);
            BuildMarket(builder);
            BuildRegion(builder);
            BuildAgent(builder);
            BuildAgentAgencyRelation(builder);
            BuildBooking(builder);
            BuildBookingRequests(builder);
            BuildPayment(builder);

            BuildItnNumerator(builder);
            BuildInvitations(builder);
            BuildAdministrators(builder);
            BuildAgencyAccounts(builder);
            BuildAuditEventLog(builder);
            BuildAccountAuditEventLog(builder);
            BuildCreditCardAuditEventLog(builder);
            BuildOfflinePaymentAuditEventLog(builder);
            BuildMarkupPolicyAuditEventLog(builder);
            BuildEntityLocks(builder);
            BuildMarkupPolicies(builder);
            BuildAgencies(builder);
            BuildSupplierOrders(builder);
            BuildPaymentLinks(builder);
            BuildServiceAccounts(builder);
            BuildBookingAuditLog(builder);
            BuildStaticData(builder);
            BuildInvoices(builder);
            BuildReceipts(builder);
            BuildAgentSystemSettings(builder);
            BuildAgencySystemSettings(builder);
            BuildUploadedImages(builder);
            BuildBookingMarkup(builder);
            BuildMaterializationBonusLog(builder);
            BuildApiClients(builder);
            BuildDisplayMarkupFormulas(builder);
            BuildBookingStatusHistory(builder);
            BuildNotifications(builder);
            BuildNotificationOptions(builder);
            BuildDefaultNotificationOptions(builder);
            BuildBookingConfirmationHistory(builder);
            BuildNGeniusRefund(builder);
            BuildAgencyMarkupBonusesAccounts(builder);
            BuildAgentDirectApiClientRelations(builder);
            BuildCompanyBanks(builder);
            BuildCompanyAccounts(builder);
        }


        private void BuildPaymentLinks(ModelBuilder builder)
        {
            builder.Entity<PaymentLink>(link =>
            {
                link.HasKey(l => l.Code);
                link.Property(l => l.Currency).IsRequired();
                link.Property(l => l.ServiceType).IsRequired();
                link.Property(l => l.Amount).IsRequired();
                link.Property(l => l.Created).IsRequired();
                link.Property(l => l.LastPaymentResponse).HasColumnType("jsonb");
                link.Property(l => l.ReferenceCode).IsRequired();
                link.HasIndex(l => l.ReferenceCode);
            });
        }


        private void BuildSupplierOrders(ModelBuilder builder)
        {
            builder.Entity<SupplierOrder>(order =>
            {
                order.HasKey(o => o.Id);
                order.HasIndex(o => o.ReferenceCode);
                order.HasIndex(o => o.SupplierCode);
                order.HasIndex(o => o.Type);
                order.Property(o => o.ConvertedPrice).IsRequired();
                order.Property(o => o.Price).IsRequired();
                order.Property(o => o.State).IsRequired();
                order.Property(o => o.ReferenceCode).IsRequired();
                order.Property(o => o.Modified).IsRequired();
                order.Property(o => o.Created).IsRequired();
                order.Property(o => o.Deadline).HasColumnType("jsonb");
            });
        }


        private void BuildAgencies(ModelBuilder builder)
        {
            builder.Entity<Agency>(agency =>
            {
                agency.HasKey(a => a.Id);
                agency.Property(a => a.Modified).IsRequired();
                agency.Property(a => a.Created).IsRequired();
                agency.Property(a => a.Name).IsRequired();
                agency.Property(a => a.IsActive)
                    .IsRequired()
                    .HasDefaultValue(true);
                agency.HasIndex(a => a.Ancestors)
                    .HasMethod("gin");
                agency.Property(a => a.Address).IsRequired();
                agency.Property(a => a.City).IsRequired();
                agency.Property(a => a.CountryCode).IsRequired();
                agency.Property(a => a.Phone).IsRequired();
                agency.Property(a => a.PreferredCurrency).IsRequired();
                agency.Property(a => a.VerificationState).IsRequired().HasDefaultValue(AgencyVerificationStates.PendingVerification);
                agency.Property(a => a.PreferredPaymentMethod).IsRequired();
                agency.Property(a => a.LegalAddress).IsRequired();
                agency.Property(b => b.CreditLimit).HasColumnType("jsonb");
            });
        }


        private void BuildMarkupPolicies(ModelBuilder builder)
        {
            builder.Entity<MarkupPolicy>(policy =>
            {
                policy.HasKey(l => l.Id);

                policy.Property(l => l.Created).IsRequired();
                policy.Property(l => l.Modified).IsRequired();
                policy.Property(l => l.FunctionType).IsRequired();
                policy.Property(l => l.Value).IsRequired();

                policy.HasIndex(b => b.SubjectScopeType);
                policy.HasIndex(b => b.SubjectScopeId);
                policy.HasIndex(b => b.DestinationScopeId);
            });
        }


        private void BuildEntityLocks(ModelBuilder builder)
        {
            builder.Entity<EntityLock>(entityLock =>
            {
                entityLock.HasKey(l => l.EntityDescriptor);
                entityLock.Property(l => l.Token).IsRequired();
                entityLock.Property(l => l.LockerInfo).IsRequired();
                entityLock.ToTable(nameof(EntityLock));
            });
        }


        private void BuildAuditEventLog(ModelBuilder builder)
        {
            builder.Entity<ManagementAuditLogEntry>(log =>
            {
                log.HasKey(l => l.Id);
                log.Property(l => l.Created).IsRequired();
                log.Property(l => l.Type).IsRequired();
                log.Property(l => l.AdministratorId).IsRequired();
                log.Property(l => l.EventData).IsRequired();
            });
        }


        private void BuildInvitations(ModelBuilder builder)
        {
            builder.Entity<UserInvitation>(inv =>
            {
                inv.HasKey(i => i.CodeHash);
                inv.Property(i => i.Email).IsRequired();
                inv.Property(i => i.Created).IsRequired();
                inv.Property(i => i.InviterUserId).IsRequired();
                inv.Property(i => i.InvitationStatus).HasDefaultValue(UserInvitationStatuses.Active);
                inv.Property(i => i.InvitationType).IsRequired();
            });
        }


        private void BuildAdministrators(ModelBuilder builder)
        {
            builder.Entity<Administrator>(adm =>
            {
                adm.HasKey(a => a.Id);
                adm.Property(a => a.LastName).IsRequired();
                adm.Property(a => a.FirstName).IsRequired();
                adm.Property(a => a.Position).IsRequired();
                adm.Property(a => a.Email).IsRequired();
                adm.HasIndex(a => a.IdentityHash);
                adm.Property(a => a.IsActive).HasDefaultValue(true);
            });
        }


        private void BuildAgencyAccounts(ModelBuilder builder)
        {
            builder.Entity<AgencyAccount>(acc =>
            {
                acc.HasKey(a => a.Id);
                acc.Property(a => a.Currency).IsRequired();
                acc.Property(a => a.AgencyId).IsRequired();
                acc.Property(a => a.IsActive)
                    .IsRequired()
                    .HasDefaultValue(true);
            });
        }


        private void BuildItnNumerator(ModelBuilder builder)
        {
            builder.Entity<ItnNumerator>()
                .HasKey(n => n.ItineraryNumber);

            builder.Entity<ItnNumerator>().ToTable(nameof(ItnNumerator));
        }


        private static void BuildCountry(ModelBuilder builder)
        {
            builder.Entity<Country>()
                .HasKey(c => c.Code);
            builder.Entity<Country>()
                .Property(c => c.Code)
                .IsRequired();
            builder.Entity<Country>()
                .Property(c => c.Names)
                .HasColumnType("jsonb")
                .IsRequired();
            builder.Entity<Country>()
                .Property(c => c.MarketId)
                .IsRequired();
        }


        private static void BuildMarket(ModelBuilder builder)
        {
            builder.Entity<Market>()
                .HasKey(c => c.Id);
            builder.Entity<Market>()
                .Property(c => c.Id)
                .IsRequired();
            builder.Entity<Market>()
                .Property(c => c.Names)
                .HasColumnType("jsonb")
                .IsRequired();
        }


        private static void BuildRegion(ModelBuilder builder)
        {
            builder.Entity<Region>()
                .HasKey(c => c.Id);
            builder.Entity<Region>()
                .Property(c => c.Id)
                .IsRequired();
            builder.Entity<Region>()
                .Property(c => c.Names)
                .HasColumnType("jsonb")
                .IsRequired();
        }


        private void BuildAgent(ModelBuilder builder)
        {
            builder.Entity<Agent>(agent =>
            {
                agent.HasKey(a => a.Id);
                agent.Property(a => a.Id).ValueGeneratedOnAdd();
                agent.Property(a => a.Email).IsRequired();
                agent.Property(a => a.Title).IsRequired();
                agent.Property(a => a.FirstName).IsRequired();
                agent.Property(a => a.LastName).IsRequired();
                agent.Property(a => a.FirstName).IsRequired();
                agent.Property(a => a.Position).IsRequired();
                agent.Property(a => a.PreferredCurrency).IsRequired();
                agent.Property(a => a.IdentityHash).IsRequired();
                agent.Property(a => a.AppSettings).HasColumnType("jsonb");
                agent.Property(a => a.UserSettings).HasColumnType("jsonb");
            });
        }


        private void BuildAgentAgencyRelation(ModelBuilder builder)
        {
            builder.Entity<AgentAgencyRelation>(relation =>
            {
                relation.ToTable("AgentAgencyRelations");

                relation.HasKey(r => new { r.AgentId, r.AgencyId });
                relation.Property(r => r.AgentId).IsRequired();
                relation.Property(r => r.Type).IsRequired();
            });
        }


        private void BuildBooking(ModelBuilder builder)
        {
            builder.Entity<Bookings.Booking>(booking =>
            {
                booking.HasKey(b => b.Id);

                booking.Property(b => b.AgentId).IsRequired();
                booking.HasIndex(b => b.AgentId);

                booking.Property(b => b.ReferenceCode).IsRequired();
                booking.HasIndex(b => b.ReferenceCode);

                booking.Property(b => b.Status).IsRequired();
                booking.Property(b => b.ItineraryNumber).IsRequired();
                booking.HasIndex(b => b.ItineraryNumber);

                booking.Property(b => b.MainPassengerName);
                booking.HasIndex(b => b.MainPassengerName);

                booking.Property(b => b.LanguageCode)
                    .IsRequired()
                    .HasDefaultValue("en");

                booking.Property(b => b.AccommodationId)
                    .IsRequired();

                booking.Property(b => b.AccommodationName)
                    .IsRequired();

                booking.Property(b => b.AccommodationInfo)
                    .HasColumnType("jsonb");

                booking.Property(b => b.Location)
                    .HasColumnType("jsonb");

                booking.Property(b => b.Rooms)
                    .HasColumnType("jsonb")
                    .HasConversion(
                        value => JsonConvert.SerializeObject(value),
                        value => JsonConvert.DeserializeObject<List<BookedRoom>>(value))
                    .HasDefaultValueSql("'[]'::jsonb");

                booking.Property(b => b.CancellationPolicies)
                    .HasColumnType("jsonb")
                    .HasDefaultValueSql("'[]'::jsonb");

                booking.Property(b => b.SpecialValues)
                    .HasColumnType("jsonb");

                booking.HasIndex(b => b.IsDirectContract);
                booking.HasIndex(b => b.ClientReferenceCode);
            });
        }


        private void BuildBookingRequests(ModelBuilder builder)
        {
            builder.Entity<BookingRequest>(bookingRequest =>
            {
                bookingRequest.HasKey(b => b.ReferenceCode);
            });

        }


        private void BuildPayment(ModelBuilder builder)
        {
            builder
                .Entity<Payment>(payment =>
                {
                    payment.HasKey(p => p.Id);
                    payment.HasIndex(p => p.ReferenceCode);
                    payment.Property(p => p.Data).HasColumnType("jsonb").IsRequired();
                    payment.Property(p => p.AccountNumber).IsRequired();
                    payment.Property(p => p.Amount).IsRequired();
                    payment.Property(p => p.Currency).IsRequired().HasConversion<string>();
                    payment.Property(p => p.Created).IsRequired();
                    payment.Property(p => p.Status).IsRequired();
                });
        }


        private void BuildAccountAuditEventLog(ModelBuilder builder)
        {
            builder.Entity<AccountBalanceAuditLogEntry>(log =>
            {
                log.HasKey(l => l.Id);
                log.Property(l => l.Created).IsRequired();
                log.Property(l => l.Type).IsRequired();
                log.Property(l => l.AccountId).IsRequired();
                log.Property(l => l.ApiCallerType).IsRequired();
                log.Property(l => l.UserId).IsRequired();
                log.Property(l => l.Amount).IsRequired();
                log.Property(l => l.EventData).IsRequired();
            });
        }


        private void BuildCreditCardAuditEventLog(ModelBuilder builder)
        {
            builder.Entity<CreditCardAuditLogEntry>(log =>
            {
                log.HasKey(l => l.Id);
                log.Property(l => l.Created).IsRequired();
                log.Property(l => l.Type).IsRequired();
                log.Property(l => l.MaskedNumber).IsRequired();
                log.Property(l => l.ApiCallerType).IsRequired();
                log.Property(l => l.UserId).IsRequired();
                log.Property(l => l.AgentId).IsRequired();
                log.Property(l => l.Amount).IsRequired();
                log.Property(l => l.Currency).IsRequired();
                log.Property(l => l.EventData).IsRequired();
            });
        }


        private void BuildOfflinePaymentAuditEventLog(ModelBuilder builder)
        {
            builder.Entity<OfflinePaymentAuditLogEntry>(log =>
            {
                log.HasKey(l => l.Id);
                log.Property(l => l.Created).IsRequired();
                log.Property(l => l.ApiCallerType).IsRequired();
                log.Property(l => l.UserId).IsRequired();
            });
        }


        private void BuildMarkupPolicyAuditEventLog(ModelBuilder builder)
        {
            builder.Entity<MarkupPolicyAuditLogEntry>(log =>
            {
                log.HasKey(l => l.Id);
                log.Property(l => l.Created).IsRequired();
                log.Property(l => l.ApiCallerType).IsRequired();
                log.Property(l => l.UserId).IsRequired();
            });
        }


        private void BuildServiceAccounts(ModelBuilder builder)
        {
            builder.Entity<ServiceAccount>(account =>
            {
                account.HasKey(a => a.Id);
                account.Property(a => a.ClientId).IsRequired();
            });
        }


        private void BuildBookingAuditLog(ModelBuilder builder)
        {
            builder.Entity<BookingAuditLogEntry>(br =>
            {
                builder.Entity<BookingAuditLogEntry>().ToTable("BookingAuditLog");
                br.HasKey(b => b.Id);
                br.Property(b => b.Id).ValueGeneratedOnAdd();

                br.Property(b => b.CreatedAt)
                    .HasDefaultValueSql("NOW()")
                    .ValueGeneratedOnAdd();

                br.Property(b => b.BookingDetails)
                    .HasColumnType("jsonb")
                    .IsRequired();
            });
        }


        private void BuildStaticData(ModelBuilder builder)
        {
            builder.Entity<StaticData.StaticData>(staticData =>
            {
                staticData.HasKey(sd => sd.Type);
                staticData.Property(sd => sd.Data)
                    .HasColumnType("jsonb")
                    .IsRequired();
            });
        }


        private void BuildInvoices(ModelBuilder builder)
        {
            builder.Entity<Invoice>(i =>
            {
                i.HasKey(i => i.Id);
                i.Property(i => i.ParentReferenceCode).IsRequired();
                i.Property(i => i.Number).IsRequired();
                i.HasIndex(i => new { i.ServiceSource, i.ServiceType, i.ParentReferenceCode });
                i.HasIndex(i => i.Number);
            });
        }


        private void BuildReceipts(ModelBuilder builder)
        {
            builder.Entity<Receipt>(receipt =>
            {
                receipt.HasKey(i => i.Id);
                receipt.Property(i => i.ParentReferenceCode).IsRequired();
                receipt.Property(i => i.Number).IsRequired();
                receipt.HasIndex(i => new { i.ServiceSource, i.ServiceType, i.ParentReferenceCode });
                receipt.HasIndex(i => i.InvoiceId);
                receipt.Property(i => i.InvoiceId).IsRequired();
            });
        }


        private void BuildAgentSystemSettings(ModelBuilder builder)
        {
            builder.Entity<AgentSystemSettings>(settings =>
            {
                settings.HasKey(r => new { r.AgentId, r.AgencyId });
                settings.Property(r => r.AccommodationBookingSettings).HasColumnType("jsonb");
                settings.Property(r => r.EnabledSuppliers).HasColumnType("jsonb");
            });
        }

        private void BuildAgencySystemSettings(ModelBuilder builder)
        {
            builder.Entity<AgencySystemSettings>(settings =>
            {
                settings.HasKey(r => r.AgencyId);
                settings.Property(r => r.AccommodationBookingSettings).HasColumnType("jsonb");
                settings.Property(r => r.EnabledSuppliers).HasColumnType("jsonb");
            });
        }


        private void BuildUploadedImages(ModelBuilder builder)
        {
            builder.Entity<UploadedImage>(settings =>
            {
                settings.HasIndex(i => new { i.AgencyId, i.FileName });
            });
        }


        private void BuildBookingMarkup(ModelBuilder builder)
        {
            builder.Entity<AppliedBookingMarkup>(bookingMarkup =>
            {
                bookingMarkup.HasIndex(x => x.Paid);
            });
        }


        private void BuildMaterializationBonusLog(ModelBuilder builder)
        {
            builder.Entity<MaterializationBonusLog>(log =>
            {
                log.HasKey(x => new { x.ReferenceCode, x.PolicyId });
            });
        }


        private void BuildApiClients(ModelBuilder builder)
        {
            builder.Entity<ApiClient>(ac =>
            {
                ac.Property(a => a.Name).IsRequired();
                ac.Property(a => a.PasswordHash).IsRequired();
                ac.Property(a => a.AgencyId).IsRequired();
                ac.Property(a => a.AgentId).IsRequired();

                ac.HasIndex(a => new { a.Name, a.PasswordHash });
                ac.HasIndex(a => a.AgencyId);
                ac.HasIndex(a => a.AgentId);
            });
        }


        private static void BuildDisplayMarkupFormulas(ModelBuilder builder)
        {
            builder.Entity<DisplayMarkupFormula>(b =>
            {
                b.HasIndex(f => new { f.AgencyId, f.AgentId }).IsUnique();
                b.Property(f => f.DisplayFormula).IsRequired();
            });
        }


        private static void BuildBookingStatusHistory(ModelBuilder builder)
        {
            builder.Entity<BookingStatusHistoryEntry>(e =>
            {
                e.HasKey(bshe => bshe.Id);
                e.HasIndex(bshe => bshe.BookingId);
                e.HasIndex(bshe => bshe.UserId);
                e.HasIndex(bshe => bshe.ApiCallerType);
                e.Property(bshe => bshe.AgencyId);
                e.Property(bshe => bshe.CreatedAt).IsRequired();
                e.Property(bshe => bshe.Status).IsRequired();
                e.Property(bshe => bshe.Initiator).IsRequired();
                e.Property(bshe => bshe.Source).IsRequired();
                e.Property(bshe => bshe.Event).IsRequired();
                e.Property(bshe => bshe.Reason);
                e.ToTable("BookingStatusHistory");
            });
        }


        private static void BuildNotifications(ModelBuilder builder)
        {
            builder.Entity<Notification>(b =>
            {
                b.Property(n => n.Message).HasColumnType("jsonb");
                b.Property(n => n.SendingSettings).HasColumnType("jsonb");
                b.HasIndex(n => n.Receiver);
                b.HasIndex(n => n.UserId);
                b.HasIndex(n => n.SendingStatus);
            });
        }


        private static void BuildNotificationOptions(ModelBuilder builder)
        {
            builder.Entity<NotificationOptions>(b =>
            {
                b.HasIndex(o => new { o.AgencyId, o.UserId, o.UserType, o.Type }).IsUnique();
            });
        }


        private static void BuildDefaultNotificationOptions(ModelBuilder builder)
        {
            builder.Entity<DefaultNotificationOptions>(e =>
            {
                e.HasKey(o => o.Type);
                e.Property(o => o.EnabledProtocols).IsRequired();
                e.Property(o => o.IsMandatory).IsRequired();
                e.Property(o => o.EnabledReceivers).IsRequired();
                e.Property(o => o.AgentEmailTemplateId);
                e.Property(o => o.AdminEmailTemplateId);
                e.Property(o => o.PropertyOwnerEmailTemplateId);
            });
        }


        private static void BuildBookingConfirmationHistory(ModelBuilder builder)
        {
            builder.Entity<BookingConfirmationHistoryEntry>(e =>
            {
                e.HasKey(hche => hche.Id);
                e.Property(hche => hche.ReferenceCode).IsRequired();
                e.HasIndex(hche => hche.ReferenceCode);
                e.Property(hche => hche.Status).IsRequired();
                e.Property(hche => hche.Initiator).IsRequired();
                e.HasIndex(hche => hche.CreatedAt);
                e.ToTable("BookingConfirmationHistory");
            });
        }


        private static void BuildNGeniusRefund(ModelBuilder builder)
        {
            builder.Entity<NGeniusRefund>(e =>
            {
                e.HasKey(r => r.Id);
                e.HasIndex(r => r.PlannedDate);
                e.ToTable("NGeniusRefunds");
            });
        }


        private static void BuildAgencyMarkupBonusesAccounts(ModelBuilder builder)
        {
            builder.Entity<AgencyMarkupBonusesAccount>(b =>
            {
                b.HasKey(a => a.Id);
                b.HasIndex(a => a.AgencyId);
                b.ToTable("AgencyMarkupBonusesAccounts");
            });
        }


        private static void BuildAgentDirectApiClientRelations(ModelBuilder builder)
        {
            builder.Entity<AgentDirectApiClientRelation>(b =>
            {
                b.HasKey(a => new { a.AgentId, a.AgencyId, a.DirectApiClientId });
            });
        }


        private static void BuildCompanyBanks(ModelBuilder builder)
        {
            builder.Entity<CompanyBank>(b =>
            {
                b.HasKey(cb => cb.Id);
                b.Property(cb => cb.Name).IsRequired();
                b.Property(cb => cb.Address).IsRequired();
                b.Property(cb => cb.RoutingCode).IsRequired();
                b.Property(cb => cb.SwiftCode).IsRequired();
                b.Property(cb => cb.Created).IsRequired();
                b.Property(cb => cb.Modified).IsRequired();
                b.Navigation(cb => cb.CompanyAccounts);
                b.ToTable("CompanyBanks");
            });
        }


        private static void BuildCompanyAccounts(ModelBuilder builder)
        {
            builder.Entity<CompanyAccount>(b =>
            {
                b.HasKey(ca => ca.Id);
                b.HasIndex(ca => ca.CompanyBankId);
                b.Property(ca => ca.Currency).IsRequired();
                b.Property(ca => ca.AccountNumber).IsRequired();
                b.Property(ca => ca.Iban).IsRequired();
                b.Property(ca => ca.IsDefault).IsRequired();
                b.Property(ca => ca.IntermediaryBankName);
                b.Property(ca => ca.IntermediaryBankAccountNumber);
                b.Property(ca => ca.IntermediaryBankSwiftCode);
                b.Property(ca => ca.IntermediaryBankAbaNo);
                b.Property(cb => cb.Created).IsRequired();
                b.Property(cb => cb.Modified).IsRequired();
                b.HasOne(ca => ca.CompanyBank).WithMany(cb => cb.CompanyAccounts).IsRequired().OnDelete(DeleteBehavior.Cascade);
                b.ToTable("CompanyAccounts");
            });
        }


        private const string ItnSequence = "itn_seq";
    }
}