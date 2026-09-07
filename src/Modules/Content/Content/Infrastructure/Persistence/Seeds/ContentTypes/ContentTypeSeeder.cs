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
/// Idempotency is per row, so a type added to the list later is seeded into databases that
/// were first seeded before it existed.
/// </remarks>
public class ContentTypeSeeder(ContentDbContext context, ILogger<ContentTypeSeeder> logger) : IDataSeeder
{
    private static readonly string[] ContentTypeNames =
    [
        nameof(EnumCoreContentType.Article),
        nameof(EnumCoreContentType.Video),
        nameof(EnumCoreContentType.Short),
        nameof(EnumCoreContentType.Lyrics),
    ];

    /// <inheritdoc />
    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        List<string> existing = await context
            .ContentTypes.Where(t => ContentTypeNames.Contains(t.Name))
            .Select(t => t.Name)
            .ToListAsync(cancellationToken);

        string[] missing = ContentTypeNames.Except(existing).ToArray();

        if (missing.Length == 0)
        {
            return;
        }

        foreach (string name in missing)
        {
            context.ContentTypes.Add(ContentTypeEntity.Create(id: Guid.NewGuid(), name: name));
        }

        await context.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Seeded {Count} content types: {Names}", missing.Length, string.Join(", ", missing));
    }
}
