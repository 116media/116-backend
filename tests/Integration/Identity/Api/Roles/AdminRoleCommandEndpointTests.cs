using _116.Identity.Infrastructure.Persistence;
using _116.Tests.Fixtures.Factories.Identity;

namespace _116.Integration.Tests.Identity.Api.Roles;

/// <summary>
/// Integration tests for the admin role command endpoints verifying create, update,
/// activate, deactivate, soft-delete, restore, and hard-delete operations
/// against a real PostgreSQL database through the full API pipeline.
/// </summary>
[Collection("Database")]
public class AdminRoleCommandEndpointTests(PostgresFixture db) : BaseApiTest(db)
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

    [Fact]
    public async Task UpdateRole_AsSuperAdmin_WithValidData_ReturnsSuccess()
    {
        await using var seedContext = CreateDbContext<IdentityDbContext>();
        var role = RoleFactory.Create(ShortName("ub"), "Original description");
        seedContext.Roles.Add(role);
        await seedContext.SaveChangesAsync();

        Client.AuthenticateAsSuperAdmin();
        var request = new { Name = ShortName("ua"), Description = "Updated description" };

        var response = await Client.PutAsJsonAsync($"{ApiRoutes.Admin.Roles}/{role.Id}", request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task UpdateRole_NonExistentRole_ReturnsNotFound()
    {
        Client.AuthenticateAsSuperAdmin();
        var request = new { Name = ShortName("gh"), Description = "Does not exist" };

        var response = await Client.PutAsJsonAsync($"{ApiRoutes.Admin.Roles}/{Guid.NewGuid()}", request);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task UpdateRole_AsAdmin_ReturnsForbidden()
    {
        await using var seedContext = CreateDbContext<IdentityDbContext>();
        var role = RoleFactory.Create(ShortName("uf"), "Admin cannot update");
        seedContext.Roles.Add(role);
        await seedContext.SaveChangesAsync();

        Client.AuthenticateAsAdmin();
        var request = new { Name = ShortName("ux"), Description = "Should be forbidden" };

        var response = await Client.PutAsJsonAsync($"{ApiRoutes.Admin.Roles}/{role.Id}", request);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

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

    [Fact]
    public async Task HardDeleteRole_AsSuperAdmin_ReturnsSuccess_AndRemovesFromDatabase()
    {
        await using var seedContext = CreateDbContext<IdentityDbContext>();
        var role = RoleFactory.Create(ShortName("hd"), "Will be permanently deleted");
        seedContext.Roles.Add(role);
        await seedContext.SaveChangesAsync();
        var roleId = role.Id;

        Client.AuthenticateAsSuperAdmin();

        var response = await Client.DeleteAsync($"{ApiRoutes.Admin.Roles}/{roleId}/hard");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        await using var verifyContext = CreateDbContext<IdentityDbContext>();
        var deleted = await verifyContext.Roles.FirstOrDefaultAsync(r => r.Id == roleId);
        deleted.Should().BeNull();
    }

    [Fact]
    public async Task HardDeleteRole_AsVisitor_ReturnsForbidden()
    {
        await using var seedContext = CreateDbContext<IdentityDbContext>();
        var role = RoleFactory.Create(ShortName("hf"), "Visitor cannot delete");
        seedContext.Roles.Add(role);
        await seedContext.SaveChangesAsync();

        Client.AuthenticateAsVisitor();

        var response = await Client.DeleteAsync($"{ApiRoutes.Admin.Roles}/{role.Id}/hard");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
