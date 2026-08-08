using _116.Identity.Application.Roles.Constants;
using _116.Identity.Application.Shared.Errors.Messages;
using _116.Identity.Application.User.UseCases.Admin.Commands.RemoveRoleFromUser.V1;
using _116.Identity.Domain.Entities;
using _116.Identity.Infrastructure.Persistence;
using _116.Shared.Application.Exceptions;
using _116.Tests.Fixtures.Factories.Identity;

namespace _116.Integration.Tests.Modules.Identity.Application.User.UseCases.Admin.Commands.RemoveRoleFromUser.V1;

/// <summary>
/// Integration tests for the AdminRemoveRoleFromUser endpoint.
/// </summary>
[Collection("Database")]
public class AdminRemoveRoleFromUserEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
{
    private static string UserRoleUrl(Guid userId, Guid roleId) =>
        $"{ApiRoutes.Admin.Users}/{userId}/{RoleRouteConstants.Endpoint}/{roleId}";

    [Fact]
    public async Task AdminRemoveRole_AsSuperAdmin_Returns200()
    {
        RoleEntity role = await SeedAsync<IdentityDbContext, RoleEntity>(context =>
        {
            RoleEntity created = RoleFactory.Create();
            context.Roles.Add(created);
            context.UserRoles.Add(UserRoleFactory.Create(TestUser.AdminId, created.Id));
            return created;
        });

        Client.AuthenticateAsSuperAdmin();

        var response = await Client.DeleteAsync(UserRoleUrl(TestUser.AdminId, role.Id));

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        AdminRemoveRoleFromUserResponse body = await response.ReadAsAsync<AdminRemoveRoleFromUserResponse>();
        body.IsSuccess.Should().BeTrue();
        body.Roles.Should().NotContain(r => r.Id == role.Id);

        await using var verifyContext = CreateDbContext<IdentityDbContext>();
        (await verifyContext.UserRoles.AnyAsync(ur => ur.UserId == TestUser.AdminId && ur.RoleId == role.Id))
            .Should()
            .BeFalse();
    }

    [Fact]
    public async Task AdminRemoveRole_AsAdmin_Returns403()
    {
        RoleEntity role = await SeedAsync<IdentityDbContext, RoleEntity>(context =>
        {
            RoleEntity created = RoleFactory.Create();
            context.Roles.Add(created);
            context.UserRoles.Add(UserRoleFactory.Create(TestUser.AdminId, created.Id));
            return created;
        });

        Client.AuthenticateAsAdmin();

        var response = await Client.DeleteAsync(UserRoleUrl(TestUser.AdminId, role.Id));

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task RemoveRole_WhenNotAssigned_ReturnsBadRequest()
    {
        RoleEntity role = await SeedAsync<IdentityDbContext, RoleEntity>(context =>
        {
            RoleEntity created = RoleFactory.Create();
            context.Roles.Add(created);
            return created;
        });

        Client.AuthenticateAsSuperAdmin();

        var response = await Client.DeleteAsync(UserRoleUrl(TestUser.AdminId, role.Id));

        await response.ShouldBeProblem<BadRequestException>(
            HttpStatusCode.BadRequest,
            Localized<ValidationErrorMessage>(m => m.RoleNotAssignedToUser())
        );
    }
}
