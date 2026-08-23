using _116.Identity.Application.Auth.UseCases.Public.Commands.ForgotPassword.V1;
using _116.Identity.Application.Shared.Errors.Messages;
using _116.Tests.Fixtures.Builders.Requests.Identity;
using FluentValidation;
using FluentValidation.Results;

namespace _116.Integration.Tests.Modules.Identity.Application.Auth.UseCases.Public.Commands.ForgotPassword.V1;

/// <summary>
/// Integration tests for the PublicForgotPassword endpoint.
/// </summary>
[Collection("Database")]
public class PublicForgotPasswordEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
{
    private static string ValidationDetail(string property, string message) =>
        new ValidationException([new ValidationFailure(property, message)]).Message;

    [Fact]
    public async Task ForgotPassword_WithInvalidEmail_ReturnsValidationError()
    {
        Client.ClearAuthentication();
        var request = new PublicForgotPasswordRequestBuilder().WithEmail("not-an-email").Build();

        var response = await Client.PostAsJsonAsync(Routes.Public.Auth.ForgotPassword(), request);

        await response.ShouldBeProblem<ValidationException>(
            HttpStatusCode.BadRequest,
            ValidationDetail("Email", Localized<ValidationErrorMessage>(m => m.InvalidEmailFormatMsg()))
        );
    }

    [Fact]
    public async Task ForgotPassword_WithValidEmail_ReturnsSuccessEchoingEmail()
    {
        Client.ClearAuthentication();
        var email = $"forgot-{Guid.NewGuid():N}@test.com";
        var request = new PublicForgotPasswordRequestBuilder().WithEmail(email).Build();

        var response = await Client.PostAsJsonAsync(Routes.Public.Auth.ForgotPassword(), request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        PublicForgotPasswordResponse body = await response.ReadAsAsync<PublicForgotPasswordResponse>();
        body.IsSuccess.Should().BeTrue();
        body.Email.Should().Be(request.Email);
    }
}
