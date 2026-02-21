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

    public virtual DbSet<Bookmark> Bookmarks { get; set; }

    public virtual DbSet<Day> Days { get; set; }

    public virtual DbSet<Follower> Followers { get; set; }

    public virtual DbSet<Highlight> Highlights { get; set; }

    public virtual DbSet<Image> Images { get; set; }

    public virtual DbSet<Region> Regions { get; set; }

    public virtual DbSet<Sermon> Sermons { get; set; }

    public virtual DbSet<SermonLabel> SermonLabels { get; set; }

    public virtual DbSet<Trip> Trips { get; set; }

    public virtual DbSet<TripAttractionOrder> TripAttractionOrders { get; set; }

    public virtual DbSet<TripImage> TripImages { get; set; }

    public virtual DbSet<TripShare> TripShares { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<Writer> Writers { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) =>
        optionsBuilder.UseSqlServer("Name=ConnectionStrings:TravelTips");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Admin>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("pk_admins");

            entity.ToTable("Admins", "db_role");

            entity.HasIndex(e => e.UserId, "idx_admin_user_id");

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

        modelBuilder.Entity<Bookmark>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("pk_bookmarks");

            entity.ToTable("Bookmarks", "db_search");

            entity.HasIndex(e => e.TripId, "idx_bookmarks_trip_id");

            entity.HasIndex(e => e.UserId, "idx_bookmarks_user_id");

            entity.HasIndex(e => new { e.UserId, e.TripId }, "idx_bookmarks_user_id_trip_id");

            entity.HasIndex(e => new { e.UserId, e.TripId }, "ux_user_id_trip_id").IsUnique();

            entity
                .HasOne(d => d.Trip)
                .WithMany(p => p.Bookmarks)
                .HasForeignKey(d => d.TripId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_trips_bookmarks");

            entity
                .HasOne(d => d.User)
                .WithMany(p => p.Bookmarks)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_users_bookmarks");
        });

        modelBuilder.Entity<Day>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("pk_days");

            entity.ToTable("Days", "db_basic");

            entity.HasIndex(e => e.TripId, "idx_day_trip_id");

            entity.HasIndex(e => e.CreatedBy, "idx_day_user_id");

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

        modelBuilder.Entity<Follower>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("pk_followers");

            entity.ToTable("Followers", "db_search");

            entity.HasIndex(e => e.Followed, "idx_followers_followed");

            entity.HasIndex(
                e => new { e.Followed, e.Following },
                "idx_followers_followed_following"
            );

            entity.HasIndex(e => e.Following, "idx_followers_following");

            entity
                .HasIndex(e => new { e.Followed, e.Following }, "ux_followers_followed_following")
                .IsUnique();

            entity
                .HasOne(d => d.FollowedNavigation)
                .WithMany(p => p.FollowerFollowedNavigations)
                .HasForeignKey(d => d.Followed)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_users_followers_followed");

            entity
                .HasOne(d => d.FollowingNavigation)
                .WithMany(p => p.FollowerFollowingNavigations)
                .HasForeignKey(d => d.Following)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_users_followers_following");
        });

        modelBuilder.Entity<Highlight>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("pk_highlights");

            entity.ToTable("Highlights", "db_basic");

            entity.HasIndex(e => e.AttractionId, "idx_highlight_attraction_id");

            entity.HasIndex(e => e.CreatedBy, "idx_highlight_user_id");

            entity
                .HasIndex(e => new { e.UsageCount, e.Id }, "idx_highlights_usageCount_id")
                .IsDescending();

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
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_users_highlights");
        });

        modelBuilder.Entity<Image>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("pk_images");

            entity.ToTable("Images", "db_image");

            entity.HasIndex(e => e.CreatedBy, "idx_image_user_id");

            entity.Property(e => e.Name).HasMaxLength(50).IsUnicode(false);

            entity
                .HasOne(d => d.CreatedByNavigation)
                .WithMany(p => p.Images)
                .HasForeignKey(d => d.CreatedBy)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_users_images");
        });

        modelBuilder.Entity<Region>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("pk_regions");

            entity.ToTable("Regions", "db_search");

            entity.HasIndex(e => e.Name, "idx_regions_name");

            entity.HasIndex(e => e.ParentRegionId, "idx_regions_parent_region_id");

            entity.HasIndex(e => e.Type, "idx_regions_type");

            entity.Property(e => e.Name).HasMaxLength(100);
            entity.Property(e => e.Slug).HasMaxLength(100);
            entity.Property(e => e.Type).HasMaxLength(10).IsUnicode(false);

            entity
                .HasOne(d => d.ParentRegion)
                .WithMany(p => p.InverseParentRegion)
                .HasForeignKey(d => d.ParentRegionId)
                .HasConstraintName("fk_regions_parent");
        });

        modelBuilder.Entity<Sermon>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("pk_sermons");

            entity.ToTable("Sermons", "db_gospel");

            entity
                .HasIndex(e => new { e.PublishAt, e.IsBanner }, "idx_sermons_banner_filter")
                .IsDescending();

            entity.HasIndex(
                e => new
                {
                    e.LabelId,
                    e.Title,
                    e.CreatedBy,
                },
                "idx_sermons_filter"
            );

            entity.Property(e => e.Title).HasMaxLength(50);

            entity
                .HasOne(d => d.CreatedByNavigation)
                .WithMany(p => p.Sermons)
                .HasForeignKey(d => d.CreatedBy)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_users_sermons");

            entity
                .HasOne(d => d.Label)
                .WithMany(p => p.Sermons)
                .HasForeignKey(d => d.LabelId)
                .HasConstraintName("fk_sermons_sermon_labels");
        });

        modelBuilder.Entity<SermonLabel>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("pk_sermon_labels");

            entity.ToTable("SermonLabels", "db_gospel");

            entity.HasIndex(
                e => new
                {
                    e.Type,
                    e.Slug,
                    e.Name,
                },
                "idx_sermon_labels_filter"
            );

            entity.Property(e => e.Name).HasMaxLength(100);
            entity.Property(e => e.Slug).HasMaxLength(100);
            entity.Property(e => e.Type).HasMaxLength(10).IsUnicode(false);

            entity
                .HasOne(d => d.ParentLabel)
                .WithMany(p => p.InverseParentLabel)
                .HasForeignKey(d => d.ParentLabelId)
                .HasConstraintName("fk_sermon_labels_parent");
        });

        modelBuilder.Entity<Trip>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("pk_trips");

            entity.ToTable("Trips", "db_basic");

            entity
                .HasIndex(e => new { e.BookmarkCount, e.Id }, "idx_trips_bookmarkCount_id")
                .IsDescending();

            entity
                .HasIndex(e => new { e.CreatedAt, e.Id }, "idx_trips_createdAt_id")
                .IsDescending();

            entity.HasIndex(
                e => new
                {
                    e.RegionId,
                    e.Budget,
                    e.IsPublic,
                    e.IsHidden,
                },
                "idx_trips_filter"
            );

            entity.Property(e => e.CreatedAt).HasColumnType("datetime");
            entity.Property(e => e.Title).HasMaxLength(50);

            entity
                .HasOne(d => d.CreatedByNavigation)
                .WithMany(p => p.Trips)
                .HasForeignKey(d => d.CreatedBy)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_users_trips");

            entity
                .HasOne(d => d.Region)
                .WithMany(p => p.Trips)
                .HasForeignKey(d => d.RegionId)
                .HasConstraintName("fk_regions_trips");
        });

        modelBuilder.Entity<TripAttractionOrder>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("pk_trip_attraction_orders");

            entity.ToTable("TripAttractionOrders", "db_basic");

            entity.HasIndex(e => e.AttractionId, "idx_tao_attraction_id");

            entity.HasIndex(e => e.DayId, "idx_tao_day_id");

            entity.HasIndex(e => e.HighlightId, "idx_tao_highlight_id");

            entity.HasIndex(e => e.IsPrivate, "idx_tao_isPrivate");

            entity.HasIndex(e => e.CreatedBy, "idx_tao_user_id");

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

            entity.HasIndex(e => e.ImageId, "idx_trip_image_image_id");

            entity.HasIndex(e => e.TripId, "idx_trip_image_trip_id");

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

        modelBuilder.Entity<TripShare>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("pk_trip_shares");

            entity.ToTable("TripShares", "db_basic");

            entity.HasIndex(e => e.TripId, "idx_trip_shares_trip_id");

            entity.HasIndex(e => e.ShareWith, "idx_trip_shares_user_id");

            entity
                .HasOne(d => d.ShareWithNavigation)
                .WithMany(p => p.TripShares)
                .HasForeignKey(d => d.ShareWith)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_trip_shares_user");

            entity
                .HasOne(d => d.Trip)
                .WithMany(p => p.TripShares)
                .HasForeignKey(d => d.TripId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_trip_shares_trip");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("pk_users");

            entity.ToTable("Users", "db_basic");

            entity.HasIndex(e => e.UserId, "UQ__Users__1788CC4D7D8D0D21").IsUnique();

            entity
                .HasIndex(e => new { e.FollowerCount, e.Id }, "idx_users_followerCount_id")
                .IsDescending();

            entity
                .HasIndex(e => new { e.FollowingCount, e.Id }, "idx_users_followingCount_id")
                .IsDescending();

            entity.HasIndex(e => new { e.Username, e.Id }, "idx_users_username_id");

            entity.Property(e => e.Email).HasMaxLength(50).IsUnicode(false);
            entity.Property(e => e.ExternalImageUrl).IsUnicode(false);
            entity.Property(e => e.UserId).HasMaxLength(50).IsUnicode(false);
            entity.Property(e => e.Username).HasMaxLength(50);

            entity
                .HasOne(d => d.Image)
                .WithMany(p => p.Users)
                .HasForeignKey(d => d.ImageId)
                .HasConstraintName("fk_images_users");
        });

        modelBuilder.Entity<Writer>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("pk_writers");

            entity.ToTable("Writers", "db_role");

            entity.HasIndex(e => e.UserId, "idx_writer_user_id");

            entity.Property(e => e.UserId).ValueGeneratedNever();

            entity
                .HasOne(d => d.User)
                .WithOne(p => p.Writer)
                .HasForeignKey<Writer>(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_users_writers");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
