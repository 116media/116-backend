using _116.Identity.Infrastructure.Persistence;
using _116.Tests.Fixtures.Factories.Identity;

namespace _116.Integration.Tests.Modules.Identity.Application.Roles.UseCases.Admin.Commands.ActivateRole.V1;

/// <summary>
/// Integration tests for the AdminActivateRole endpoint.
/// </summary>
[Collection("Database")]
public class AdminActivateRoleEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
{
    private static string ShortName(string prefix = "r") => $"{prefix}{Guid.NewGuid().ToString("N")[..8]}";

    [Fact]
    public async Task ActivateRole_AsSuperAdmin_ReturnsSuccess()
    {
        await using var seedContext = CreateDbContext<IdentityDbContext>();
        var role = RoleFactory.CreateInactive(ShortName("ac"));
        seedContext.Roles.Add(role);
        await seedContext.SaveChangesAsync();

        Client.AuthenticateAsSuperAdmin();

        var response = await Client.PatchAsync($"{ApiRoutes.Admin.Roles}/{role.Id}/activate", null);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task ActivateRole_NonExistentRole_ReturnsNotFound()
    {
        Client.AuthenticateAsSuperAdmin();

        var response = await Client.PatchAsync($"{ApiRoutes.Admin.Roles}/{Guid.NewGuid()}/activate", null);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    /// <summary>
    /// Verifies that activating a role that is already active returns a 409 Conflict.
    /// </summary>
    [Fact]
    public async Task ActivateRole_WhenAlreadyActive_ReturnsConflict()
    {
        await using var seedContext = CreateDbContext<IdentityDbContext>();
        var role = RoleFactory.Create(ShortName("aa"));
        seedContext.Roles.Add(role);
        await seedContext.SaveChangesAsync();

        Client.AuthenticateAsSuperAdmin();

        var response = await Client.PatchAsync($"{ApiRoutes.Admin.Roles}/{role.Id}/activate", null);

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }
}
