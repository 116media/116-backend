using _116.Content.Domain.Enums;
using _116.Content.Infrastructure.Persistence;

namespace _116.Integration.Tests.Modules.Content;

/// <summary>
/// Covers the seeding branch of <c>UseContentModule</c> through a host booted as Development,
/// so the assertion is about the rows the seeders write at startup.
/// </summary>
/// <param name="db">The Development-environment host and its container.</param>
[Collection("Seeding")]
public class ContentModuleSeedingTests(SeedingPostgresFixture db)
{
    [Fact]
    public async Task DevelopmentHost_RunsTheContentTypeSeeder()
    {
        using IServiceScope scope = db.Api.Services.CreateScope();
        await using ContentDbContext context = scope.ServiceProvider.GetRequiredService<ContentDbContext>();

        bool seeded = await context.ContentTypes.AnyAsync(
            contentType => contentType.Name == nameof(EnumCoreContentType.Article),
            TestContext.Current.CancellationToken
        );

        seeded.Should().BeTrue("ContentTypeSeeder runs when the host boots outside the Testing environment");
    }
}
