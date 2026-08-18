using _116.Tests.Fixtures.Builders.Requests.Identity;

namespace _116.Integration.Tests.Shared.Application.Configurations;

/// <summary>
/// Verifies that a host started without <c>OTP_PEPPER</c> refuses to run an OTP flow rather than
/// falling back to an unkeyed hash, which would leave stored codes recoverable from a dump.
/// </summary>
/// <remarks>
/// These tests run against <see cref="OtpPepperlessPostgresFixture" />, the only host booted with
/// the variable cleared. The key is read when the OTP service is resolved, so any request that
/// reaches the service fails; the endpoint chosen here is anonymous, keeping the assertion about
/// configuration rather than authentication.
/// </remarks>
/// <param name="db">The dedicated Testcontainer database and pepperless application host.</param>
[Collection("OtpPepperless")]
public class OtpPepperFailClosedTests(OtpPepperlessPostgresFixture db) : IDisposable
{
    private readonly HttpClient _client = db.Api.CreateClient();

    /// <inheritdoc />
    public void Dispose()
    {
        _client.Dispose();
        GC.SuppressFinalize(this);
    }

    [Fact]
    public async Task ForgotPassword_WithoutAConfiguredPepper_FailsInsteadOfHashingUnkeyed()
    {
        // Arrange
        var request = new PublicForgotPasswordRequestBuilder().WithEmail(TestUser.VisitorEmail).Build();

        // Act
        using HttpResponseMessage response = await _client.PostAsJsonAsync(
            Routes.Public.Auth.ForgotPassword(),
            request
        );

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
    }
}
