using _116.Identity.Infrastructure.Persistence;
using _116.Tests.Fixtures.Factories.Identity;

namespace _116.Integration.Tests.Modules.Identity.Application.Roles.UseCases.Admin.Commands.DeactivateRole.V1;

/// <summary>
/// Integration tests for the AdminDeactivateRole endpoint.
/// </summary>
[Collection("Database")]
public class AdminDeactivateRoleEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
{
    private static string ShortName(string prefix = "r") => $"{prefix}{Guid.NewGuid().ToString("N")[..8]}";

    [Fact]
    public async Task DeactivateRole_AsSuperAdmin_ReturnsSuccess()
    {
        await using var seedContext = CreateDbContext<IdentityDbContext>();
        var role = RoleFactory.Create(ShortName("da"), "Will be deactivated");
        seedContext.Roles.Add(role);
        await seedContext.SaveChangesAsync();

        Client.AuthenticateAsSuperAdmin();

        var response = await Client.PatchAsync($"{ApiRoutes.Admin.Roles}/{role.Id}/deactivate", null);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    /// <summary>
    /// Verifies that deactivating a role that is already inactive returns a 409 Conflict.
    /// </summary>
    [Fact]
    public async Task DeactivateRole_WhenAlreadyInactive_ReturnsConflict()
    {
        await using var seedContext = CreateDbContext<IdentityDbContext>();
        var role = RoleFactory.CreateInactive(ShortName("di"));
        seedContext.Roles.Add(role);
        await seedContext.SaveChangesAsync();

        Client.AuthenticateAsSuperAdmin();

        var response = await Client.PatchAsync($"{ApiRoutes.Admin.Roles}/{role.Id}/deactivate", null);

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }
}
