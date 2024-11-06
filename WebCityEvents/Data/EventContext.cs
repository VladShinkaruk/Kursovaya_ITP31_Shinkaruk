using Microsoft.EntityFrameworkCore;
using WebCityEvents.Models;

namespace WebCityEvents.Data
{
    public class EventContext : DbContext
    {
        public EventContext(DbContextOptions<EventContext> options) : base(options) { }
        public EventContext() : base() { }
        public DbSet<Place> Places { get; set; }
        public DbSet<Organizer> Organizers { get; set; }
        public DbSet<Customer> Customers { get; set; }
        public DbSet<Event> Events { get; set; }
        public DbSet<TicketOrder> TicketOrders { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                ConfigurationBuilder builder = new();
                builder.SetBasePath(Directory.GetCurrentDirectory());
                builder.AddJsonFile("appsettings.json");
                IConfigurationRoot configuration = builder.Build();

                string connectionString = configuration.GetConnectionString("LocalSQLConnection");

                optionsBuilder
                    .UseSqlServer(connectionString)
                    .LogTo(message => System.Diagnostics.Debug.WriteLine(message));
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<TicketOrder>()
                .HasKey(t => t.OrderID);
        }

        public async Task<bool> IsDatabaseEmptyAsync()
        {
            return !await Customers.AnyAsync();
        }
    }
}