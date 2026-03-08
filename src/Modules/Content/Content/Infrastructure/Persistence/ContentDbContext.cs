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
    /// <summary>Gets the DbSet for content type entities.</summary>
    public DbSet<ContentTypeEntity> ContentTypes => Set<ContentTypeEntity>();

    /// <summary>Gets the DbSet for pricing tier entities.</summary>
    public DbSet<PricingTierEntity> PricingTiers => Set<PricingTierEntity>();

    /// <summary>Gets the DbSet for promotion level entities.</summary>
    public DbSet<PromotionLevelEntity> PromotionLevels => Set<PromotionLevelEntity>();

    /// <summary>Gets the DbSet for tag entities.</summary>
    public DbSet<TagEntity> Tags => Set<TagEntity>();

    /// <inheritdoc />
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(ContentConstants.SchemaName);
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        base.OnModelCreating(modelBuilder);
    }
}
