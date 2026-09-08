using CourtBooker.Models;
using Microsoft.EntityFrameworkCore;

namespace CourtBooker.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users { get; set; }
    public DbSet<Court> Courts { get; set; }
    public DbSet<Reservation> Reservations { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Reservation>()
            .HasOne(r => r.User)
            .WithMany(u => u.Reservations)
            .HasForeignKey(r => r.UserId);

        modelBuilder.Entity<Reservation>()
            .HasOne(r => r.Court)
            .WithMany(c => c.Reservations)
            .HasForeignKey(r => r.CourtId);
    }
}