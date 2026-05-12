using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace TravelTipsAPI.Models.TravelTipsModels;

public partial class TravelTipsContext : DbContext
{
    public TravelTipsContext() { }

    public TravelTipsContext(DbContextOptions<TravelTipsContext> options)
        : base(options) { }

    public virtual DbSet<Ad> Ads { get; set; }

    public virtual DbSet<AdSubLog> AdSubLogs { get; set; }

    public virtual DbSet<AdTarget> AdTargets { get; set; }

    public virtual DbSet<Admin> Admins { get; set; }

    public virtual DbSet<Attraction> Attractions { get; set; }

    public virtual DbSet<Banner> Banners { get; set; }

    public virtual DbSet<BannerMan> BannerMen { get; set; }

    public virtual DbSet<BannerStyling> BannerStylings { get; set; }

    public virtual DbSet<Bookmark> Bookmarks { get; set; }

    public virtual DbSet<Business> Businesses { get; set; }

    public virtual DbSet<Day> Days { get; set; }

    public virtual DbSet<Follower> Followers { get; set; }

    public virtual DbSet<Highlight> Highlights { get; set; }

    public virtual DbSet<Image> Images { get; set; }

    public virtual DbSet<ProcessedStripeEvent> ProcessedStripeEvents { get; set; }

    public virtual DbSet<Region> Regions { get; set; }

    public virtual DbSet<Reviewer> Reviewers { get; set; }

    public virtual DbSet<Subscription> Subscriptions { get; set; }

    public virtual DbSet<SubscriptionPlan> SubscriptionPlans { get; set; }

    public virtual DbSet<TargetRule> TargetRules { get; set; }

    public virtual DbSet<Trip> Trips { get; set; }

    public virtual DbSet<TripAttractionOrder> TripAttractionOrders { get; set; }

    public virtual DbSet<TripImage> TripImages { get; set; }

    public virtual DbSet<TripShare> TripShares { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<UserSubExtend> UserSubExtends { get; set; }

    public virtual DbSet<Writer> Writers { get; set; }

    public virtual DbSet<Writing> Writings { get; set; }

    public virtual DbSet<WritingLabel> WritingLabels { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) =>
        optionsBuilder.UseSqlServer("Name=ConnectionStrings:TravelTipsLocal");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Ad>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("pk_ads");

            entity.ToTable("Ads", "db_feed");

            entity.HasIndex(e => new { e.SubStatus, e.Status }, "idx_ad_filter");

            entity.HasIndex(e => new { e.CreatedBy, e.BusinessId }, "idx_ad_owner");

            entity.Property(e => e.Link).HasMaxLength(255).IsUnicode(false);
            entity.Property(e => e.LinkLabel).HasMaxLength(50);
            entity.Property(e => e.RenewSub).HasDefaultValue(true);
            entity.Property(e => e.Status).HasMaxLength(20).IsUnicode(false);
            entity.Property(e => e.StripeItemId).HasMaxLength(255).IsUnicode(false);
            entity.Property(e => e.StripeSubscriptionId).HasMaxLength(255).IsUnicode(false);
            entity.Property(e => e.SubStatus).HasMaxLength(10).IsUnicode(false);
            entity.Property(e => e.Text).HasMaxLength(100);
            entity.Property(e => e.Title).HasMaxLength(50);

            entity
                .HasOne(d => d.Business)
                .WithMany(p => p.Ads)
                .HasForeignKey(d => d.BusinessId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_businesses_ads");

            entity
                .HasOne(d => d.CreatedByNavigation)
                .WithMany(p => p.Ads)
                .HasForeignKey(d => d.CreatedBy)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_users_ads");

            entity
                .HasOne(d => d.Image)
                .WithMany(p => p.Ads)
                .HasForeignKey(d => d.ImageId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_images_ads");
        });

        modelBuilder.Entity<AdSubLog>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("pk_ad_sub_logs");

            entity.ToTable("AdSubLogs", "db_feed");

            entity
                .HasIndex(e => new { e.AdId, e.Id }, "idx_latest_ad_sub_log_by_adId")
                .IsDescending(false, true);

            entity.Property(e => e.Note).HasMaxLength(200).IsUnicode(false);

            entity
                .HasOne(d => d.Ad)
                .WithMany(p => p.AdSubLogs)
                .HasForeignKey(d => d.AdId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_ads_ad_sub_logs");
        });

        modelBuilder.Entity<AdTarget>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("pk_ad_targets");

            entity.ToTable("AdTargets", "db_feed");

            entity.HasIndex(e => e.AdId, "idx_ad_target_ad");

            entity.HasIndex(e => new { e.TargetType, e.TargetValue }, "idx_ad_target_search");

            entity.Property(e => e.TargetType).HasMaxLength(10).IsUnicode(false);
            entity.Property(e => e.TargetValue).HasMaxLength(50);

            entity
                .HasOne(d => d.Ad)
                .WithMany(p => p.AdTargets)
                .HasForeignKey(d => d.AdId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_ads_ad_targets");
        });

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

        modelBuilder.Entity<Banner>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("pk_banners");

            entity.ToTable("Banners", "db_feed");

            entity.HasIndex(e => new { e.To, e.From }, "idx_banner_publish_period").IsDescending();

            entity.Property(e => e.Label).HasMaxLength(100);
            entity.Property(e => e.Link).HasMaxLength(100);
            entity.Property(e => e.Overview).HasMaxLength(300);
            entity.Property(e => e.SubLabel).HasMaxLength(100);
            entity.Property(e => e.Title).HasMaxLength(100);

            entity
                .HasOne(d => d.Image)
                .WithMany(p => p.Banners)
                .HasForeignKey(d => d.ImageId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_banners_images");

            entity
                .HasOne(d => d.Styling)
                .WithMany(p => p.Banners)
                .HasForeignKey(d => d.StylingId)
                .HasConstraintName("fk_banners_banner_stylings");
        });

        modelBuilder.Entity<BannerMan>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("pk_banner_men");

            entity.ToTable("BannerMen", "db_role");

            entity.HasIndex(e => e.UserId, "idx_banner_men_user_id");

            entity.Property(e => e.UserId).ValueGeneratedNever();

            entity
                .HasOne(d => d.User)
                .WithOne(p => p.BannerMan)
                .HasForeignKey<BannerMan>(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_users_banner_men");
        });

        modelBuilder.Entity<BannerStyling>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("pk_banner_stylings");

            entity.ToTable("BannerStylings", "db_feed");

            entity.Property(e => e.Name).HasMaxLength(100);
        });

        modelBuilder.Entity<Bookmark>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("pk_bookmarks");

            entity.ToTable("Bookmarks", "db_search");

            entity.HasIndex(e => e.TripId, "idx_bookmarks_trip_id");

            entity.HasIndex(e => e.UserId, "idx_bookmarks_user_id");

            entity
                .HasIndex(e => new { e.UserId, e.TripId }, "idx_bookmarks_user_id_trip_id")
                .IsDescending(false, true);

            entity.HasIndex(e => new { e.UserId, e.TripId }, "ux_user_id_trip_id").IsUnique();

            entity
                .HasOne(d => d.Trip)
                .WithMany(p => p.Bookmarks)
                .HasForeignKey(d => d.TripId)
                .HasConstraintName("fk_trips_bookmarks");

            entity
                .HasOne(d => d.User)
                .WithMany(p => p.Bookmarks)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_users_bookmarks");
        });

        modelBuilder.Entity<Business>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("pk_businesses");

            entity.ToTable("Businesses", "db_feed");

            entity.HasIndex(e => new { e.CreatedBy, e.Status }, "idx_business_status");

            entity.Property(e => e.Address).HasMaxLength(200);
            entity.Property(e => e.Name).HasMaxLength(100);
            entity.Property(e => e.Status).HasMaxLength(10).IsUnicode(false);
            entity.Property(e => e.Website).HasMaxLength(100).IsUnicode(false);

            entity
                .HasOne(d => d.CreatedByNavigation)
                .WithMany(p => p.Businesses)
                .HasForeignKey(d => d.CreatedBy)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_users_businesses");

            entity
                .HasOne(d => d.Image)
                .WithMany(p => p.Businesses)
                .HasForeignKey(d => d.ImageId)
                .HasConstraintName("fk_images_businesses");
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

            entity
                .HasIndex(
                    e => new
                    {
                        e.CreatedBy,
                        e.Type,
                        e.Id,
                    },
                    "idx_image_type"
                )
                .IsDescending(false, false, true);

            entity.HasIndex(e => e.CreatedBy, "idx_image_user_id");

            entity.Property(e => e.Name).HasMaxLength(50).IsUnicode(false);
            entity.Property(e => e.Type).HasMaxLength(10).IsUnicode(false);

            entity
                .HasOne(d => d.CreatedByNavigation)
                .WithMany(p => p.Images)
                .HasForeignKey(d => d.CreatedBy)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_users_images");
        });

        modelBuilder.Entity<ProcessedStripeEvent>(entity =>
        {
            entity.HasKey(e => e.StripeEventId).HasName("pk_processed_stripe_events");

            entity.ToTable("ProcessedStripeEvents", "db_record");

            entity.Property(e => e.StripeEventId).HasMaxLength(255).IsUnicode(false);
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

        modelBuilder.Entity<Reviewer>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("pk_reviewers");

            entity.ToTable("Reviewers", "db_role");

            entity.HasIndex(e => e.UserId, "idx_reviewers_user_id");

            entity.Property(e => e.UserId).ValueGeneratedNever();

            entity
                .HasOne(d => d.User)
                .WithOne(p => p.Reviewer)
                .HasForeignKey<Reviewer>(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_users_reviewers");
        });

        modelBuilder.Entity<Subscription>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("pk_subscriptions");

            entity.ToTable("Subscriptions", "db_plan");

            entity.HasIndex(e => e.StripeSubscriptionId, "idx_subscription_stripe_sub_id");

            entity.HasIndex(e => new { e.UserId, e.Status }, "idx_subscription_user_active");

            entity
                .HasIndex(
                    e => new
                    {
                        e.UserId,
                        e.Start,
                        e.End,
                    },
                    "idx_subscription_user_record"
                )
                .IsDescending(false, true, true);

            entity
                .HasIndex(e => new { e.UserId, e.PlanId }, "uidx_subscription_only_active")
                .IsUnique()
                .HasFilter("([Status]='active')");

            entity.Property(e => e.Status).HasMaxLength(10).IsUnicode(false);
            entity.Property(e => e.StripeSubscriptionId).HasMaxLength(255).IsUnicode(false);

            entity
                .HasOne(d => d.Plan)
                .WithMany(p => p.Subscriptions)
                .HasForeignKey(d => d.PlanId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_subscriptions_subscription_plans");

            entity
                .HasOne(d => d.User)
                .WithMany(p => p.Subscriptions)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_users_subscriptions");
        });

        modelBuilder.Entity<SubscriptionPlan>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("pk_subscription_plans");

            entity.ToTable("SubscriptionPlans", "db_plan");

            entity.Property(e => e.Description).HasMaxLength(100).IsUnicode(false);
        });

        modelBuilder.Entity<TargetRule>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("pk_target_rules");

            entity.ToTable("TargetRules", "db_feed");

            entity.HasIndex(e => new { e.TargetType, e.TargetValue }, "idx_target_rules_filter");

            entity.Property(e => e.TargetType).HasMaxLength(10).IsUnicode(false);
            entity.Property(e => e.TargetValue).HasMaxLength(50);
        });

        modelBuilder.Entity<Trip>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("pk_trips");

            entity.ToTable("Trips", "db_basic");

            entity
                .HasIndex(
                    e => new
                    {
                        e.IsPublic,
                        e.IsHidden,
                        e.RegionId,
                        e.CreatedAt,
                        e.Id,
                    },
                    "idx_trips_filter_chronological"
                )
                .IsDescending(false, false, false, true, true);

            entity
                .HasIndex(
                    e => new
                    {
                        e.IsPublic,
                        e.IsHidden,
                        e.RegionId,
                        e.BookmarkCount,
                        e.Id,
                    },
                    "idx_trips_filter_popularity"
                )
                .IsDescending(false, false, false, true, true);

            entity
                .HasIndex(
                    e => new
                    {
                        e.CreatedBy,
                        e.CreatedAt,
                        e.Id,
                    },
                    "idx_trips_user_most_recent"
                )
                .IsDescending(false, true, true);

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
                .HasConstraintName("fk_images_trip_images");

            entity
                .HasOne(d => d.Trip)
                .WithMany(p => p.TripImages)
                .HasForeignKey(d => d.TripId)
                .HasConstraintName("fk_trips_trip_images");
        });

        modelBuilder.Entity<TripShare>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("pk_trip_shares");

            entity.ToTable("TripShares", "db_basic");

            entity
                .HasIndex(e => new { e.ShareWith, e.TripId }, "idx_trip_shares_cursor")
                .IsDescending(false, true);

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
                .HasConstraintName("fk_trip_shares_trip");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("pk_users");

            entity.ToTable("Users", "db_basic");

            entity.HasIndex(e => e.UserId, "UQ__Users__1788CC4D8D5A6A53").IsUnique();

            entity
                .HasIndex(e => new { e.FollowerCount, e.Id }, "idx_users_followerCount_id")
                .IsDescending();

            entity
                .HasIndex(e => new { e.FollowingCount, e.Id }, "idx_users_followingCount_id")
                .IsDescending();

            entity.HasIndex(e => e.StripeCustomerId, "idx_users_stripe_customer_id");

            entity.HasIndex(e => new { e.Username, e.Id }, "idx_users_username_id");

            entity.Property(e => e.Email).HasMaxLength(50).IsUnicode(false);
            entity.Property(e => e.ExternalImageUrl).IsUnicode(false);
            entity.Property(e => e.StripeCurrency).HasMaxLength(3).IsUnicode(false).IsFixedLength();
            entity.Property(e => e.StripeCustomerId).HasMaxLength(255).IsUnicode(false);
            entity.Property(e => e.UserId).HasMaxLength(50).IsUnicode(false);
            entity.Property(e => e.Username).HasMaxLength(50);

            entity
                .HasOne(d => d.Image)
                .WithMany(p => p.Users)
                .HasForeignKey(d => d.ImageId)
                .HasConstraintName("fk_images_users");
        });

        modelBuilder.Entity<UserSubExtend>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("pk_user_sub_extends");

            entity.ToTable("UserSubExtends", "db_basic");

            entity.Property(e => e.UserId).ValueGeneratedNever();
            entity.Property(e => e.MaxTripCount).HasDefaultValue(3);

            entity
                .HasOne(d => d.User)
                .WithOne(p => p.UserSubExtend)
                .HasForeignKey<UserSubExtend>(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_users_user_sub_extends");
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

        modelBuilder.Entity<Writing>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("pk_writings");

            entity.ToTable("Writings", "db_gospel");

            entity.HasIndex(
                e => new
                {
                    e.LabelId,
                    e.Title,
                    e.CreatedBy,
                },
                "idx_writings_filter"
            );

            entity.HasIndex(e => e.PublishAt, "idx_writings_publishAt_filter").IsDescending();

            entity.Property(e => e.Title).HasMaxLength(50);

            entity
                .HasOne(d => d.CreatedByNavigation)
                .WithMany(p => p.Writings)
                .HasForeignKey(d => d.CreatedBy)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_users_writings");

            entity
                .HasOne(d => d.Label)
                .WithMany(p => p.Writings)
                .HasForeignKey(d => d.LabelId)
                .HasConstraintName("fk_writings_writing_labels");
        });

        modelBuilder.Entity<WritingLabel>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("pk_writing_labels");

            entity.ToTable("WritingLabels", "db_gospel");

            entity.HasIndex(
                e => new
                {
                    e.Type,
                    e.Slug,
                    e.Name,
                },
                "idx_writing_labels_filter"
            );

            entity.Property(e => e.Name).HasMaxLength(100);
            entity.Property(e => e.Slug).HasMaxLength(100);
            entity.Property(e => e.Type).HasMaxLength(10).IsUnicode(false);

            entity
                .HasOne(d => d.ParentLabel)
                .WithMany(p => p.InverseParentLabel)
                .HasForeignKey(d => d.ParentLabelId)
                .HasConstraintName("fk_writing_labels_parent");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
