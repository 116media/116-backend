using _116.Identity.Application.Roles.UseCases.Admin.Commands.UpdatePermission.V1;
using _116.Identity.Domain.Entities;
using _116.Identity.Infrastructure.Persistence;
using _116.Shared.Application.Exceptions;
using _116.Shared.Application.Exceptions.Messages;
using _116.Tests.Fixtures.Builders.Requests.Identity;
using _116.Tests.Fixtures.Factories.Identity;

namespace _116.Integration.Tests.Modules.Identity.Application.Roles.UseCases.Admin.Commands.UpdatePermission.V1;

/// <summary>
/// Integration tests for the AdminUpdatePermission endpoint.
/// </summary>
[Collection("Database")]
public class AdminUpdatePermissionEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
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
    public async Task UpdatePermission_ShouldReturn200_WhenSuperAdminWithValidData()
    {
        PermissionEntity permission = await SeedAsync<IdentityDbContext, PermissionEntity>(ctx =>
        {
            PermissionEntity entity = PermissionFactory.Create(
                UniqueResource("up"),
                UniqueAction("up"),
                "To be updated"
            );
            ctx.Permissions.Add(entity);
            return entity;
        });

        Client.AuthenticateAsSuperAdmin();

        AdminUpdatePermissionRequest request = new AdminUpdatePermissionRequestBuilder().Build();

        var response = await Client.PutAsJsonAsync($"{ApiRoutes.Admin.Permissions}/{permission.Id}", request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.ReadAsAsync<AdminUpdatePermissionResponse>();
        body.Permission.Id.Should().Be(permission.Id);
        body.Permission.Resource.Should().Be(request.Resource);
        body.Permission.Action.Should().Be(request.Action);
        body.Permission.Description.Should().Be(request.Description);

        await using IdentityDbContext context = CreateDbContext<IdentityDbContext>();
        PermissionEntity? updated = await context.Permissions.FindAsync(permission.Id);
        updated!.Resource.Should().Be(request.Resource);
        updated.Action.Should().Be(request.Action);
        updated.Description.Should().Be(request.Description);
    }

    [Fact]
    public async Task UpdatePermission_ShouldReturn404_WhenNonExistent()
    {
        Client.AuthenticateAsSuperAdmin();

        Guid nonExistentId = Guid.NewGuid();
        AdminUpdatePermissionRequest request = new AdminUpdatePermissionRequestBuilder().Build();

        var response = await Client.PutAsJsonAsync($"{ApiRoutes.Admin.Permissions}/{nonExistentId}", request);

        await response.ShouldBeProblem<NotFoundException>(
            HttpStatusCode.NotFound,
            Localized<SharedExceptionMessage>(m => m.EntityNotFound("Permission"))
        );
    }
}
