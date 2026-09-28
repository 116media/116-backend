using _116.Content.Infrastructure.Persistence;
using _116.Tests.TestData.Helpers;
using Microsoft.EntityFrameworkCore;

namespace _116.Content.TestData.Factories.Helpers;

/// <summary>
/// Builds isolated in-memory <see cref="ContentDbContext" /> instances for repository unit tests,
/// carrying the audit interceptor the application registers.
/// </summary>
public static class ContentDbContextFactory
{
    /// <summary>
    /// Creates a context over its own in-memory store.
    /// </summary>
    /// <returns>A new ContentDbContext the caller owns and disposes.</returns>
    public static ContentDbContext CreateInMemory() => new(CreateInMemoryOptions());

    /// <summary>
    /// Creates the options behind <see cref="CreateInMemory" />, for tests that construct the context themselves.
    /// </summary>
    /// <returns>Options over a fresh in-memory store, with CreatedAt stamping enabled.</returns>
    public static DbContextOptions<ContentDbContext> CreateInMemoryOptions() =>
        new DbContextOptionsBuilder<ContentDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .AddInterceptors(new CreatedAtStampingInterceptor())
            .Options;
}
