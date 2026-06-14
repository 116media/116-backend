using _116.Identity.Infrastructure.Persistence;
using _116.Tests.Fixtures.Factories.Identity;

namespace _116.Integration.Tests.Modules.Identity.Application.Roles.UseCases.Admin.Commands.SoftDeleteRole.V1;

/// <summary>
/// Integration tests for the AdminSoftDeleteRole endpoint.
/// </summary>
[Collection("Database")]
public class AdminSoftDeleteRoleEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
{
    private static string ShortName(string prefix = "r") => $"{prefix}{Guid.NewGuid().ToString("N")[..8]}";

    [Fact]
    public async Task SoftDeleteRole_AsSuperAdmin_ReturnsSuccess()
    {
        await using var seedContext = CreateDbContext<IdentityDbContext>();
        var role = RoleFactory.Create(ShortName("sd"), "Will be soft deleted");
        seedContext.Roles.Add(role);
        await seedContext.SaveChangesAsync();

        Client.AuthenticateAsSuperAdmin();

        var response = await Client.DeleteAsync($"{ApiRoutes.Admin.Roles}/{role.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
