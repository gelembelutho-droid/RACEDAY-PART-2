using Microsoft.EntityFrameworkCore;
using RaceDay.API.Models;

namespace RaceDay.API.Data
{
    public class RaceDayDbContext : DbContext
    {
        public RaceDayDbContext(DbContextOptions<RaceDayDbContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Event> Events { get; set; }
        public DbSet<Race> Races { get; set; }
        public DbSet<Registration> Registrations { get; set; }
        public DbSet<Result> Results { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<Sponsor> Sponsors { get; set; }
        public DbSet<EventSponsor> EventSponsors { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ============================================================
            // COMPOSITE KEYS
            // ============================================================
            modelBuilder.Entity<EventSponsor>()
                .HasKey(es => new { es.EventId, es.SponsorId });

            // ============================================================
            // UNIQUE CONSTRAINTS
            // ============================================================
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();

            modelBuilder.Entity<Registration>()
                .HasIndex(r => r.BibNumber)
                .IsUnique()
                .HasFilter("[BibNumber] IS NOT NULL");

            modelBuilder.Entity<Registration>()
                .HasIndex(r => new { r.UserId, r.RaceId })
                .IsUnique();

            modelBuilder.Entity<Result>()
                .HasIndex(r => r.RegistrationId)
                .IsUnique();

            modelBuilder.Entity<Payment>()
                .HasIndex(p => p.RegistrationId)
                .IsUnique();

            modelBuilder.Entity<Payment>()
                .HasIndex(p => p.TransactionId)
                .IsUnique()
                .HasFilter("[TransactionId] IS NOT NULL");

            // ============================================================
            // RELATIONSHIPS
            // ============================================================

            // User -> Events (One-to-Many)
            modelBuilder.Entity<Event>()
                .HasOne(e => e.Organizer)
                .WithMany(u => u.OrganizedEvents)
                .HasForeignKey(e => e.OrganizerId)
                .OnDelete(DeleteBehavior.Restrict);

            // User -> Registrations (One-to-Many)
            modelBuilder.Entity<Registration>()
                .HasOne(r => r.User)
                .WithMany(u => u.Registrations)
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // Race -> Registrations (One-to-Many)
            modelBuilder.Entity<Registration>()
                .HasOne(r => r.Race)
                .WithMany(ra => ra.Registrations)
                .HasForeignKey(r => r.RaceId)
                .OnDelete(DeleteBehavior.Restrict);

            // Registration -> Result (One-to-One)
            modelBuilder.Entity<Result>()
                .HasOne(r => r.Registration)
                .WithOne(reg => reg.Result)
                .HasForeignKey<Result>(r => r.RegistrationId)
                .OnDelete(DeleteBehavior.Restrict);

            // Registration -> Payment (One-to-One)
            modelBuilder.Entity<Payment>()
                .HasOne(p => p.Registration)
                .WithOne(reg => reg.Payment)
                .HasForeignKey<Payment>(p => p.RegistrationId)
                .OnDelete(DeleteBehavior.Restrict);

            // Event -> Races (One-to-Many)
            modelBuilder.Entity<Race>()
                .HasOne(r => r.Event)
                .WithMany(e => e.Races)
                .HasForeignKey(r => r.EventId)
                .OnDelete(DeleteBehavior.Restrict);

            // Event -> EventSponsors (One-to-Many)
            modelBuilder.Entity<EventSponsor>()
                .HasOne(es => es.Event)
                .WithMany(e => e.EventSponsors)
                .HasForeignKey(es => es.EventId)
                .OnDelete(DeleteBehavior.Restrict);

            // Sponsor -> EventSponsors (One-to-Many)
            modelBuilder.Entity<EventSponsor>()
                .HasOne(es => es.Sponsor)
                .WithMany(s => s.EventSponsors)
                .HasForeignKey(es => es.SponsorId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}