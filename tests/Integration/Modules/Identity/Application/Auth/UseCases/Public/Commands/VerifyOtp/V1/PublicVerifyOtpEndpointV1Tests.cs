using _116.Identity.Domain.Enums;
using _116.Identity.Infrastructure.Persistence;
using _116.Tests.Fixtures.Factories.Identity;

namespace _116.Integration.Tests.Modules.Identity.Application.Auth.UseCases.Public.Commands.VerifyOtp.V1;

/// <summary>
/// Integration tests for the PublicVerifyOtp endpoint.
/// </summary>
[Collection("Database")]
public class PublicVerifyOtpEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
{
    private const string AuthUrl = ApiRoutes.Public.Auth;

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

        var response = await Client.PostAsJsonAsync($"{AuthUrl}/verify-otp", request);

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

        var response = await Client.PostAsJsonAsync($"{AuthUrl}/verify-otp", request);

        response
            .StatusCode.Should()
            .BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.NotFound, HttpStatusCode.UnprocessableEntity);
    }

    /// <summary>
    /// Verifies that submitting an expired OTP returns 410 Gone.
    /// Covers the OtpExpirationExceptionHandler path.
    /// </summary>
    [Fact]
    public async Task VerifyOtp_WithExpiredOtp_ReturnsGone()
    {
        Client.ClearAuthentication();

        var email = $"pub-expired-{Guid.NewGuid():N}@test.com";
        var user = UserFactory.Create(email);
        user.Activate();

        var otp = OtpFactory.CreateExpired(user.Id, Otp.ValidCode, EnumOtpPurpose.EmailVerification);

        await using var seedContext = CreateDbContext<IdentityDbContext>();
        seedContext.Users.Add(user);
        seedContext.Otps.Add(otp);
        await seedContext.SaveChangesAsync();

        var request = new
        {
            Email = email,
            Code = Otp.ValidCode,
            Purpose = nameof(EnumOtpPurpose.EmailVerification),
        };

        var response = await Client.PostAsJsonAsync($"{AuthUrl}/verify-otp", request);

        response.StatusCode.Should().Be(HttpStatusCode.Gone);
    }

    /// <summary>
    /// Verifies that submitting an OTP after maximum attempts have been reached returns 429 TooManyRequests.
    /// Covers the OtpAttemptsLimitExceptionHandler path.
    /// </summary>
    [Fact]
    public async Task VerifyOtp_WithMaxAttemptsReached_ReturnsTooManyRequests()
    {
        Client.ClearAuthentication();

        var email = $"pub-maxotp-{Guid.NewGuid():N}@test.com";
        var user = UserFactory.Create(email);
        user.Activate();

        var otp = OtpFactory.CreateMaxAttemptsReached(user.Id, Otp.ValidCode, EnumOtpPurpose.EmailVerification);

        await using var seedContext = CreateDbContext<IdentityDbContext>();
        seedContext.Users.Add(user);
        seedContext.Otps.Add(otp);
        await seedContext.SaveChangesAsync();

        var request = new
        {
            Email = email,
            Code = Otp.ValidCode,
            Purpose = nameof(EnumOtpPurpose.EmailVerification),
        };

        var response = await Client.PostAsJsonAsync($"{AuthUrl}/verify-otp", request);

        response.StatusCode.Should().Be(HttpStatusCode.TooManyRequests);
    }
}
