using _116.Identity.Domain.Enums;
using _116.Identity.Infrastructure.Persistence;

namespace _116.Integration.Tests.Modules.Identity;

/// <summary>
/// Covers the seeding branch of <c>UseIdentityModule</c> through a host booted as Development,
/// so the assertion is about the rows the seeders write at startup.
/// </summary>
/// <param name="db">The Development-environment host and its container.</param>
[Collection("Seeding")]
public class IdentityModuleSeedingTests(SeedingPostgresFixture db)
{
    [Fact]
    public async Task DevelopmentHost_RunsTheIdentitySeeders()
    {
        using IServiceScope scope = db.Api.Services.CreateScope();
        await using IdentityDbContext context = scope.ServiceProvider.GetRequiredService<IdentityDbContext>();

        bool seeded = await context.Roles.AnyAsync(
            role => role.Name == nameof(EnumCoreUserRole.Visitor),
            TestContext.Current.CancellationToken
        );

        seeded.Should().BeTrue("VisitorRoleSeeder runs when the host boots outside the Testing environment");
    }
}
