using System.Text.Json;
using _116.Identity.Infrastructure.Persistence;
using _116.Tests.Fixtures.Factories.Identity;

namespace _116.Integration.Tests.Identity.Api.Roles;

/// <summary>
/// Integration tests for the admin permission CRUD and query endpoints.
/// Covers authorization enforcement (SuperAdmin-only for commands, AdminOrSuperAdmin for queries),
/// input validation, conflict detection, soft/hard delete lifecycle, and pagination.
/// </summary>
[Collection("Database")]
public class AdminPermissionEndpointTests(PostgresFixture db) : BaseApiTest(db)
{
    /// <summary>
    /// Generates a unique resource name that fits the 15-char max length.
    /// </summary>
    private static string UniqueResource(string prefix = "pt") => $"{prefix}_{Guid.NewGuid().ToString("N")[..8]}";

    /// <summary>
    /// Generates a unique action name that fits the 15-char max length.
    /// </summary>
    private static string UniqueAction(string prefix = "act") => $"{prefix}_{Guid.NewGuid().ToString("N")[..8]}";

    [Fact]
    public async Task CreatePermission_ShouldReturnSuccess_WhenSuperAdminWithValidData()
    {
        Client.AuthenticateAsSuperAdmin();

        var payload = new
        {
            Resource = UniqueResource("cr"),
            Action = UniqueAction("cr"),
            Description = "Test permission for creation",
        };

        var response = await Client.PostAsJsonAsync(ApiRoutes.Admin.Permissions, payload);

        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.Created);
    }

    [Fact]
    public async Task CreatePermission_ShouldReturn403_WhenAdmin()
    {
        Client.AuthenticateAsAdmin();

        var payload = new
        {
            Resource = UniqueResource("ad"),
            Action = UniqueAction("ad"),
            Description = "Admin should not be allowed",
        };

        var response = await Client.PostAsJsonAsync(ApiRoutes.Admin.Permissions, payload);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task CreatePermission_ShouldReturn401_WhenNoAuth()
    {
        Client.ClearAuthentication();

        var payload = new
        {
            Resource = UniqueResource("na"),
            Action = UniqueAction("na"),
            Description = "No auth should be rejected",
        };

        var response = await Client.PostAsJsonAsync(ApiRoutes.Admin.Permissions, payload);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task CreatePermission_ShouldReturn409_WhenDuplicateResourceAction()
    {
        Client.AuthenticateAsSuperAdmin();

        var resource = UniqueResource("dup");
        var action = UniqueAction("dup");

        var payload = new
        {
            Resource = resource,
            Action = action,
            Description = "First creation",
        };

        await Client.PostAsJsonAsync(ApiRoutes.Admin.Permissions, payload);

        var duplicatePayload = new
        {
            Resource = resource,
            Action = action,
            Description = "Duplicate creation",
        };

        var response = await Client.PostAsJsonAsync(ApiRoutes.Admin.Permissions, duplicatePayload);

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task CreatePermission_ShouldReturnBadRequest_WhenResourceIsEmpty()
    {
        Client.AuthenticateAsSuperAdmin();

        var payload = new
        {
            Resource = "",
            Action = UniqueAction("ev"),
            Description = "Empty resource should fail",
        };

        var response = await Client.PostAsJsonAsync(ApiRoutes.Admin.Permissions, payload);

        response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.UnprocessableEntity);
    }

    [Fact]
    public async Task UpdatePermission_ShouldReturn200_WhenSuperAdminWithValidData()
    {
        Client.AuthenticateAsSuperAdmin();

        var createPayload = new
        {
            Resource = UniqueResource("up"),
            Action = UniqueAction("up"),
            Description = "To be updated",
        };

        var createResponse = await Client.PostAsJsonAsync(ApiRoutes.Admin.Permissions, createPayload);
        var createBody = await createResponse.Content.ReadAsStringAsync();
        using var createDoc = JsonDocument.Parse(createBody);
        var permissionId = createDoc.RootElement.GetProperty("permission").GetProperty("id").GetString();

        var updatePayload = new
        {
            Resource = UniqueResource("upd"),
            Action = UniqueAction("upd"),
            Description = "Updated description",
        };

        var response = await Client.PutAsJsonAsync($"{ApiRoutes.Admin.Permissions}/{permissionId}", updatePayload);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task UpdatePermission_ShouldReturn404_WhenNonExistent()
    {
        Client.AuthenticateAsSuperAdmin();

        var nonExistentId = Guid.NewGuid();
        var updatePayload = new
        {
            Resource = UniqueResource("nf"),
            Action = UniqueAction("nf"),
            Description = "Should not be found",
        };

        var response = await Client.PutAsJsonAsync($"{ApiRoutes.Admin.Permissions}/{nonExistentId}", updatePayload);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task ActivatePermission_ShouldReturn200_WhenSuperAdmin()
    {
        Client.AuthenticateAsSuperAdmin();

        var createPayload = new
        {
            Resource = UniqueResource("ac"),
            Action = UniqueAction("ac"),
            Description = "To be activated",
        };

        var createResponse = await Client.PostAsJsonAsync(ApiRoutes.Admin.Permissions, createPayload);
        var createBody = await createResponse.Content.ReadAsStringAsync();
        using var createDoc = JsonDocument.Parse(createBody);
        var permissionId = createDoc.RootElement.GetProperty("permission").GetProperty("id").GetString();

        await Client.PatchAsync($"{ApiRoutes.Admin.Permissions}/{permissionId}/deactivate", null);

        var response = await Client.PatchAsync($"{ApiRoutes.Admin.Permissions}/{permissionId}/activate", null);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task DeactivatePermission_ShouldReturn200_WhenSuperAdmin()
    {
        Client.AuthenticateAsSuperAdmin();

        var createPayload = new
        {
            Resource = UniqueResource("da"),
            Action = UniqueAction("da"),
            Description = "To be deactivated",
        };

        var createResponse = await Client.PostAsJsonAsync(ApiRoutes.Admin.Permissions, createPayload);
        var createBody = await createResponse.Content.ReadAsStringAsync();
        using var createDoc = JsonDocument.Parse(createBody);
        var permissionId = createDoc.RootElement.GetProperty("permission").GetProperty("id").GetString();

        var response = await Client.PatchAsync($"{ApiRoutes.Admin.Permissions}/{permissionId}/deactivate", null);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task SoftDeletePermission_ShouldReturn200_WhenSuperAdmin()
    {
        Client.AuthenticateAsSuperAdmin();

        var createPayload = new
        {
            Resource = UniqueResource("sd"),
            Action = UniqueAction("sd"),
            Description = "To be soft deleted",
        };

        var createResponse = await Client.PostAsJsonAsync(ApiRoutes.Admin.Permissions, createPayload);
        var createBody = await createResponse.Content.ReadAsStringAsync();
        using var createDoc = JsonDocument.Parse(createBody);
        var permissionId = createDoc.RootElement.GetProperty("permission").GetProperty("id").GetString();

        var response = await Client.DeleteAsync($"{ApiRoutes.Admin.Permissions}/{permissionId}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task RestorePermission_ShouldReturn200_WhenSuperAdminAfterSoftDelete()
    {
        Client.AuthenticateAsSuperAdmin();

        var createPayload = new
        {
            Resource = UniqueResource("rs"),
            Action = UniqueAction("rs"),
            Description = "To be restored",
        };

        var createResponse = await Client.PostAsJsonAsync(ApiRoutes.Admin.Permissions, createPayload);
        var createBody = await createResponse.Content.ReadAsStringAsync();
        using var createDoc = JsonDocument.Parse(createBody);
        var permissionId = createDoc.RootElement.GetProperty("permission").GetProperty("id").GetString();

        await Client.DeleteAsync($"{ApiRoutes.Admin.Permissions}/{permissionId}");

        var response = await Client.PatchAsync($"{ApiRoutes.Admin.Permissions}/{permissionId}/restore", null);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task HardDeletePermission_ShouldReturn200_AndRemoveFromDatabase()
    {
        Client.AuthenticateAsSuperAdmin();

        var createPayload = new
        {
            Resource = UniqueResource("hd"),
            Action = UniqueAction("hd"),
            Description = "To be hard deleted",
        };

        var createResponse = await Client.PostAsJsonAsync(ApiRoutes.Admin.Permissions, createPayload);
        var createBody = await createResponse.Content.ReadAsStringAsync();
        using var createDoc = JsonDocument.Parse(createBody);
        var permissionId = Guid.Parse(createDoc.RootElement.GetProperty("permission").GetProperty("id").GetString()!);

        var response = await Client.DeleteAsync($"{ApiRoutes.Admin.Permissions}/{permissionId}/hard");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        await using var context = CreateDbContext<IdentityDbContext>();
        var exists = await context.Permissions.AnyAsync(p => p.Id == permissionId);
        exists.Should().BeFalse();
    }

    [Fact]
    public async Task GetAllPermissions_ShouldReturn200_WhenAdmin()
    {
        Client.AuthenticateAsSuperAdmin();

        var createPayload = new
        {
            Resource = UniqueResource("ga"),
            Action = UniqueAction("ga"),
            Description = "Seeded for listing",
        };

        await Client.PostAsJsonAsync(ApiRoutes.Admin.Permissions, createPayload);

        Client.AuthenticateAsAdmin();

        var response = await Client.GetAsync($"{ApiRoutes.Admin.Permissions}?pageIndex=0&pageSize=10");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetPermissionById_ShouldReturn200_WhenAdminAndExists()
    {
        Client.AuthenticateAsSuperAdmin();

        var createPayload = new
        {
            Resource = UniqueResource("gi"),
            Action = UniqueAction("gi"),
            Description = "Seeded for get by id",
        };

        var createResponse = await Client.PostAsJsonAsync(ApiRoutes.Admin.Permissions, createPayload);
        var createBody = await createResponse.Content.ReadAsStringAsync();
        using var createDoc = JsonDocument.Parse(createBody);
        var permissionId = createDoc.RootElement.GetProperty("permission").GetProperty("id").GetString();

        Client.AuthenticateAsAdmin();

        var response = await Client.GetAsync($"{ApiRoutes.Admin.Permissions}/{permissionId}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetPermissionById_ShouldReturn404_WhenNonExistentGuid()
    {
        Client.AuthenticateAsAdmin();

        var nonExistentId = Guid.NewGuid();

        var response = await Client.GetAsync($"{ApiRoutes.Admin.Permissions}/{nonExistentId}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
