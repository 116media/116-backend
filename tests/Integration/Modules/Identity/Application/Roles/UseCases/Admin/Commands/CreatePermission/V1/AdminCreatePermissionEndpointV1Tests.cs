using System.Text.Json;
using _116.Identity.Infrastructure.Persistence;
using _116.Tests.Fixtures.Factories.Identity;

namespace _116.Integration.Tests.Modules.Identity.Application.Roles.UseCases.Admin.Commands.CreatePermission.V1;

/// <summary>
/// Integration tests for the AdminCreatePermission endpoint.
/// </summary>
[Collection("Database")]
public class AdminCreatePermissionEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
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
}
