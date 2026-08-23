using _116.Identity.Application.Roles.UseCases.Admin.Queries.GetAllRoles.V1;
using _116.Shared.Application.Exceptions;
using _116.Shared.Application.Exceptions.Messages;

namespace _116.Integration.Tests.Shared.Application.Decorators;

/// <summary>
/// Verifies that the LoggingDecorator does not interfere with
/// request/response flow and that failed commands still return error responses.
/// </summary>
[Collection("Database")]
public class LoggingDecoratorTests(PostgresFixture db) : BaseApiTest(db)
{
    [Fact]
    public async Task Command_ShouldCompleteWithoutLoggingErrors()
    {
        Client.AuthenticateAsSuperAdmin();

        var response = await Client.GetAsync(ApiRoutes.Admin.Roles);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        AdminGetAllRolesResponse body = await response.ReadAsAsync<AdminGetAllRolesResponse>();
        body.Roles.Should().NotBeNull("the logged query should still flow through to the handler and return data");
    }

    [Fact]
    public async Task FailedCommand_ShouldStillReturnErrorResponse()
    {
        Client.AuthenticateAsSuperAdmin();

        Guid nonExistentId = Guid.NewGuid();
        var response = await Client.GetAsync($"{ApiRoutes.Admin.Roles}/{nonExistentId}");

        await response.ShouldBeProblem<NotFoundException>(
            HttpStatusCode.NotFound,
            Localized<SharedExceptionMessage>(m => m.EntityNotFound("Role"))
        );
    }
}
