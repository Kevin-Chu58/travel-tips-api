using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace TravelTipsAPI.Models.TravelTipsModels;

public partial class TravelTipsContext : DbContext
{
    public TravelTipsContext() { }

    public TravelTipsContext(DbContextOptions<TravelTipsContext> options)
        : base(options) { }

    public virtual DbSet<Admin> Admins { get; set; }

    public virtual DbSet<Attraction> Attractions { get; set; }

    public virtual DbSet<Day> Days { get; set; }

    public virtual DbSet<Highlight> Highlights { get; set; }

    public virtual DbSet<Image> Images { get; set; }

    public virtual DbSet<Trip> Trips { get; set; }

    public virtual DbSet<TripAttractionOrder> TripAttractionOrders { get; set; }

    public virtual DbSet<TripImage> TripImages { get; set; }

    public virtual DbSet<User> Users { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) =>
        optionsBuilder.UseSqlServer("Name=ConnectionStrings:TravelTips");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Admin>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("pk_admins");

            entity.ToTable("Admins", "db_role");

            entity.Property(e => e.UserId).ValueGeneratedNever();

            entity
                .HasOne(d => d.User)
                .WithOne(p => p.Admin)
                .HasForeignKey<Admin>(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_users_admins");
        });

        modelBuilder.Entity<Attraction>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("pk_attractions");

            entity.ToTable("Attractions", "db_basic");

            entity.HasIndex(e => e.Category, "idx_attractions_Category");

            entity.HasIndex(e => e.IsDeprecated, "idx_attractions_IsDeprecated");

            entity.HasIndex(e => e.Lat, "idx_attractions_Lat");

            entity.HasIndex(e => e.Lng, "idx_attractions_Lng");

            entity.HasIndex(
                e => new
                {
                    e.City,
                    e.State,
                    e.Country,
                },
                "idx_attractions_Location"
            );

            entity.Property(e => e.Address).HasMaxLength(1000);
            entity.Property(e => e.Category).HasMaxLength(50);
            entity.Property(e => e.City).HasMaxLength(100);
            entity.Property(e => e.Country).HasMaxLength(60);
            entity.Property(e => e.HereId).IsUnicode(false);
            entity.Property(e => e.Lat).HasColumnType("decimal(10, 7)");
            entity.Property(e => e.Lng).HasColumnType("decimal(10, 7)");
            entity.Property(e => e.ResultType).HasMaxLength(20).IsUnicode(false);
            entity.Property(e => e.State).HasMaxLength(100);
            entity.Property(e => e.Title).HasMaxLength(500);
        });

        modelBuilder.Entity<Day>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("pk_days");

            entity.ToTable("Days", "db_basic");

            entity.Property(e => e.Title).HasMaxLength(50).IsUnicode(false);

            entity
                .HasOne(d => d.CreatedByNavigation)
                .WithMany(p => p.Days)
                .HasForeignKey(d => d.CreatedBy)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_users_days");

            entity
                .HasOne(d => d.Trip)
                .WithMany(p => p.Days)
                .HasForeignKey(d => d.TripId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_trips_days");
        });

        modelBuilder.Entity<Highlight>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("pk_highlights");

            entity.ToTable("Highlights", "db_basic");

            entity
                .HasOne(d => d.Attraction)
                .WithMany(p => p.Highlights)
                .HasForeignKey(d => d.AttractionId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_attractions_highlights");

            entity
                .HasOne(d => d.CreatedByNavigation)
                .WithMany(p => p.Highlights)
                .HasForeignKey(d => d.CreatedBy)
                .HasConstraintName("fk_users_highlights");
        });

        modelBuilder.Entity<Image>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("pk_images");

            entity.ToTable("Images", "db_image");

            entity.Property(e => e.Name).HasMaxLength(50).IsUnicode(false);

            entity
                .HasOne(d => d.CreatedByNavigation)
                .WithMany(p => p.Images)
                .HasForeignKey(d => d.CreatedBy)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_users_images");
        });

        modelBuilder.Entity<Trip>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("pk_trips");

            entity.ToTable("Trips", "db_basic");

            entity.HasIndex(e => e.IsHidden, "idx_trips_isHidden");

            entity.HasIndex(e => e.IsPublic, "idx_trips_isPublic");

            entity.Property(e => e.CreatedAt).HasColumnType("datetime");
            entity.Property(e => e.Title).HasMaxLength(50).IsUnicode(false);

            entity
                .HasOne(d => d.CreatedByNavigation)
                .WithMany(p => p.Trips)
                .HasForeignKey(d => d.CreatedBy)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_users_trips");
        });

        modelBuilder.Entity<TripAttractionOrder>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("pk_trip_attraction_orders");

            entity.ToTable("TripAttractionOrders", "db_basic");

            entity.HasIndex(e => e.HighlightId, "idx_tao_highlight_id");

            entity.Property(e => e.TransportMode).HasMaxLength(20).IsUnicode(false);

            entity
                .HasOne(d => d.Attraction)
                .WithMany(p => p.TripAttractionOrders)
                .HasForeignKey(d => d.AttractionId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_trip_attraction_orders_attractions");

            entity
                .HasOne(d => d.CreatedByNavigation)
                .WithMany(p => p.TripAttractionOrders)
                .HasForeignKey(d => d.CreatedBy)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_users_trip_attraction_orders");

            entity
                .HasOne(d => d.Day)
                .WithMany(p => p.TripAttractionOrders)
                .HasForeignKey(d => d.DayId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_days_trip_attraction_orders");

            entity
                .HasOne(d => d.Highlight)
                .WithMany(p => p.TripAttractionOrders)
                .HasForeignKey(d => d.HighlightId)
                .HasConstraintName("fk_trip_attraction_orders_highlights");
        });

        modelBuilder.Entity<TripImage>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("pk_trip_images");

            entity.ToTable("TripImages", "db_image");

            entity
                .HasOne(d => d.Image)
                .WithMany(p => p.TripImages)
                .HasForeignKey(d => d.ImageId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_images_trip_images");

            entity
                .HasOne(d => d.Trip)
                .WithMany(p => p.TripImages)
                .HasForeignKey(d => d.TripId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_trips_trip_images");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("pk_users");

            entity.ToTable("Users", "db_basic");

            entity.HasIndex(e => e.UserId, "UQ__Users__1788CC4D70D79A34").IsUnique();

            entity.Property(e => e.Email).HasMaxLength(50).IsUnicode(false);
            entity.Property(e => e.UserId).HasMaxLength(50).IsUnicode(false);
            entity.Property(e => e.Username).HasMaxLength(50).IsUnicode(false);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
