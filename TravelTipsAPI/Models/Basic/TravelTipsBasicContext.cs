using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace TravelTipsAPI.Models.Basic;

public partial class TravelTipsBasicContext : DbContext
{
    public TravelTipsBasicContext()
    {
    }

    public TravelTipsBasicContext(DbContextOptions<TravelTipsBasicContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Attraction> Attractions { get; set; }

    public virtual DbSet<Day> Days { get; set; }

    public virtual DbSet<Link> Links { get; set; }

    public virtual DbSet<PreferRoute> PreferRoutes { get; set; }

    public virtual DbSet<RouteType> RouteTypes { get; set; }

    public virtual DbSet<SmallTrip> SmallTrips { get; set; }

    public virtual DbSet<Trip> Trips { get; set; }

    public virtual DbSet<TripAttractionOrder> TripAttractionOrders { get; set; }

    public virtual DbSet<User> Users { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.UseSqlServer("Name=ConnectionStrings:TravelTipsBasic");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Attraction>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("pk_attractions");

            entity.ToTable("Attractions", "db_basic");

            entity.HasIndex(e => e.OsmId, "idx_attractions_osmId");

            entity.Property(e => e.Address)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.Description)
                .HasMaxLength(500)
                .IsUnicode(false);
            entity.Property(e => e.Name)
                .HasMaxLength(50)
                .IsUnicode(false);

            entity.HasOne(d => d.CreatedByNavigation).WithMany(p => p.Attractions)
                .HasForeignKey(d => d.CreatedBy)
                .HasConstraintName("fk_users_attractions");

            entity.HasOne(d => d.Link).WithMany(p => p.Attractions)
                .HasForeignKey(d => d.LinkId)
                .HasConstraintName("fk_attractions_links");
        });

        modelBuilder.Entity<Day>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("pk_days");

            entity.ToTable("Days", "db_basic");

            entity.Property(e => e.Description)
                .HasMaxLength(500)
                .IsUnicode(false);
            entity.Property(e => e.Name)
                .HasMaxLength(50)
                .IsUnicode(false);

            entity.HasOne(d => d.Trip).WithMany(p => p.Days)
                .HasForeignKey(d => d.TripId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_trips_days");
        });

        modelBuilder.Entity<Link>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("pk_links");

            entity.ToTable("Links", "db_basic");

            entity.Property(e => e.Name)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Url)
                .HasMaxLength(50)
                .IsUnicode(false);
        });

        modelBuilder.Entity<PreferRoute>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("pk_prefer_routes");

            entity.ToTable("PreferRoutes", "db_basic");

            entity.HasIndex(e => new { e.DepartOsmId, e.ArrivalOsmId }, "idx_prefer_routes_osmId");

            entity.Property(e => e.Ref)
                .HasMaxLength(20)
                .IsUnicode(false);

            entity.HasOne(d => d.CreatedByNavigation).WithMany(p => p.PreferRoutes)
                .HasForeignKey(d => d.CreatedBy)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_users_prefer_routes");

            entity.HasOne(d => d.Link).WithMany(p => p.PreferRoutes)
                .HasForeignKey(d => d.LinkId)
                .HasConstraintName("fk_prefer_routes_links");

            entity.HasOne(d => d.TypeNavigation).WithMany(p => p.PreferRoutes)
                .HasForeignKey(d => d.Type)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_prefer_routes_route_types");
        });

        modelBuilder.Entity<RouteType>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("pk_route_types");

            entity.ToTable("RouteTypes", "db_basic");

            entity.Property(e => e.Name)
                .HasMaxLength(20)
                .IsUnicode(false);
        });

        modelBuilder.Entity<SmallTrip>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("pk_small_trips");

            entity.ToTable("SmallTrips", "db_basic");

            entity.Property(e => e.Description)
                .HasMaxLength(500)
                .IsUnicode(false);
            entity.Property(e => e.Name)
                .HasMaxLength(50)
                .IsUnicode(false);

            entity.HasOne(d => d.Trip).WithMany(p => p.SmallTrips)
                .HasForeignKey(d => d.TripId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_trips_small_trips");
        });

        modelBuilder.Entity<Trip>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("pk_trips");

            entity.ToTable("Trips", "db_basic");

            entity.HasIndex(e => e.IsPublic, "idx_trips_isPublic");

            entity.Property(e => e.CreatedAt).HasColumnType("datetime");
            entity.Property(e => e.Description)
                .HasMaxLength(500)
                .IsUnicode(false);
            entity.Property(e => e.LastUpdatedAt).HasColumnType("datetime");
            entity.Property(e => e.Name)
                .HasMaxLength(50)
                .IsUnicode(false);

            entity.HasOne(d => d.CreatedByNavigation).WithMany(p => p.Trips)
                .HasForeignKey(d => d.CreatedBy)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_users_trips");
        });

        modelBuilder.Entity<TripAttractionOrder>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("pk_trip_attraction_orders");

            entity.ToTable("TripAttractionOrders", "db_basic");

            entity.Property(e => e.IsBikePreferred).HasDefaultValue(true);
            entity.Property(e => e.IsDrivePreferred).HasDefaultValue(true);
            entity.Property(e => e.IsOnFootPreferred).HasDefaultValue(true);

            entity.HasOne(d => d.Attraction).WithMany(p => p.TripAttractionOrders)
                .HasForeignKey(d => d.AttractionId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_trip_attraction_orders_attractions");

            entity.HasOne(d => d.Day).WithMany(p => p.TripAttractionOrders)
                .HasForeignKey(d => d.DayId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_days_trip_attraction_orders");

            entity.HasMany(d => d.PreferRoutes).WithMany(p => p.TripAttractionOrders)
                .UsingEntity<Dictionary<string, object>>(
                    "TripAttractionOrderRoute",
                    r => r.HasOne<PreferRoute>().WithMany()
                        .HasForeignKey("PreferRouteId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("fk_prefer_routes_trip_attraction_order_routes"),
                    l => l.HasOne<TripAttractionOrder>().WithMany()
                        .HasForeignKey("TripAttractionOrderId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("fk_trip_attraction_orders_trip_attraction_order_routes"),
                    j =>
                    {
                        j.HasKey("TripAttractionOrderId", "PreferRouteId").HasName("pk_trip_attraction_order_routes");
                        j.ToTable("TripAttractionOrderRoutes", "db_basic");
                    });
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("pk_users");

            entity.ToTable("Users", "db_basic");

            entity.HasIndex(e => e.UserId, "UQ__Users__1788CC4D6E5216E4").IsUnique();

            entity.Property(e => e.Email)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.UserId)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Username)
                .HasMaxLength(20)
                .IsUnicode(false);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
