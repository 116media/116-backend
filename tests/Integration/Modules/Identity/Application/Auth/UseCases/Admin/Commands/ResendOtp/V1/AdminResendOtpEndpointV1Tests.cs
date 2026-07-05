using _116.Identity.Application.Auth.Constants;
using _116.Identity.Application.Auth.UseCases.Admin.Commands.ResendOtp.V1;
using _116.Tests.Fixtures.Builders.Requests.Identity;

namespace _116.Integration.Tests.Modules.Identity.Application.Auth.UseCases.Admin.Commands.ResendOtp.V1;

/// <summary>
/// Integration tests for the AdminResendOtp endpoint.
/// </summary>
[Collection("Database")]
public class AdminResendOtpEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
{
    private const string AuthUrl = ApiRoutes.Admin.Auth;
    private const string ResendOtpUrl = $"{AuthUrl}/{AuthRouteConstants.ResendOtp}";

    [Fact]
    public async Task ResendOtp_WithEmptyEmail_ReturnsValidationError()
    {
        Client.ClearAuthentication();
        var request = new AdminResendOtpRequestBuilder().WithEmail(string.Empty).Build();

        var response = await Client.PostAsJsonAsync(ResendOtpUrl, request);

        await response.ShouldBeProblem(HttpStatusCode.BadRequest);
    }

    /// <summary>
    /// Verifies that requesting an OTP resend for a non-existent email returns 200 OK with a
    /// success payload. The endpoint reports success to avoid leaking account existence.
    /// </summary>
    [Fact]
    public async Task ResendOtp_WithNonExistentEmail_ReturnsOk()
    {
        Client.ClearAuthentication();
        var request = new AdminResendOtpRequestBuilder().WithEmail($"no-admin-{Guid.NewGuid():N}@test.com").Build();

        var response = await Client.PostAsJsonAsync(ResendOtpUrl, request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        AdminResendOtpResponse body = await response.ReadAsAsync<AdminResendOtpResponse>();
        body.IsSuccess.Should().BeTrue();
    }
}
