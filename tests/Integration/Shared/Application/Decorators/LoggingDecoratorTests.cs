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

        response.Should().NotBeNull();
        response.StatusCode.Should().NotBe(HttpStatusCode.InternalServerError);
    }

    [Fact]
    public async Task FailedCommand_ShouldStillReturnErrorResponse()
    {
        Client.AuthenticateAsSuperAdmin();

        var nonExistentId = Guid.NewGuid();
        var response = await Client.GetAsync($"{ApiRoutes.Admin.Roles}/{nonExistentId}");

        response.StatusCode.Should().BeOneOf(HttpStatusCode.NotFound, HttpStatusCode.BadRequest);
    }
}
