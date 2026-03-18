using System.Reflection;
using _116.Content.Domain.Constants;
using _116.Content.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace _116.Content.Infrastructure.Persistence;

/// <summary>
/// Entity Framework Core database context for the Content module.
/// Manages all content-related entities under the "content" schema.
/// </summary>
/// <param name="options">The options to configure this database context.</param>
public class ContentDbContext(DbContextOptions<ContentDbContext> options) : DbContext(options)
{
    /// <summary>
    /// Gets the DbSet for content type entities.
    /// </summary>
    public DbSet<ContentTypeEntity> ContentTypes => Set<ContentTypeEntity>();

    /// <summary>
    /// Gets the DbSet for pricing tier entities.
    /// </summary>
    public DbSet<PricingTierEntity> PricingTiers => Set<PricingTierEntity>();

    /// <summary>
    /// Gets the DbSet for promotion level entities.
    /// </summary>
    public DbSet<PromotionLevelEntity> PromotionLevels => Set<PromotionLevelEntity>();

    /// <summary>
    /// Gets the DbSet for tag entities.
    /// </summary>
    public DbSet<TagEntity> Tags => Set<TagEntity>();

    /// <summary>
    /// Gets the DbSet for category entities.
    /// </summary>
    public DbSet<CategoryEntity> Categories => Set<CategoryEntity>();

    /// <summary>
    /// Gets the DbSet for category pricing entities.
    /// </summary>
    public DbSet<CategoryPricingEntity> CategoryPricing => Set<CategoryPricingEntity>();

    /// <summary>
    /// Gets the DbSet for customer entities.
    /// </summary>
    public DbSet<CustomerEntity> Customers => Set<CustomerEntity>();

    /// <summary>
    /// Gets the DbSet for package entities.
    /// </summary>
    public DbSet<PackageEntity> Packages => Set<PackageEntity>();

    /// <summary>
    /// Gets the DbSet for package slot entities.
    /// </summary>
    public DbSet<PackageSlotEntity> PackageSlots => Set<PackageSlotEntity>();

    /// <summary>
    /// Gets the DbSet for article entities.
    /// </summary>
    public DbSet<ArticleEntity> Articles => Set<ArticleEntity>();

    /// <summary>
    /// Gets the DbSet for article image entities (tracks all cover and body images per article).
    /// </summary>
    public DbSet<ArticleImageEntity> ArticleImages => Set<ArticleImageEntity>();

    /// <summary>
    /// Gets the DbSet for article tag junction entities.
    /// </summary>
    public DbSet<ArticleTagEntity> ArticleTags => Set<ArticleTagEntity>();

    /// <summary>
    /// Gets the DbSet for video entities.
    /// </summary>
    public DbSet<VideoEntity> Videos => Set<VideoEntity>();

    /// <summary>
    /// Gets the DbSet for video tag junction entities.
    /// </summary>
    public DbSet<VideoTagEntity> VideoTags => Set<VideoTagEntity>();

    /// <summary>
    /// Gets the DbSet for short video entities.
    /// </summary>
    public DbSet<ShortVideoEntity> ShortVideos => Set<ShortVideoEntity>();

    /// <summary>
    /// Gets the DbSet for lyrics entities.
    /// </summary>
    public DbSet<LyricsEntity> Lyrics => Set<LyricsEntity>();

    /// <summary>
    /// Gets the DbSet for content order entities.
    /// </summary>
    public DbSet<ContentOrderEntity> ContentOrders => Set<ContentOrderEntity>();

    /// <summary>
    /// Gets the DbSet for content order item entities.
    /// </summary>
    public DbSet<ContentOrderItemEntity> ContentOrderItems => Set<ContentOrderItemEntity>();

    /// <summary>
    /// Gets the DbSet for content item tier snapshot entities.
    /// </summary>
    public DbSet<ContentItemTierEntity> ContentItemTiers => Set<ContentItemTierEntity>();

    /// <summary>
    /// Gets the DbSet for content payment entities.
    /// </summary>
    public DbSet<ContentPaymentEntity> ContentPayments => Set<ContentPaymentEntity>();

    /// <inheritdoc />
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(ContentConstants.SchemaName);
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        base.OnModelCreating(modelBuilder);
    }
}
