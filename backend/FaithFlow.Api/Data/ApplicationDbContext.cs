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
    public DbSet<Department> Departments { get; set; }
    public DbSet<DepartmentRequestType> DepartmentRequestTypes { get; set; }
    public DbSet<UserDepartment> UserDepartments { get; set; }
    public DbSet<Notification> Notifications { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // ── Prayer relationships ─────────────────────────────────────────
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

        // ── Notification → Prayer ────────────────────────────────────────
        modelBuilder.Entity<Notification>()
            .HasOne(n => n.Prayer)
            .WithMany()
            .HasForeignKey(n => n.PrayerId)
            .OnDelete(DeleteBehavior.Cascade);

        // ── DepartmentRequestType join ───────────────────────────────────
        modelBuilder.Entity<DepartmentRequestType>()
            .HasKey(d => new { d.DepartmentId, d.RequestTypeId });

        modelBuilder.Entity<DepartmentRequestType>()
            .HasOne(d => d.Department)
            .WithMany(dept => dept.DepartmentRequestTypes)
            .HasForeignKey(d => d.DepartmentId);

        modelBuilder.Entity<DepartmentRequestType>()
            .HasOne(d => d.RequestType)
            .WithMany()
            .HasForeignKey(d => d.RequestTypeId);

        // ── UserDepartment → Department ──────────────────────────────────
        modelBuilder.Entity<UserDepartment>()
            .HasOne(ud => ud.Department)
            .WithMany(d => d.Members)
            .HasForeignKey(ud => ud.DepartmentId)
            .OnDelete(DeleteBehavior.Cascade);

        // Unique: one membership record per user per department
        modelBuilder.Entity<UserDepartment>()
            .HasIndex(ud => new { ud.UserId, ud.DepartmentId })
            .IsUnique();

        // ── Seed: RequestTypes ───────────────────────────────────────────
        modelBuilder.Entity<RequestType>().HasData(
            new RequestType { Id = 1, Name = "Ride" },
            new RequestType { Id = 2, Name = "Prayer" },
            new RequestType { Id = 3, Name = "Supply" },
            new RequestType { Id = 4, Name = "Service" },
            new RequestType { Id = 5, Name = "Labor" }
        );

        // ── Seed: Departments ────────────────────────────────────────────
        modelBuilder.Entity<Department>().HasData(
            new Department { Id = 1, Name = "Chaplain Services",    Description = "Handles prayer and pastoral care requests." },
            new Department { Id = 2, Name = "Transportation",       Description = "Handles ride and transport requests." },
            new Department { Id = 3, Name = "Supply & Logistics",   Description = "Handles supply and materials requests." },
            new Department { Id = 4, Name = "Facilities",           Description = "Handles service, maintenance, and labor requests." }
        );

        // ── Seed: Department ↔ RequestType mappings ──────────────────────
        modelBuilder.Entity<DepartmentRequestType>().HasData(
            new DepartmentRequestType { DepartmentId = 1, RequestTypeId = 2 }, // Chaplain ← Prayer
            new DepartmentRequestType { DepartmentId = 2, RequestTypeId = 1 }, // Transportation ← Ride
            new DepartmentRequestType { DepartmentId = 3, RequestTypeId = 3 }, // Supply & Logistics ← Supply
            new DepartmentRequestType { DepartmentId = 4, RequestTypeId = 4 }, // Facilities ← Service
            new DepartmentRequestType { DepartmentId = 4, RequestTypeId = 5 }  // Facilities ← Labor
        );
    }
}
