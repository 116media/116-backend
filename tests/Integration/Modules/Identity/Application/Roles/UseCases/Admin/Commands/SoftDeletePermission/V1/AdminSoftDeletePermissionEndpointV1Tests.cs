using _116.Identity.Application.Roles.UseCases.Admin.Commands.SoftDeletePermission.V1;
using _116.Identity.Application.Shared.Errors.Messages;
using _116.Identity.Domain.Entities;
using _116.Identity.Infrastructure.Persistence;
using _116.Shared.Application.Exceptions;
using _116.Tests.Fixtures.Factories.Identity;

namespace _116.Integration.Tests.Modules.Identity.Application.Roles.UseCases.Admin.Commands.SoftDeletePermission.V1;

/// <summary>
/// Integration tests for the AdminSoftDeletePermission endpoint.
/// </summary>
[Collection("Database")]
public class AdminSoftDeletePermissionEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
{
    /// <summary>
    /// Generates a unique resource name that fits the 15-char max length.
    /// </summary>
    private static string UniqueResource(string prefix = "pt") => $"{prefix}_{Guid.NewGuid().ToString("N")[..8]}";

    /// <summary>
    /// Generates a unique action name that fits the 15-char max length.
    /// </summary>
    private static string UniqueAction(string prefix = "act") => $"{prefix}_{Guid.NewGuid().ToString("N")[..8]}";

    private async Task<bool> IsPermissionDeletedAsync(Guid id)
    {
        await using IdentityDbContext ctx = CreateDbContext<IdentityDbContext>();
        PermissionEntity? permission = await ctx.Permissions.FindAsync(id);
        return permission!.IsDeleted;
    }

    [Fact]
    public async Task SoftDeletePermission_ShouldReturn200_WhenSuperAdmin()
    {
        PermissionEntity permission = await SeedAsync<IdentityDbContext, PermissionEntity>(ctx =>
        {
            PermissionEntity entity = PermissionFactory.Create(UniqueResource("sd"), UniqueAction("sd"));
            ctx.Permissions.Add(entity);
            return entity;
        });

        Client.AuthenticateAsSuperAdmin();

        var response = await Client.DeleteAsync($"{ApiRoutes.Admin.Permissions}/{permission.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.ReadAsAsync<AdminSoftDeletePermissionResponse>();
        body.IsSuccess.Should().BeTrue();
        body.Permission.Id.Should().Be(permission.Id);
        body.Permission.IsDeleted.Should().BeTrue();

        (await IsPermissionDeletedAsync(permission.Id)).Should().BeTrue();
    }

    [Fact]
    public async Task SoftDeletePermission_WhenAlreadyDeleted_ReturnsConflict()
    {
        PermissionEntity permission = await SeedAsync<IdentityDbContext, PermissionEntity>(ctx =>
        {
            PermissionEntity entity = PermissionFactory.CreateDeleted();
            ctx.Permissions.Add(entity);
            return entity;
        });

        Client.AuthenticateAsSuperAdmin();

        var response = await Client.DeleteAsync($"{ApiRoutes.Admin.Permissions}/{permission.Id}");

        await response.ShouldBeProblem<ConflictException>(
            HttpStatusCode.Conflict,
            Localized<ConflictErrorMessage>(m => m.PermissionAlreadyDeleted())
        );
        (await IsPermissionDeletedAsync(permission.Id)).Should().BeTrue();
    }
}
