using _116.Identity.Application.Roles.Constants;
using _116.Identity.Application.User.UseCases.Admin.Commands.AssignRoleToUser.V1;
using _116.Identity.Domain.Entities;
using _116.Identity.Infrastructure.Persistence;
using _116.Tests.Fixtures.Builders.Requests.Identity;
using _116.Tests.Fixtures.Factories.Identity;

namespace _116.Integration.Tests.Modules.Identity.Application.User.UseCases.Admin.Commands.AssignRoleToUser.V1;

/// <summary>
/// Integration tests for the AdminAssignRoleToUser endpoint.
/// </summary>
[Collection("Database")]
public class AdminAssignRoleToUserEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
{
    private static string UserRolesUrl(Guid userId) =>
        $"{ApiRoutes.Admin.Users}/{userId}/{RoleRouteConstants.Endpoint}";

    [Fact]
    public async Task AdminAssignRole_AsSuperAdmin_WithValidData_Returns200()
    {
        RoleEntity role = await SeedAsync<IdentityDbContext, RoleEntity>(context =>
        {
            RoleEntity created = RoleFactory.Create();
            context.Roles.Add(created);
            return created;
        });

        Client.AuthenticateAsSuperAdmin();
        var request = new AdminAssignRoleToUserRequestBuilder().WithRoleId(role.Id).Build();

        var response = await Client.PostAsJsonAsync(UserRolesUrl(TestUser.AdminId), request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        AdminAssignRoleToUserResponse body = await response.ReadAsAsync<AdminAssignRoleToUserResponse>();
        body.Roles.Should().Contain(r => r.Id == role.Id);

        await using var verifyContext = CreateDbContext<IdentityDbContext>();
        (await verifyContext.UserRoles.AnyAsync(ur => ur.UserId == TestUser.AdminId && ur.RoleId == role.Id))
            .Should()
            .BeTrue();
    }

    [Fact]
    public async Task AdminAssignRole_AsAdmin_Returns403()
    {
        RoleEntity role = await SeedAsync<IdentityDbContext, RoleEntity>(context =>
        {
            RoleEntity created = RoleFactory.Create();
            context.Roles.Add(created);
            return created;
        });

        Client.AuthenticateAsAdmin();
        var request = new AdminAssignRoleToUserRequestBuilder().WithRoleId(role.Id).Build();

        var response = await Client.PostAsJsonAsync(UserRolesUrl(TestUser.AdminId), request);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task AdminAssignRole_WithNonExistentRole_Returns404()
    {
        Client.AuthenticateAsSuperAdmin();
        var request = new AdminAssignRoleToUserRequestBuilder().WithRoleId(Guid.NewGuid()).Build();

        var response = await Client.PostAsJsonAsync(UserRolesUrl(TestUser.AdminId), request);

        await response.ShouldBeProblem(HttpStatusCode.NotFound);
    }

    /// <summary>
    /// Verifies that assigning a role that is already assigned to the user returns a 409 Conflict.
    /// </summary>
    [Fact]
    public async Task AssignRole_WhenAlreadyAssigned_ReturnsConflict()
    {
        RoleEntity role = await SeedAsync<IdentityDbContext, RoleEntity>(context =>
        {
            RoleEntity created = RoleFactory.Create();
            context.Roles.Add(created);
            context.UserRoles.Add(UserRoleFactory.Create(TestUser.AdminId, created.Id));
            return created;
        });

        Client.AuthenticateAsSuperAdmin();
        var request = new AdminAssignRoleToUserRequestBuilder().WithRoleId(role.Id).Build();

        var response = await Client.PostAsJsonAsync(UserRolesUrl(TestUser.AdminId), request);

        await response.ShouldBeProblem(HttpStatusCode.Conflict);
    }

    /// <summary>
    /// Verifies that assigning an inactive role to a user returns a 400 Bad Request.
    /// </summary>
    [Fact]
    public async Task AssignRole_WhenRoleInactive_ReturnsBadRequest()
    {
        RoleEntity role = await SeedAsync<IdentityDbContext, RoleEntity>(context =>
        {
            RoleEntity created = RoleFactory.CreateInactive();
            context.Roles.Add(created);
            return created;
        });

        Client.AuthenticateAsSuperAdmin();
        var request = new AdminAssignRoleToUserRequestBuilder().WithRoleId(role.Id).Build();

        var response = await Client.PostAsJsonAsync(UserRolesUrl(TestUser.AdminId), request);

        await response.ShouldBeProblem(HttpStatusCode.BadRequest);
    }

    /// <summary>
    /// Verifies that assigning a soft-deleted role to a user returns a 400 Bad Request carrying
    /// the deleted-role message rather than the inactive-role one, since a soft delete also
    /// clears <c>IsActive</c> and both states would otherwise be indistinguishable.
    /// </summary>
    [Fact]
    public async Task AssignRole_WhenRoleDeleted_ReturnsBadRequest()
    {
        RoleEntity role = await SeedAsync<IdentityDbContext, RoleEntity>(context =>
        {
            RoleEntity created = RoleFactory.CreateDeleted();
            context.Roles.Add(created);
            return created;
        });

        Client.AuthenticateAsSuperAdmin();
        var request = new AdminAssignRoleToUserRequestBuilder().WithRoleId(role.Id).Build();

        var httpRequest = new HttpRequestMessage(HttpMethod.Post, UserRolesUrl(TestUser.AdminId))
        {
            Content = JsonContent.Create(request),
        };
        httpRequest.Headers.Add("Accept-Language", "en");

        var response = await Client.SendAsync(httpRequest);

        await response.ShouldBeProblem(HttpStatusCode.BadRequest, "Cannot use a deleted role.");
    }
}
