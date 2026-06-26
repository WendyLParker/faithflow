using FaithFlow.Backend.Models;
using Microsoft.EntityFrameworkCore;

namespace FaithFlow.Backend.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // DbSets (Tables)
        public DbSet<Prayer> Prayers { get; set; } = null!;
        public DbSet<Category> Categories { get; set; } = null!;
        public DbSet<JournalEntry> JournalEntries { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Optional: Make UserId indexed for faster queries
            modelBuilder.Entity<Prayer>()
                .HasIndex(p => p.UserId);
        }
    }
}