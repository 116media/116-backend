namespace _116.Integration.Tests.Modules.Identity.Application.Auth.UseCases.Public.Commands.VerifyOtp.V1;

/// <summary>
/// Integration tests for the PublicVerifyOtp endpoint.
/// </summary>
[Collection("Database")]
public class PublicVerifyOtpEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
{
    [Fact]
    public async Task VerifyOtp_WithEmptyEmail_ReturnsValidationError()
    {
        Client.ClearAuthentication();
        var request = new
        {
            Email = "",
            Otp = "123456",
            Purpose = "EmailVerification",
        };

        var response = await Client.PostAsJsonAsync($"{ApiRoutes.Public.Auth}/verify-otp", request);

        response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.UnprocessableEntity);
    }

    [Fact]
    public async Task VerifyOtp_WithInvalidOtp_ReturnsError()
    {
        Client.ClearAuthentication();
        var request = new
        {
            Email = "nonexistent@test.com",
            Otp = "000000",
            Purpose = "EmailVerification",
        };

        var response = await Client.PostAsJsonAsync($"{ApiRoutes.Public.Auth}/verify-otp", request);

        response
            .StatusCode.Should()
            .BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.NotFound, HttpStatusCode.UnprocessableEntity);
    }
}
