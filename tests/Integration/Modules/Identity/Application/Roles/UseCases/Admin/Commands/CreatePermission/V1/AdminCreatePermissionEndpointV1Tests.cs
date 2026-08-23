using _116.Identity.Application.Roles.UseCases.Admin.Commands.CreatePermission.V1;
using _116.Identity.Application.Shared.Errors.Messages;
using _116.Identity.Domain.Entities;
using _116.Identity.Infrastructure.Persistence;
using _116.Shared.Application.Exceptions;
using _116.Tests.Fixtures.Builders.Requests.Identity;
using FluentValidation;
using FluentValidation.Results;

namespace _116.Integration.Tests.Modules.Identity.Application.Roles.UseCases.Admin.Commands.CreatePermission.V1;

/// <summary>
/// Integration tests for the AdminCreatePermission endpoint.
/// </summary>
[Collection("Database")]
public class AdminCreatePermissionEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
{
    private static string ValidationDetail(string property, string message) =>
        new ValidationException([new ValidationFailure(property, message)]).Message;

    [Fact]
    public async Task CreatePermission_ShouldReturnSuccess_WhenSuperAdminWithValidData()
    {
        Client.AuthenticateAsSuperAdmin();

        AdminCreatePermissionRequest request = new AdminCreatePermissionRequestBuilder().Build();

        var response = await Client.PostAsJsonAsync(ApiRoutes.Admin.Permissions, request);

        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var body = await response.ReadAsAsync<AdminCreatePermissionResponse>();
        body.Permission.Id.Should().NotBeEmpty();
        body.Permission.Resource.Should().Be(request.Resource);
        body.Permission.Action.Should().Be(request.Action);
        body.Permission.Description.Should().Be(request.Description);

        await using IdentityDbContext context = CreateDbContext<IdentityDbContext>();
        PermissionEntity? permission = await context.Permissions.FirstOrDefaultAsync(p => p.Id == body.Permission.Id);
        permission.Should().NotBeNull();
        permission!.Resource.Should().Be(request.Resource);
        permission.Action.Should().Be(request.Action);
    }

    [Fact]
    public async Task CreatePermission_ShouldReturn403_WhenAdmin()
    {
        Client.AuthenticateAsAdmin();

        AdminCreatePermissionRequest request = new AdminCreatePermissionRequestBuilder().Build();

        var response = await Client.PostAsJsonAsync(ApiRoutes.Admin.Permissions, request);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task CreatePermission_ShouldReturn401_WhenNoAuth()
    {
        Client.ClearAuthentication();

        AdminCreatePermissionRequest request = new AdminCreatePermissionRequestBuilder().Build();

        var response = await Client.PostAsJsonAsync(ApiRoutes.Admin.Permissions, request);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task CreatePermission_ShouldReturn409_WhenDuplicateResourceAction()
    {
        Client.AuthenticateAsSuperAdmin();

        AdminCreatePermissionRequest request = new AdminCreatePermissionRequestBuilder().Build();
        await Client.PostAsJsonAsync(ApiRoutes.Admin.Permissions, request);

        AdminCreatePermissionRequest duplicateRequest = new AdminCreatePermissionRequestBuilder()
            .WithResource(request.Resource)
            .WithAction(request.Action)
            .Build();

        var response = await Client.PostAsJsonAsync(ApiRoutes.Admin.Permissions, duplicateRequest);

        await response.ShouldBeProblem<ConflictException>(
            HttpStatusCode.Conflict,
            Localized<ConflictErrorMessage>(m => m.PermissionAlreadyExists(request.Resource, request.Action))
        );
    }

    [Fact]
    public async Task CreatePermission_ShouldReturnBadRequest_WhenResourceIsEmpty()
    {
        Client.AuthenticateAsSuperAdmin();

        AdminCreatePermissionRequest request = new AdminCreatePermissionRequestBuilder()
            .WithResource(string.Empty)
            .Build();

        var response = await Client.PostAsJsonAsync(ApiRoutes.Admin.Permissions, request);

        await response.ShouldBeProblem<ValidationException>(
            HttpStatusCode.BadRequest,
            ValidationDetail("Resource", Localized<ValidationErrorMessage>(m => m.PermissionResourceRequired()))
        );
    }
}
