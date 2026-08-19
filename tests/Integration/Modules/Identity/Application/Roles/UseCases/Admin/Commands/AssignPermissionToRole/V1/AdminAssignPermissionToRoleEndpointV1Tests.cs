using _116.Identity.Application.Roles.UseCases.Admin.Commands.AssignPermissionToRole.V1;
using _116.Identity.Domain.Entities;
using _116.Identity.Infrastructure.Persistence;
using _116.Tests.Fixtures.Builders.Requests.Identity;
using _116.Tests.Fixtures.Factories.Identity;

namespace _116.Integration.Tests.Modules.Identity.Application.Roles.UseCases.Admin.Commands.AssignPermissionToRole.V1;

/// <summary>
/// Integration tests for the AdminAssignPermissionToRole endpoint.
/// </summary>
[Collection("Database")]
public class AdminAssignPermissionToRoleEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
{
    private async Task<bool> AssignmentExistsAsync(Guid roleId, Guid permissionId)
    {
        await using IdentityDbContext ctx = CreateDbContext<IdentityDbContext>();
        return await ctx.RolePermissions.AnyAsync(rp => rp.RoleId == roleId && rp.PermissionId == permissionId);
    }

    [Fact]
    public async Task AssignPermission_AsSuperAdmin_WithValidData_ReturnsSuccess()
    {
        Guid roleId = Guid.NewGuid();
        Guid permissionId = Guid.NewGuid();

        await SeedAsync<IdentityDbContext>(ctx =>
        {
            ctx.Roles.Add(RoleFactory.CreateWithId(roleId, $"rpa_{Guid.NewGuid():N}"[..20]));
            ctx.Permissions.Add(PermissionFactory.CreateWithId(permissionId, $"rpa_{Guid.NewGuid():N}"[..15], "read"));
        });

        Client.AuthenticateAsSuperAdmin();
        AdminAssignPermissionToRoleRequest request = new AdminAssignPermissionToRoleRequestBuilder()
            .WithPermissionId(permissionId)
            .Build();

        var response = await Client.PostAsJsonAsync(Routes.Admin.Roles.Permissions(roleId), request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.ReadAsAsync<AdminAssignPermissionToRoleResponse>();
        body.Role.Id.Should().Be(roleId);
        body.Role.Permissions.Should().Contain(p => p.Id == permissionId);

        (await AssignmentExistsAsync(roleId, permissionId)).Should().BeTrue();
    }

    [Fact]
    public async Task AssignPermission_AsAdmin_ReturnsForbidden()
    {
        Guid roleId = Guid.NewGuid();
        Guid permissionId = Guid.NewGuid();

        await SeedAsync<IdentityDbContext>(ctx =>
        {
            ctx.Roles.Add(RoleFactory.CreateWithId(roleId, $"rpa_{Guid.NewGuid():N}"[..20]));
            ctx.Permissions.Add(PermissionFactory.CreateWithId(permissionId, $"rpa_{Guid.NewGuid():N}"[..15], "read"));
        });

        Client.AuthenticateAsAdmin();
        AdminAssignPermissionToRoleRequest request = new AdminAssignPermissionToRoleRequestBuilder()
            .WithPermissionId(permissionId)
            .Build();

        var response = await Client.PostAsJsonAsync(Routes.Admin.Roles.Permissions(roleId), request);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task AssignPermission_WithNoAuth_ReturnsUnauthorized()
    {
        Client.ClearAuthentication();
        Guid roleId = Guid.NewGuid();
        AdminAssignPermissionToRoleRequest request = new AdminAssignPermissionToRoleRequestBuilder().Build();

        var response = await Client.PostAsJsonAsync(Routes.Admin.Roles.Permissions(roleId), request);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task AssignPermission_NonExistentRole_ReturnsNotFound()
    {
        Guid permissionId = Guid.NewGuid();

        await SeedAsync<IdentityDbContext>(ctx =>
        {
            ctx.Permissions.Add(PermissionFactory.CreateWithId(permissionId, $"rpa_{Guid.NewGuid():N}"[..15], "read"));
        });

        Client.AuthenticateAsSuperAdmin();
        Guid nonExistentRoleId = Guid.NewGuid();
        AdminAssignPermissionToRoleRequest request = new AdminAssignPermissionToRoleRequestBuilder()
            .WithPermissionId(permissionId)
            .Build();

        var response = await Client.PostAsJsonAsync(Routes.Admin.Roles.Permissions(nonExistentRoleId), request);

        await response.ShouldBeProblem(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task AssignPermission_NonExistentPermission_ReturnsNotFound()
    {
        Guid roleId = Guid.NewGuid();

        await SeedAsync<IdentityDbContext>(ctx =>
        {
            ctx.Roles.Add(RoleFactory.CreateWithId(roleId, $"rpa_{Guid.NewGuid():N}"[..20]));
        });

        Client.AuthenticateAsSuperAdmin();
        Guid nonExistentPermissionId = Guid.NewGuid();
        AdminAssignPermissionToRoleRequest request = new AdminAssignPermissionToRoleRequestBuilder()
            .WithPermissionId(nonExistentPermissionId)
            .Build();

        var response = await Client.PostAsJsonAsync(Routes.Admin.Roles.Permissions(roleId), request);

        await response.ShouldBeProblem(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task AssignPermission_AlreadyAssigned_ReturnsConflict()
    {
        Guid roleId = Guid.NewGuid();
        Guid permissionId = Guid.NewGuid();

        await SeedAsync<IdentityDbContext>(ctx =>
        {
            ctx.Roles.Add(RoleFactory.CreateWithId(roleId, $"rpa_{Guid.NewGuid():N}"[..20]));
            ctx.Permissions.Add(PermissionFactory.CreateWithId(permissionId, $"rpa_{Guid.NewGuid():N}"[..15], "read"));
            ctx.RolePermissions.Add(RolePermissionEntity.Create(Guid.NewGuid(), roleId, permissionId));
        });

        Client.AuthenticateAsSuperAdmin();
        AdminAssignPermissionToRoleRequest request = new AdminAssignPermissionToRoleRequestBuilder()
            .WithPermissionId(permissionId)
            .Build();

        var response = await Client.PostAsJsonAsync(Routes.Admin.Roles.Permissions(roleId), request);

        await response.ShouldBeProblem(HttpStatusCode.Conflict);
    }

    /// <summary>
    /// Verifies that assigning an inactive permission to a role returns a 400 Bad Request.
    /// </summary>
    [Fact]
    public async Task AssignPermission_WhenPermissionInactive_ReturnsBadRequest()
    {
        Guid roleId = Guid.NewGuid();

        PermissionEntity permission = await SeedAsync<IdentityDbContext, PermissionEntity>(ctx =>
        {
            ctx.Roles.Add(RoleFactory.CreateWithId(roleId, $"rpa_{Guid.NewGuid():N}"[..20]));
            PermissionEntity entity = PermissionFactory.CreateInactive();
            ctx.Permissions.Add(entity);
            return entity;
        });

        Client.AuthenticateAsSuperAdmin();
        AdminAssignPermissionToRoleRequest request = new AdminAssignPermissionToRoleRequestBuilder()
            .WithPermissionId(permission.Id)
            .Build();

        var response = await Client.PostAsJsonAsync(Routes.Admin.Roles.Permissions(roleId), request);

        await response.ShouldBeProblem(HttpStatusCode.BadRequest);
    }

    /// <summary>
    /// Verifies that assigning a soft-deleted permission to a role returns a 400 Bad Request
    /// carrying the deleted-permission message rather than the inactive-permission one, since a
    /// soft delete also clears <c>IsActive</c> and both states would otherwise be
    /// indistinguishable.
    /// </summary>
    [Fact]
    public async Task AssignPermission_WhenPermissionDeleted_ReturnsBadRequest()
    {
        Guid roleId = Guid.NewGuid();

        PermissionEntity permission = await SeedAsync<IdentityDbContext, PermissionEntity>(ctx =>
        {
            ctx.Roles.Add(RoleFactory.CreateWithId(roleId, $"rpa_{Guid.NewGuid():N}"[..20]));
            PermissionEntity entity = PermissionFactory.CreateDeleted();
            ctx.Permissions.Add(entity);
            return entity;
        });

        Client.AuthenticateAsSuperAdmin();
        AdminAssignPermissionToRoleRequest request = new AdminAssignPermissionToRoleRequestBuilder()
            .WithPermissionId(permission.Id)
            .Build();

        var httpRequest = new HttpRequestMessage(HttpMethod.Post, Routes.Admin.Roles.Permissions(roleId))
        {
            Content = JsonContent.Create(request),
        };
        httpRequest.Headers.Add("Accept-Language", "en");

        var response = await Client.SendAsync(httpRequest);

        await response.ShouldBeProblem(HttpStatusCode.BadRequest, "Cannot use a deleted permission.");
    }
}
