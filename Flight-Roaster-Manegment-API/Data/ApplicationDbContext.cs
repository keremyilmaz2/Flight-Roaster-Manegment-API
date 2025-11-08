using FlightRosterAPI.Models.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace FlightRosterAPI.Data
{
    public class ApplicationDbContext : IdentityDbContext<User, IdentityRole<int>, int>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        // DbSets
        public DbSet<Aircraft> Aircrafts { get; set; }
        public DbSet<Flight> Flights { get; set; }
        public DbSet<Pilot> Pilots { get; set; }
        public DbSet<CabinCrew> CabinCrews { get; set; }
        public DbSet<Passenger> Passengers { get; set; }
        public DbSet<Seat> Seats { get; set; }
        public DbSet<FlightCrew> FlightCrews { get; set; }
        public DbSet<FlightCabinCrew> FlightCabinCrews { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Identity tablolarının isimlerini özelleştir
            modelBuilder.Entity<User>().ToTable("Users");
            modelBuilder.Entity<IdentityRole<int>>().ToTable("Roles");
            modelBuilder.Entity<IdentityUserRole<int>>().ToTable("UserRoles");
            modelBuilder.Entity<IdentityUserClaim<int>>().ToTable("UserClaims");
            modelBuilder.Entity<IdentityUserLogin<int>>().ToTable("UserLogins");
            modelBuilder.Entity<IdentityUserToken<int>>().ToTable("UserTokens");
            modelBuilder.Entity<IdentityRoleClaim<int>>().ToTable("RoleClaims");

            // Aircraft Configuration
            modelBuilder.Entity<Aircraft>(entity =>
            {
                entity.HasKey(e => e.AircraftId);
                entity.HasIndex(e => e.RegistrationNumber).IsUnique();
                entity.Property(e => e.AircraftType).IsRequired();
                entity.Property(e => e.MaxRangeKm).HasColumnType("decimal(10,2)");
            });

            // Flight Configuration
            modelBuilder.Entity<Flight>(entity =>
            {
                entity.HasKey(e => e.FlightId);
                entity.HasIndex(e => e.FlightNumber);
                entity.Property(e => e.DistanceKm).HasColumnType("decimal(10,2)");

                entity.HasOne(e => e.Aircraft)
                    .WithMany(a => a.Flights)
                    .HasForeignKey(e => e.AircraftId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Pilot Configuration
            modelBuilder.Entity<Pilot>(entity =>
            {
                entity.HasKey(e => e.PilotId);
                entity.HasIndex(e => e.LicenseNumber).IsUnique();
                entity.Property(e => e.MaxFlightDistanceKm).HasColumnType("decimal(10,2)");

                entity.HasOne(e => e.User)
                    .WithOne()
                    .HasForeignKey<Pilot>(e => e.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // CabinCrew Configuration
            modelBuilder.Entity<CabinCrew>(entity =>
            {
                entity.HasKey(e => e.CabinCrewId);

                entity.HasOne(e => e.User)
                    .WithOne()
                    .HasForeignKey<CabinCrew>(e => e.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Passenger Configuration
            modelBuilder.Entity<Passenger>(entity =>
            {
                entity.HasKey(e => e.PassengerId);
                entity.HasIndex(e => e.PassportNumber).IsUnique();

                entity.HasOne(e => e.User)
                    .WithOne()
                    .HasForeignKey<Passenger>(e => e.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Seat Configuration
            modelBuilder.Entity<Seat>(entity =>
            {
                entity.HasKey(e => e.SeatId);
                entity.HasIndex(e => new { e.FlightId, e.SeatNumber }).IsUnique();

                entity.HasOne(e => e.Flight)
                    .WithMany(f => f.Seats)
                    .HasForeignKey(e => e.FlightId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Passenger)
                    .WithMany(p => p.Seats)
                    .HasForeignKey(e => e.PassengerId)
                    .OnDelete(DeleteBehavior.SetNull);

                entity.HasOne(e => e.ParentPassenger)
                    .WithMany()
                    .HasForeignKey(e => e.ParentPassengerId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // FlightCrew Configuration
            modelBuilder.Entity<FlightCrew>(entity =>
            {
                entity.HasKey(e => e.FlightCrewId);

                entity.HasOne(e => e.Flight)
                    .WithMany(f => f.FlightCrews)
                    .HasForeignKey(e => e.FlightId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Pilot)
                    .WithMany(p => p.FlightCrews)
                    .HasForeignKey(e => e.PilotId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // FlightCabinCrew Configuration
            modelBuilder.Entity<FlightCabinCrew>(entity =>
            {
                entity.HasKey(e => e.FlightCabinCrewId);

                entity.HasOne(e => e.Flight)
                    .WithMany(f => f.FlightCabinCrews)
                    .HasForeignKey(e => e.FlightId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.CabinCrew)
                    .WithMany(c => c.FlightCabinCrews)
                    .HasForeignKey(e => e.CabinCrewId)
                    .OnDelete(DeleteBehavior.Restrict);
            });
        }
    }
}