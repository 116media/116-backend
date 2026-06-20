using _116.Identity.Infrastructure.Persistence;
using _116.Tests.Fixtures.Factories.Identity;

namespace _116.Integration.Tests.Modules.Identity.Application.Roles.UseCases.Admin.Commands.CreateRole.V1;

/// <summary>
/// Integration tests for the AdminCreateRole endpoint.
/// </summary>
[Collection("Database")]
public class AdminCreateRoleEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
{
    private static string ShortName(string prefix = "r") => $"{prefix}{Guid.NewGuid().ToString("N")[..8]}";

    [Fact]
    public async Task CreateRole_AsSuperAdmin_WithValidData_ReturnsSuccess()
    {
        Client.AuthenticateAsSuperAdmin();
        var name = ShortName("cr");
        var request = new { Name = name, Description = "A test role for creation" };

        var response = await Client.PostAsJsonAsync(ApiRoutes.Admin.Roles, request);

        response.StatusCode.Should().Be(HttpStatusCode.Created);

        await using var context = CreateDbContext<IdentityDbContext>();
        var role = await context.Roles.FirstOrDefaultAsync(r => r.Name == name);
        role.Should().NotBeNull();
        role!.Description.Should().Be("A test role for creation");
    }

    [Fact]
    public async Task CreateRole_AsAdmin_ReturnsForbidden()
    {
        Client.AuthenticateAsAdmin();
        var request = new { Name = ShortName("fa"), Description = "Should not be created" };

        var response = await Client.PostAsJsonAsync(ApiRoutes.Admin.Roles, request);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task CreateRole_WithNoAuth_ReturnsUnauthorized()
    {
        Client.ClearAuthentication();
        var request = new { Name = ShortName("na"), Description = "Should not be created" };

        var response = await Client.PostAsJsonAsync(ApiRoutes.Admin.Roles, request);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task CreateRole_WithDuplicateName_ReturnsConflict()
    {
        var duplicateName = ShortName("dup");

        await using var seedContext = CreateDbContext<IdentityDbContext>();
        var existingRole = RoleFactory.Create(duplicateName, "Already exists");
        seedContext.Roles.Add(existingRole);
        await seedContext.SaveChangesAsync();

        Client.AuthenticateAsSuperAdmin();
        var request = new { Name = duplicateName, Description = "Duplicate attempt" };

        var response = await Client.PostAsJsonAsync(ApiRoutes.Admin.Roles, request);

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task CreateRole_WithEmptyName_ReturnsValidationError()
    {
        // Arrange
        Client.AuthenticateAsSuperAdmin();
        var request = new { Name = "", Description = "Missing name" };

        // Act
        var response = await Client.PostAsJsonAsync(ApiRoutes.Admin.Roles, request);

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.UnprocessableEntity);
    }
}
