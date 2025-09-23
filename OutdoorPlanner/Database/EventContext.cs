using Microsoft.EntityFrameworkCore;
using OutdoorPlanner.Core;

namespace OutdoorPlanner.Database
{
    internal class EventContext : DbContext
    {
        public DbSet<Event> Events => Set<Event>();
        public EventContext() => Database.EnsureCreated();

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseNpgsql(Program._connectionString);
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Event>()
                .Property(e => e.Id)
                .ValueGeneratedOnAdd();
        }

    }
}

