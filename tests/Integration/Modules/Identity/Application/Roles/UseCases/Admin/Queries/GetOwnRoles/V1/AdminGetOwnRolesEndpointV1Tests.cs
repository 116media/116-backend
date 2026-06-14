using System.Text.Json;
using _116.Identity.Infrastructure.Persistence;
using _116.Tests.Fixtures.Factories.Identity;

namespace _116.Integration.Tests.Modules.Identity.Application.Roles.UseCases.Admin.Queries.GetOwnRoles.V1;

/// <summary>
/// Integration tests for the AdminGetOwnRoles endpoint.
/// </summary>
[Collection("Database")]
public class AdminGetOwnRolesEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
{
    private static string ShortName(string prefix = "rq") => $"{prefix}{Guid.NewGuid().ToString("N")[..8]}";

    [Fact]
    public async Task GetOwnRoles_AsSuperAdminWithSeededRoles_ReturnsOk()
    {
        await using var context = CreateDbContext<IdentityDbContext>();
        var role = RoleFactory.Create(ShortName("own"), "Role assigned to SuperAdmin.");
        context.Roles.Add(role);
        await context.SaveChangesAsync();

        var userRole = UserRoleFactory.Create(TestUser.SuperAdminId, role.Id);
        context.UserRoles.Add(userRole);
        await context.SaveChangesAsync();

        Client.AuthenticateAsSuperAdmin();

        var response = await Client.GetAsync("/api/v1/admin/me/roles");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(body);
        var rolesProp = doc.RootElement.GetProperty("roles");

        rolesProp.GetArrayLength().Should().BeGreaterThanOrEqualTo(1);
    }

    [Fact]
    public async Task GetOwnRoles_WithoutAuthentication_ReturnsUnauthorized()
    {
        Client.ClearAuthentication();

        var response = await Client.GetAsync("/api/v1/admin/me/roles");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
