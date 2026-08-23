using _116.Identity.Application.Roles.UseCases.Admin.Queries.GetRoleById.V1;
using _116.Identity.Domain.Entities;
using _116.Identity.Infrastructure.Persistence;
using _116.Shared.Application.Exceptions;
using _116.Shared.Application.Exceptions.Messages;
using _116.Tests.Fixtures.Factories.Identity;

namespace _116.Integration.Tests.Modules.Identity.Application.Roles.UseCases.Admin.Queries.GetRoleById.V1;

/// <summary>
/// Integration tests for the AdminGetRoleById endpoint.
/// </summary>
[Collection("Database")]
public class AdminGetRoleByIdEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
{
    private static string ShortName(string prefix = "rq") => $"{prefix}{Guid.NewGuid().ToString("N")[..8]}";

    [Fact]
    public async Task GetRoleById_AsSuperAdmin_WithExistingRole_ReturnsOk()
    {
        string roleName = ShortName("det");
        RoleEntity role = await SeedAsync<IdentityDbContext, RoleEntity>(ctx =>
        {
            RoleEntity entity = RoleFactory.Create(roleName, "A role fetched by ID.");
            ctx.Roles.Add(entity);
            return entity;
        });

        Client.AuthenticateAsSuperAdmin();

        var response = await Client.GetAsync($"{ApiRoutes.Admin.Roles}/{role.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.ReadAsAsync<AdminGetRoleByIdResponse>();
        body.Role.Id.Should().Be(role.Id);
        body.Role.Name.Should().Be(roleName);
        body.Role.Description.Should().Be("A role fetched by ID.");
    }

    [Fact]
    public async Task GetRoleById_WithNonExistentGuid_ReturnsNotFound()
    {
        Client.AuthenticateAsSuperAdmin();

        var response = await Client.GetAsync($"{ApiRoutes.Admin.Roles}/{Guid.NewGuid()}");

        await response.ShouldBeProblem<NotFoundException>(
            HttpStatusCode.NotFound,
            Localized<SharedExceptionMessage>(m => m.EntityNotFound("Role"))
        );
    }

    [Fact]
    public async Task GetRoleById_AsVisitor_ReturnsForbidden()
    {
        RoleEntity role = await SeedAsync<IdentityDbContext, RoleEntity>(ctx =>
        {
            RoleEntity entity = RoleFactory.Create(ShortName("fb"), "Visitor should not access.");
            ctx.Roles.Add(entity);
            return entity;
        });

        Client.AuthenticateAsVisitor();

        var response = await Client.GetAsync($"{ApiRoutes.Admin.Roles}/{role.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
