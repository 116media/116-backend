using _116.Identity.Infrastructure.Persistence;
using _116.Tests.Fixtures.Factories.Identity;

namespace _116.Integration.Tests.Modules.Identity.Application.Auth.UseCases.Admin.Commands.VerifyOtp.V1;

/// <summary>
/// Integration tests for the AdminVerifyOtp endpoint.
/// </summary>
[Collection("Database")]
public class AdminVerifyOtpEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
{
    private const string AuthUrl = ApiRoutes.Admin.Auth;

    [Fact]
    public async Task VerifyOtp_WithEmptyFields_ReturnsValidationError()
    {
        Client.ClearAuthentication();
        var request = new
        {
            Email = "",
            Code = "",
            Purpose = "",
        };

        var response = await Client.PostAsJsonAsync($"{AuthUrl}/verify-otp", request);

        response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.UnprocessableEntity);
    }
}
