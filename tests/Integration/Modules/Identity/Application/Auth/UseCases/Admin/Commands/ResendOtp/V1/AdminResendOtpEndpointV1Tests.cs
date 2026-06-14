using _116.Identity.Infrastructure.Persistence;
using _116.Tests.Fixtures.Factories.Identity;

namespace _116.Integration.Tests.Modules.Identity.Application.Auth.UseCases.Admin.Commands.ResendOtp.V1;

/// <summary>
/// Integration tests for the AdminResendOtp endpoint.
/// </summary>
[Collection("Database")]
public class AdminResendOtpEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
{
    private const string AuthUrl = ApiRoutes.Admin.Auth;

    [Fact]
    public async Task ResendOtp_WithEmptyEmail_ReturnsValidationError()
    {
        Client.ClearAuthentication();
        var request = new { Email = "", Purpose = "EmailVerification" };

        var response = await Client.PostAsJsonAsync($"{AuthUrl}/resend-otp", request);

        response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.UnprocessableEntity);
    }
}
