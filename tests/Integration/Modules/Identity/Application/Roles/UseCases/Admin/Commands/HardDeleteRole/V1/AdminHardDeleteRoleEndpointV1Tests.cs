using _116.Identity.Application.Roles.UseCases.Admin.Commands.HardDeleteRole.V1;
using _116.Identity.Application.Shared.Errors.Messages;
using _116.Identity.Domain.Entities;
using _116.Identity.Infrastructure.Persistence;
using _116.Shared.Application.Exceptions;
using _116.Tests.Fixtures.Factories.Identity;

namespace _116.Integration.Tests.Modules.Identity.Application.Roles.UseCases.Admin.Commands.HardDeleteRole.V1;

/// <summary>
/// Integration tests for the AdminHardDeleteRole endpoint.
/// </summary>
[Collection("Database")]
public class AdminHardDeleteRoleEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
{
    private static string ShortName(string prefix = "r") => $"{prefix}{Guid.NewGuid().ToString("N")[..8]}";

    [Fact]
    public async Task HardDeleteRole_AsSuperAdmin_ReturnsSuccess_AndRemovesFromDatabase()
    {
        RoleEntity role = await SeedAsync<IdentityDbContext, RoleEntity>(ctx =>
        {
            RoleEntity entity = RoleFactory.Create(ShortName("hd"), "Will be permanently deleted");
            ctx.Roles.Add(entity);
            return entity;
        });
        Guid roleId = role.Id;

        Client.AuthenticateAsSuperAdmin();

        var response = await Client.DeleteAsync(Routes.Admin.Roles.Hard(roleId));

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.ReadAsAsync<AdminHardDeleteRoleResponse>();
        body.IsSuccess.Should().BeTrue();

        await using IdentityDbContext verifyContext = CreateDbContext<IdentityDbContext>();
        bool exists = await verifyContext.Roles.AnyAsync(r => r.Id == roleId);
        exists.Should().BeFalse();
    }

    [Fact]
    public async Task HardDeleteRole_AsVisitor_ReturnsForbidden()
    {
        RoleEntity role = await SeedAsync<IdentityDbContext, RoleEntity>(ctx =>
        {
            RoleEntity entity = RoleFactory.Create(ShortName("hf"), "Visitor cannot delete");
            ctx.Roles.Add(entity);
            return entity;
        });

        Client.AuthenticateAsVisitor();

        var response = await Client.DeleteAsync(Routes.Admin.Roles.Hard(role.Id));

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task HardDeleteRole_CoreRole_ReturnsBadRequest()
    {
        RoleEntity coreRole = await SeedAsync<IdentityDbContext, RoleEntity>(ctx =>
        {
            RoleEntity entity = RoleFactory.CreateSuperAdmin();
            ctx.Roles.Add(entity);
            return entity;
        });

        Client.AuthenticateAsSuperAdmin();

        var response = await Client.DeleteAsync(Routes.Admin.Roles.Hard(coreRole.Id));

        await response.ShouldBeProblem<BadRequestException>(
            HttpStatusCode.BadRequest,
            Localized<ValidationErrorMessage>(m => m.CoreRoleCannotBeDeleted(coreRole.Name))
        );
    }
}
