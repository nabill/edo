using System;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using HappyTravel.Edo.Data.Customers;
using HappyTravel.Edo.Data.Locations;
using HappyTravel.Edo.Data.Management;
using HappyTravel.Edo.Data.Numeration;
using HappyTravel.Edo.Data.Payments;
using Microsoft.EntityFrameworkCore;

namespace HappyTravel.Edo.Data
{
    public class EdoContext : DbContext
    {
        public EdoContext(DbContextOptions<EdoContext> options) : base(options)
        {

        }

        [DbFunction("jsonb_to_string")]
        public static string JsonbToString(string target)
            => throw new Exception();

        public async Task<long> GetNextItineraryNumber()
        {
            using (var command = Database.GetDbConnection().CreateCommand())
            {
                command.CommandType = CommandType.Text;
                command.CommandText = $"SELECT nextval('{ItnSequence}')";

                if (command.Connection.State == ConnectionState.Closed)
                    command.Connection.Open();

                return (long)(await command.ExecuteScalarAsync());
            }
        }

        public Task<int> GenerateNextItnMember(string itn)
        {
            const string currentNumberColumn = "CurrentNumber";
            const string itnNumberColumn = "ItineraryNumber";
            // TODO: Get table and columns info from context metadata.
            return ItnNumerator.FromSql($"UPDATE public.\"{nameof(ItnNumerator)}\" SET \"{currentNumberColumn}\" = \"{currentNumberColumn}\" + 1 WHERE \"{itnNumberColumn}\" = '{itn}' RETURNING *;", itn)
                .Select(c => c.CurrentNumber)
                .SingleAsync();
        }

        public Task RegisterItn(string itn)
        {
            ItnNumerator.Add(new ItnNumerator
            {
                ItineraryNumber = itn,
                CurrentNumber = 0
            });
            return SaveChangesAsync();
        }

        private DbSet<ItnNumerator> ItnNumerator { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.HasPostgresExtension("postgis")
                .HasPostgresExtension("uuid-ossp");

            builder.HasSequence<long>(ItnSequence)
                .StartsAt(1)
                .IncrementsBy(1);

            builder.Entity<Location>()
                .HasKey(l => l.Id);
            builder.Entity<Location>()
                .Property(l => l.Id)
                .HasDefaultValueSql("uuid_generate_v4()")
                .IsRequired();
            builder.Entity<Location>()
                .Property(l => l.Coordinates)
                .HasColumnType("geography (point)")
                .IsRequired();
            builder.Entity<Location>()
                .Property(l => l.Name)
                .HasColumnType("jsonb")
                .IsRequired();
            builder.Entity<Location>()
                .Property(l => l.Locality)
                .HasColumnType("jsonb")
                .IsRequired();
            builder.Entity<Location>()
                .Property(l => l.Country)
                .HasColumnType("jsonb")
                .IsRequired();
            builder.Entity<Location>()
                .Property(l => l.DistanceInMeters)
                .IsRequired();
            builder.Entity<Location>()
                .Property(l => l.Source)
                .IsRequired();
            builder.Entity<Location>()
                .Property(l => l.Type)
                .IsRequired();

            BuildCountry(builder);
            BuildRegion(builder);
            BuildCustomer(builder);
            BuildCompany(builder);
            BuildCustomerCompanyRelation(builder);
            BuildBooking(builder);
            BuildItnNumerator(builder);
            BuildInvitations(builder);
            BuildAdministrators(builder);
            BuildPaymentAccounts(builder);
            BuildAuditEventLog(builder);

            DataSeeder.AddData(builder);
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
                inv.Property(i => i.Created).IsRequired();
                inv.Property(i => i.Data).IsRequired();
                inv.Property(i => i.Email).IsRequired();
                inv.Property(i => i.IsAccepted).HasDefaultValue(false);
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
            });
        }
        
        private void BuildPaymentAccounts(ModelBuilder builder)
        {
            builder.Entity<PaymentAccount>(acc =>
            {
                acc.HasKey(a => a.Id);
                acc.Property(a => a.Currency).IsRequired();
                acc.Property(a => a.CompanyId).IsRequired();
            });
        }

        private void BuildItnNumerator(ModelBuilder builder)
        {
            builder.Entity<ItnNumerator>()
                .HasKey(n => n.ItineraryNumber);
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
                .Property(c => c.RegionId)
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

        private void BuildCustomer(ModelBuilder builder)
        {
            builder.Entity<Customer>(customer =>
            {
                customer.HasKey(c => c.Id);
                customer.Property(c => c.Id).ValueGeneratedOnAdd();
                customer.Property(c => c.Email).IsRequired();
                customer.Property(c => c.Title).IsRequired();
                customer.Property(c => c.FirstName).IsRequired();
                customer.Property(c => c.LastName).IsRequired();
                customer.Property(c => c.FirstName).IsRequired();
                customer.Property(c => c.Position).IsRequired();
                customer.Property(c => c.IdentityHash).IsRequired();
            });
        }

        private void BuildCompany(ModelBuilder builder)
        {
            builder.Entity<Company>(company =>
            {
                company.HasKey(c => c.Id);
                company.Property(c => c.Id).ValueGeneratedOnAdd();
                company.Property(c => c.Address).IsRequired();
                company.Property(c => c.City).IsRequired();
                company.Property(c => c.CountryCode).IsRequired();
                company.Property(c => c.Name).IsRequired();
                company.Property(c => c.Phone).IsRequired();
                company.Property(c => c.PreferredCurrency).IsRequired();
                company.Property(c => c.PreferredPaymentMethod).IsRequired();
                company.Property(c => c.State).IsRequired();
            });
        }

        private void BuildCustomerCompanyRelation(ModelBuilder builder)
        {
            builder.Entity<CustomerCompanyRelation>(relation =>
            {
                relation.ToTable("CustomerCompanyRelations");

                relation.HasKey(r => new { r.CustomerId, r.CompanyId });
                relation.Property(r => r.CompanyId).IsRequired();
                relation.Property(r => r.CustomerId).IsRequired();
                relation.Property(r => r.Type).IsRequired();
            });
        }

        private void BuildBooking(ModelBuilder builder)
        {
            builder.Entity<Booking.Booking>(booking =>
            {
                booking.HasKey(b => b.Id);
                
                booking.Property(b => b.CustomerId).IsRequired();
                booking.HasIndex(b => b.CustomerId);
                
                booking.Property(b => b.CompanyId).IsRequired();
                booking.HasIndex(b => b.CompanyId);
                
                booking.Property(b => b.ReferenceCode).IsRequired();
                booking.HasIndex(b => b.ReferenceCode);
                
                booking.Property(b => b.BookingDetails)
                    .HasColumnType("jsonb");
                
                booking.Property(b => b.ServiceDetails)
                    .HasColumnType("jsonb");
                
                booking.Property(b => b.Status).IsRequired();
                booking.Property(b => b.ItineraryNumber).IsRequired();
                booking.HasIndex(b => b.ItineraryNumber);
                
                booking.Property(b => b.MainPassengerName).IsRequired();
                booking.HasIndex(b => b.MainPassengerName);

                booking.Property(b => b.ServiceType).IsRequired();
                booking.HasIndex(b => b.ServiceType);
            });
        }


        public DbSet<Country> Countries { get; set; }
        public DbSet<Company> Companies { get; set; }
        public DbSet<Customer> Customers { get; set; }
        public DbSet<CustomerCompanyRelation> CustomerCompanyRelations { get; set; }
        public DbSet<Location> Locations { get; set; }
        public DbSet<Region> Regions { get; set; }

        private const string ItnSequence = "itn_seq";
        public DbSet<Booking.Booking> Bookings { get; set; }
        
        public DbSet<UserInvitation> UserInvitations { get; set; }
        
        public DbSet<PaymentAccount> PaymentAccounts { get; set; }
        
        public DbSet<Administrator> Administrators { get; set; }
        
        public DbSet<ManagementAuditLogEntry> ManagementAuditLog { get; set; }
    }
}
