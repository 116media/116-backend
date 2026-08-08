using _116.Identity.Application.Auth.UseCases.Public.Commands.ResendOtp.V1;
using _116.Identity.Application.Shared.Errors.Messages;
using _116.Tests.Fixtures.Builders.Requests.Identity;
using FluentValidation;
using FluentValidation.Results;

namespace _116.Integration.Tests.Modules.Identity.Application.Auth.UseCases.Public.Commands.ResendOtp.V1;

/// <summary>
/// Integration tests for the PublicResendOtp endpoint.
/// </summary>
[Collection("Database")]
public class PublicResendOtpEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
{
    private static string ValidationDetail(string property, string message) =>
        new ValidationException([new ValidationFailure(property, message)]).Message;

    [Fact]
    public async Task ResendOtp_WithEmptyEmail_ReturnsValidationError()
    {
        Client.ClearAuthentication();
        var request = new PublicResendOtpRequestBuilder().WithEmail(string.Empty).Build();

        var response = await Client.PostAsJsonAsync(Routes.Public.Auth.ResendOtp(), request);

        await response.ShouldBeProblem<ValidationException>(
            HttpStatusCode.BadRequest,
            ValidationDetail("Email", Localized<ValidationErrorMessage>(m => m.EmailRequired()))
        );
    }

    [Fact]
    public async Task ResendOtp_WithNonExistentEmail_ReturnsOk()
    {
        Client.ClearAuthentication();
        var request = new PublicResendOtpRequestBuilder().WithEmail("nonexistent@test.com").Build();

        var response = await Client.PostAsJsonAsync(Routes.Public.Auth.ResendOtp(), request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        PublicResendOtpResponse body = await response.ReadAsAsync<PublicResendOtpResponse>();
        body.IsSuccess.Should().BeTrue();
    }
}
