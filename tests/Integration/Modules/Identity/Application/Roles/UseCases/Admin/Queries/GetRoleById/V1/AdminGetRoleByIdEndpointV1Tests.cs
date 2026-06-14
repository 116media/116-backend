using System.Text.Json;
using _116.Identity.Infrastructure.Persistence;
using _116.Tests.Fixtures.Factories.Identity;

namespace _116.Integration.Tests.Modules.Identity.Application.Roles.UseCases.Admin.Queries.GetRoleById.V1;

/// <summary>
/// Integration tests for the AdminGetRoleById endpoint.
/// </summary>
[Collection("Database")]
public class AdminGetRoleByIdEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
{
    private static string ShortName(string prefix = "rq") => $"{prefix}{Guid.NewGuid().ToString("N")[..8]}";

    [Fact]
    public async Task GetRoleById_AsSuperAdmin_WithExistingRole_ReturnsOk()
    {
        await using var context = CreateDbContext<IdentityDbContext>();
        var role = RoleFactory.Create(ShortName("det"), "A role fetched by ID.");
        context.Roles.Add(role);
        await context.SaveChangesAsync();

        Client.AuthenticateAsSuperAdmin();

        var response = await Client.GetAsync($"{ApiRoutes.Admin.Roles}/{role.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(body);
        var roleProp = doc.RootElement.GetProperty("role");

        roleProp.GetProperty("id").GetString().Should().Be(role.Id.ToString());
    }

    [Fact]
    public async Task GetRoleById_WithNonExistentGuid_ReturnsNotFound()
    {
        Client.AuthenticateAsSuperAdmin();

        var response = await Client.GetAsync($"{ApiRoutes.Admin.Roles}/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetRoleById_AsVisitor_ReturnsForbidden()
    {
        await using var context = CreateDbContext<IdentityDbContext>();
        var role = RoleFactory.Create(ShortName("fb"), "Visitor should not access.");
        context.Roles.Add(role);
        await context.SaveChangesAsync();

        Client.AuthenticateAsVisitor();

        var response = await Client.GetAsync($"{ApiRoutes.Admin.Roles}/{role.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
