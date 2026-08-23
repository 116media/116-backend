using _116.Identity.Application.Roles.UseCases.Admin.Commands.CreateRole.V1;
using _116.Identity.Application.Shared.Errors.Messages;
using _116.Identity.Domain.Entities;
using _116.Identity.Infrastructure.Persistence;
using _116.Shared.Application.Exceptions;
using _116.Tests.Fixtures.Builders.Requests.Identity;
using _116.Tests.Fixtures.Factories.Identity;
using FluentValidation;
using FluentValidation.Results;

namespace _116.Integration.Tests.Modules.Identity.Application.Roles.UseCases.Admin.Commands.CreateRole.V1;

/// <summary>
/// Integration tests for the AdminCreateRole endpoint.
/// </summary>
[Collection("Database")]
public class AdminCreateRoleEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
{
    private static string ShortName(string prefix = "r") => $"{prefix}{Guid.NewGuid().ToString("N")[..8]}";

    private static string ValidationDetail(string property, string message) =>
        new ValidationException([new ValidationFailure(property, message)]).Message;

    [Fact]
    public async Task CreateRole_AsSuperAdmin_WithValidData_ReturnsSuccess()
    {
        Client.AuthenticateAsSuperAdmin();
        AdminCreateRoleRequest request = new AdminCreateRoleRequestBuilder().Build();

        var response = await Client.PostAsJsonAsync(ApiRoutes.Admin.Roles, request);

        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var body = await response.ReadAsAsync<AdminCreateRoleResponse>();
        body.Role.Id.Should().NotBeEmpty();
        body.Role.Name.Should().Be(request.Name);
        body.Role.Description.Should().Be(request.Description);

        await using IdentityDbContext context = CreateDbContext<IdentityDbContext>();
        RoleEntity? role = await context.Roles.FirstOrDefaultAsync(r => r.Id == body.Role.Id);
        role.Should().NotBeNull();
        role!.Name.Should().Be(request.Name);
        role.Description.Should().Be(request.Description);
    }

    [Fact]
    public async Task CreateRole_AsAdmin_ReturnsForbidden()
    {
        Client.AuthenticateAsAdmin();
        AdminCreateRoleRequest request = new AdminCreateRoleRequestBuilder().Build();

        var response = await Client.PostAsJsonAsync(ApiRoutes.Admin.Roles, request);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task CreateRole_WithNoAuth_ReturnsUnauthorized()
    {
        Client.ClearAuthentication();
        AdminCreateRoleRequest request = new AdminCreateRoleRequestBuilder().Build();

        var response = await Client.PostAsJsonAsync(ApiRoutes.Admin.Roles, request);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task CreateRole_WithDuplicateName_ReturnsConflict()
    {
        string duplicateName = ShortName("dup");

        await SeedAsync<IdentityDbContext>(ctx =>
        {
            RoleEntity existingRole = RoleFactory.Create(duplicateName, "Already exists");
            ctx.Roles.Add(existingRole);
        });

        Client.AuthenticateAsSuperAdmin();
        AdminCreateRoleRequest request = new AdminCreateRoleRequestBuilder().WithName(duplicateName).Build();

        var response = await Client.PostAsJsonAsync(ApiRoutes.Admin.Roles, request);

        await response.ShouldBeProblem<ConflictException>(
            HttpStatusCode.Conflict,
            Localized<ConflictErrorMessage>(m => m.RoleAlreadyExists(duplicateName))
        );
    }

    [Fact]
    public async Task CreateRole_WithEmptyName_ReturnsValidationError()
    {
        Client.AuthenticateAsSuperAdmin();
        AdminCreateRoleRequest request = new AdminCreateRoleRequestBuilder().WithName(string.Empty).Build();

        var response = await Client.PostAsJsonAsync(ApiRoutes.Admin.Roles, request);

        await response.ShouldBeProblem<ValidationException>(
            HttpStatusCode.BadRequest,
            ValidationDetail("Name", Localized<ValidationErrorMessage>(m => m.RoleNameRequired()))
        );
    }
}
