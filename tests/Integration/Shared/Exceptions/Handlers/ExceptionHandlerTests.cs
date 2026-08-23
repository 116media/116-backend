using _116.Identity.Application.Shared.Errors.Messages;
using _116.Shared.Application.Exceptions;
using _116.Shared.Application.Exceptions.Messages;
using FluentValidation;
using FluentValidation.Results;

namespace _116.Integration.Tests.Shared.Exceptions.Handlers;

/// <summary>
/// Verifies that all exception handler strategies return correct HTTP status codes
/// and ProblemDetails structure through the full API pipeline.
/// </summary>
[Collection("Database")]
public class ExceptionHandlerTests(PostgresFixture db) : BaseApiTest(db)
{
    private static string ValidationDetail(params (string Property, string Message)[] failures) =>
        new ValidationException(failures.Select(f => new ValidationFailure(f.Property, f.Message))).Message;

    [Fact]
    public async Task AuthenticationException_ShouldReturn401_WhenNoToken()
    {
        Client.ClearAuthentication();

        var response = await Client.GetAsync(ApiRoutes.Admin.Roles);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task AuthorizationException_ShouldReturn403_WhenWrongRole()
    {
        Client.AuthenticateAsVisitor();

        var response = await Client.PostAsJsonAsync(
            ApiRoutes.Admin.Roles,
            new { Name = "ForbiddenRole", Description = "Should be forbidden" }
        );

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task NotFoundExceptionHandler_ShouldReturn404_WhenEntityNotFound()
    {
        Client.AuthenticateAsSuperAdmin();

        Guid nonExistentId = Guid.NewGuid();
        var response = await Client.GetAsync($"{ApiRoutes.Admin.Roles}/{nonExistentId}");

        await response.ShouldBeProblem<NotFoundException>(
            HttpStatusCode.NotFound,
            Localized<SharedExceptionMessage>(m => m.EntityNotFound("Role"))
        );
    }

    [Fact]
    public async Task NotFoundExceptionHandler_ShouldReturnProblemDetails()
    {
        Client.AuthenticateAsSuperAdmin();

        Guid nonExistentId = Guid.NewGuid();
        var response = await Client.GetAsync($"{ApiRoutes.Admin.Roles}/{nonExistentId}");

        await response.ShouldBeProblem<NotFoundException>(
            HttpStatusCode.NotFound,
            Localized<SharedExceptionMessage>(m => m.EntityNotFound("Role"))
        );
    }

    [Fact]
    public async Task ValidationExceptionHandler_ShouldReturn400_WhenValidationFails()
    {
        Client.AuthenticateAsSuperAdmin();

        var response = await Client.PostAsJsonAsync(ApiRoutes.Admin.Roles, new { Name = "", Description = "" });

        await response.ShouldBeProblem<ValidationException>(
            HttpStatusCode.BadRequest,
            ValidationDetail(
                ("Name", Localized<ValidationErrorMessage>(m => m.RoleNameRequired())),
                ("Description", Localized<ValidationErrorMessage>(m => m.RoleDescriptionRequired()))
            )
        );
    }

    [Fact]
    public async Task ValidationExceptionHandler_ShouldReturnProblemDetails()
    {
        Client.AuthenticateAsSuperAdmin();

        var response = await Client.PostAsJsonAsync(ApiRoutes.Admin.Roles, new { Name = "", Description = "" });

        await response.ShouldBeProblem<ValidationException>(
            HttpStatusCode.BadRequest,
            ValidationDetail(
                ("Name", Localized<ValidationErrorMessage>(m => m.RoleNameRequired())),
                ("Description", Localized<ValidationErrorMessage>(m => m.RoleDescriptionRequired()))
            )
        );
    }

    [Fact]
    public async Task ResourceNotFoundExceptionHandler_ShouldReturn404_ForNonExistentRoute()
    {
        var response = await Client.GetAsync($"{ApiRoutes.Public.Base}/nonexistent-resource");

        await response.ShouldBeProblem<ResourceNotFoundException>(
            HttpStatusCode.NotFound,
            Localized<SharedExceptionMessage>(m => m.ResourceNotFound())
        );
    }

    [Fact]
    public async Task ResourceNotFoundExceptionHandler_ShouldReturnProblemDetails_ForNonExistentRoute()
    {
        var response = await Client.GetAsync($"{ApiRoutes.Public.Base}/nonexistent-resource");

        await response.ShouldBeProblem<ResourceNotFoundException>(
            HttpStatusCode.NotFound,
            Localized<SharedExceptionMessage>(m => m.ResourceNotFound())
        );
    }

    [Fact]
    public async Task MethodNotAllowedExceptionHandler_ShouldReturn405_ForWrongMethod()
    {
        var response = await Client.DeleteAsync(Routes.Public.Auth.Login());

        response.StatusCode.Should().Be(HttpStatusCode.MethodNotAllowed);
    }

    [Fact]
    public async Task ResourceNotFoundExceptionHandler_ShouldReturn404_ForInvalidUuidOnGuidConstrainedRoute()
    {
        Client.AuthenticateAsSuperAdmin();

        var response = await Client.GetAsync($"{ApiRoutes.Admin.Roles}/not-a-uuid");

        await response.ShouldBeProblem<ResourceNotFoundException>(
            HttpStatusCode.NotFound,
            Localized<SharedExceptionMessage>(m => m.ResourceNotFound())
        );
    }

    [Fact]
    public async Task ConflictExceptionHandler_ShouldReturn409_WhenDuplicateCreated()
    {
        Client.AuthenticateAsSuperAdmin();

        var rolePayload = new { Name = "ConflictTestRole", Description = "First creation" };

        await Client.PostAsJsonAsync(ApiRoutes.Admin.Roles, rolePayload);

        var response = await Client.PostAsJsonAsync(ApiRoutes.Admin.Roles, rolePayload);

        await response.ShouldBeProblem<ConflictException>(
            HttpStatusCode.Conflict,
            Localized<ConflictErrorMessage>(m => m.RoleAlreadyExists("ConflictTestRole"))
        );
    }
}
