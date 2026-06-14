using _116.Identity.Infrastructure.Persistence;
using _116.Tests.Fixtures.Factories.Identity;

namespace _116.Integration.Tests.Modules.Identity.Application.Roles.UseCases.Admin.Commands.RestoreRole.V1;

/// <summary>
/// Integration tests for the AdminRestoreRole endpoint.
/// </summary>
[Collection("Database")]
public class AdminRestoreRoleEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
{
    private static string ShortName(string prefix = "r") => $"{prefix}{Guid.NewGuid().ToString("N")[..8]}";

    [Fact]
    public async Task RestoreRole_AsSuperAdmin_AfterSoftDelete_ReturnsSuccess()
    {
        await using var seedContext = CreateDbContext<IdentityDbContext>();
        var role = RoleFactory.CreateDeleted(ShortName("rs"));
        seedContext.Roles.Add(role);
        await seedContext.SaveChangesAsync();

        Client.AuthenticateAsSuperAdmin();

        var response = await Client.PatchAsync($"{ApiRoutes.Admin.Roles}/{role.Id}/restore", null);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
