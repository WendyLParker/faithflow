using Microsoft.EntityFrameworkCore;
using FaithFlow.Backend.Models;

namespace FaithFlow.Backend.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Prayer> Prayers { get; set; }
    public DbSet<ProgressNote> ProgressNotes { get; set; }
    public DbSet<Category> Categories { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Prayer <-> ProgressNote (One-to-Many)
        modelBuilder.Entity<ProgressNote>()
            .HasOne(pn => pn.Prayer)
            .WithMany(p => p.ProgressNotes)
            .HasForeignKey(pn => pn.PrayerId)
            .OnDelete(DeleteBehavior.Cascade);

        // Optional: Global filter to only show current user's prayers
        // modelBuilder.Entity<Prayer>()
        //     .HasQueryFilter(p => p.UserId == _currentUserService.GetUserId());
    }
}