namespace _116.Integration.Tests.Modules.Identity.Application.Auth.UseCases.Public.Commands.ResendOtp.V1;

/// <summary>
/// Integration tests for the PublicResendOtp endpoint.
/// </summary>
[Collection("Database")]
public class PublicResendOtpEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
{
    [Fact]
    public async Task ResendOtp_WithEmptyEmail_ReturnsValidationError()
    {
        Client.ClearAuthentication();
        var request = new { Email = "", Purpose = "EmailVerification" };

        var response = await Client.PostAsJsonAsync($"{ApiRoutes.Public.Auth}/resend-otp", request);

        response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.UnprocessableEntity);
    }

    [Fact]
    public async Task ResendOtp_WithNonExistentEmail_ReturnsOk()
    {
        Client.ClearAuthentication();
        var request = new { Email = "nonexistent@test.com", Purpose = "EmailVerification" };

        var response = await Client.PostAsJsonAsync($"{ApiRoutes.Public.Auth}/resend-otp", request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
