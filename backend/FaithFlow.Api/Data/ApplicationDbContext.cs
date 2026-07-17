using Microsoft.EntityFrameworkCore;
using FaithFlow.Backend.Models;

namespace FaithFlow.Backend.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Request> Requests { get; set; }
    public DbSet<RequestComment> RequestComments { get; set; }
    public DbSet<ProgressNote> ProgressNotes { get; set; }
    public DbSet<RequestType> RequestTypes { get; set; }
    public DbSet<Group> Groups { get; set; }
    public DbSet<RequestGroup> RequestGroups { get; set; }
    public DbSet<UserGroup> UserGroups { get; set; }
    public DbSet<Notification> Notifications { get; set; }
    public DbSet<UserRole> UserRoles { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<ProgressNote>()
            .HasOne(pn => pn.Request)
            .WithMany(r => r.ProgressNotes)
            .HasForeignKey(pn => pn.RequestId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<RequestComment>()
            .HasOne(c => c.Request)
            .WithMany(r => r.Comments)
            .HasForeignKey(c => c.RequestId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Notification>()
            .HasOne(n => n.Comment)
            .WithMany()
            .HasForeignKey(n => n.CommentId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<Request>()
            .HasOne(r => r.RequestType)
            .WithMany(rt => rt.Requests)
            .HasForeignKey(r => r.RequestTypeId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Notification>()
            .HasOne(n => n.Request)
            .WithMany()
            .HasForeignKey(n => n.RequestId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<RequestGroup>()
            .HasKey(rg => new { rg.RequestId, rg.GroupId });

        modelBuilder.Entity<RequestGroup>()
            .HasOne(rg => rg.Request)
            .WithMany(r => r.AssignedGroups)
            .HasForeignKey(rg => rg.RequestId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<RequestGroup>()
            .HasOne(rg => rg.Group)
            .WithMany()
            .HasForeignKey(rg => rg.GroupId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<UserGroup>()
            .HasOne(ug => ug.Group)
            .WithMany(g => g.Members)
            .HasForeignKey(ug => ug.GroupId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<UserGroup>()
            .HasIndex(ug => new { ug.UserId, ug.GroupId })
            .IsUnique();

        modelBuilder.Entity<UserRole>()
            .Property(ur => ur.Id)
            .ValueGeneratedOnAdd();

        modelBuilder.Entity<UserRole>()
            .HasIndex(ur => ur.UserId)
            .IsUnique();

        modelBuilder.Entity<RequestType>().HasData(
            new RequestType { Id = 1, Name = "Ride" },
            new RequestType { Id = 2, Name = "Prayer" },
            new RequestType { Id = 3, Name = "Supply" },
            new RequestType { Id = 4, Name = "Service" },
            new RequestType { Id = 5, Name = "Labor" }
        );

        modelBuilder.Entity<Group>().HasData(
            new Group { Id = 1, Name = "Chaplain Services", Description = "Handles prayer and pastoral care requests." },
            new Group { Id = 2, Name = "Transportation", Description = "Handles ride and transport requests." },
            new Group { Id = 3, Name = "Supply & Logistics", Description = "Handles supply and materials requests." },
            new Group { Id = 4, Name = "Facilities", Description = "Handles service, maintenance, and labor requests." }
        );
    }
}
