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
    public DbSet<RequestType> RequestTypes { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<ProgressNote>()
            .HasOne(pn => pn.Prayer)
            .WithMany(p => p.ProgressNotes)
            .HasForeignKey(pn => pn.PrayerId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Prayer>()
            .HasOne(p => p.RequestType)
            .WithMany(rt => rt.Prayers)
            .HasForeignKey(p => p.RequestTypeId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<RequestType>().HasData(
            new RequestType { Id = 1, Name = "Ride" },
            new RequestType { Id = 2, Name = "Prayer" },
            new RequestType { Id = 3, Name = "Supply" },
            new RequestType { Id = 4, Name = "Service" },
            new RequestType { Id = 5, Name = "Labor" }
        );
    }
}
