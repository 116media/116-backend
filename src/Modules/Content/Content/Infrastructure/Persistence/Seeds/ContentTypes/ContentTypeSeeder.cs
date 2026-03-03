using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;
using _116.Shared.Infrastructure.Seed;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace _116.Content.Infrastructure.Persistence.Seeds.ContentTypes;

/// <summary>
/// Seeder responsible for creating the core content types: Article, Video, and Short.
/// </summary>
/// <remarks>
/// These are structural constants required by the entire content system.
/// Every category and content item must belong to one of these types.
/// This seeder is idempotent — it skips execution if any content types already exist.
/// </remarks>
public class ContentTypeSeeder(ContentDbContext context, ILogger<ContentTypeSeeder> logger) : IDataSeeder
{
    private static readonly string[] ContentTypeNames =
    [
        nameof(EnumCoreContentType.Article),
        nameof(EnumCoreContentType.Video),
        nameof(EnumCoreContentType.Short),
    ];

    /// <inheritdoc />
    public async Task SeedAllAsync()
    {
        try
        {
            logger.LogInformation("Starting content type seeding process...");

            bool alreadySeeded = await context.ContentTypes.AnyAsync();
            if (alreadySeeded)
            {
                logger.LogInformation("Content types already exist. Skipping seeding.");
                return;
            }

            await ExecuteSeedingAsync();
            logger.LogInformation("Content type seeding completed successfully!");
        }
        catch (Exception ex)
        {
            logger.LogError(exception: ex, "Failed to seed content type data");
            throw;
        }
    }

    /// <summary>
    /// Creates the Article, Video, and Short content types.
    /// </summary>
    private async Task ExecuteSeedingAsync()
    {
        ContentTypeEntity[] contentTypes = ContentTypeNames
            .Select(name => ContentTypeEntity.Create(id: Guid.NewGuid(), name: name))
            .ToArray();

        await context.ContentTypes.AddRangeAsync(contentTypes);
        await context.SaveChangesAsync();

        logger.LogInformation(
            "Created {Count} content types: {Names}",
            contentTypes.Length,
            string.Join(", ", ContentTypeNames)
        );
    }
}
