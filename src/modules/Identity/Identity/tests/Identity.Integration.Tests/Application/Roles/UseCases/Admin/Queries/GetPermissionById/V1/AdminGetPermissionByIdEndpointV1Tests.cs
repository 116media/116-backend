using _116.BuildingBlocks.Application.Exceptions;
using _116.BuildingBlocks.Application.Exceptions.Messages;
using _116.Identity.Application.Roles.UseCases.Admin.Queries.GetPermissionById.V1;
using _116.Identity.Domain.Entities;
using _116.Identity.Infrastructure.Persistence;
using _116.Identity.TestData.Factories;
using _116.Identity.TestData.Mocks.Infrastructure;
using _116.Identity.TestData.Mocks.Repositories;
using _116.Identity.TestData.Mocks.Services;
using _116.Storage.TestData.Factories;
using _116.Storage.TestData.Mocks.Infrastructure;
using _116.Storage.TestData.Mocks.Services;
using _116.Tests.TestData.Mocks;

namespace _116.Identity.Integration.Tests.Application.Roles.UseCases.Admin.Queries.GetPermissionById.V1;

/// <summary>
/// Integration tests for the AdminGetPermissionById endpoint.
/// </summary>
[Collection("Database")]
public class AdminGetPermissionByIdEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
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
    public async Task GetPermissionById_ShouldReturn200_WhenAdminAndExists()
    {
        string resource = UniqueResource("gi");
        string action = UniqueAction("gi");
        PermissionEntity permission = await SeedAsync<IdentityDbContext, PermissionEntity>(ctx =>
        {
            PermissionEntity entity = PermissionFactory.Create(resource, action, "Seeded for get by id");
            ctx.Permissions.Add(entity);
            return entity;
        });

        Client.AuthenticateAsAdmin();

        var response = await Client.GetAsync($"{ApiRoutes.Admin.Permissions}/{permission.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.ReadAsAsync<AdminGetPermissionByIdResponse>();
        body.Permission.Id.Should().Be(permission.Id);
        body.Permission.Resource.Should().Be(resource);
        body.Permission.Action.Should().Be(action);
    }

    [Fact]
    public async Task GetPermissionById_ShouldReturn404_WhenNonExistentGuid()
    {
        Client.AuthenticateAsAdmin();

        Guid nonExistentId = Guid.NewGuid();

        var response = await Client.GetAsync($"{ApiRoutes.Admin.Permissions}/{nonExistentId}");

        await response.ShouldBeProblem<NotFoundException>(
            HttpStatusCode.NotFound,
            Localized<SharedExceptionMessage>(m => m.EntityNotFound("Permission"))
        );
    }
}
