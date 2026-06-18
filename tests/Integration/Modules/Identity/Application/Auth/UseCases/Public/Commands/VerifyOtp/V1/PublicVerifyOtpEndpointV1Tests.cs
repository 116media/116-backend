using _116.Identity.Application.Auth.UseCases.Public.Commands.VerifyOtp.V1;
using _116.Identity.Domain.Enums;
using _116.Identity.Infrastructure.Persistence;
using _116.Tests.Fixtures.Builders.Requests.Identity;
using _116.Tests.Fixtures.Factories.Identity;

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
        var request = new PublicVerifyOtpRequestBuilder().WithEmail(string.Empty).Build();

        var response = await Client.PostAsJsonAsync(Routes.Public.Auth.VerifyOtp(), request);

        await response.ShouldBeProblem(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task VerifyOtp_WithInvalidOtp_ReturnsError()
    {
        Client.ClearAuthentication();
        var request = new PublicVerifyOtpRequestBuilder()
            .WithEmail("nonexistent@test.com")
            .WithCode(Otp.InvalidCode)
            .Build();

        var response = await Client.PostAsJsonAsync(Routes.Public.Auth.VerifyOtp(), request);

        await response.ShouldBeProblem(HttpStatusCode.NotFound);
    }

    /// <summary>
    /// Verifies that submitting a valid, unexpired OTP marks the account as verified and
    /// consumes the OTP in the database.
    /// </summary>
    [Fact]
    public async Task VerifyOtp_WithValidOtp_MarksUserVerifiedAndConsumesOtp()
    {
        Client.ClearAuthentication();

        var email = $"pub-verify-ok-{Guid.NewGuid():N}@test.com";
        var user = UserFactory.Create(email);
        user.Activate();

        var otp = OtpFactory.Create(user.Id, Otp.ValidCode, EnumOtpPurpose.EmailVerification);

        await using var seedContext = CreateDbContext<IdentityDbContext>();
        seedContext.Users.Add(user);
        seedContext.Otps.Add(otp);
        await seedContext.SaveChangesAsync();

        var request = new PublicVerifyOtpRequestBuilder()
            .WithEmail(email)
            .WithCode(Otp.ValidCode)
            .WithPurpose(nameof(EnumOtpPurpose.EmailVerification))
            .Build();

        var response = await Client.PostAsJsonAsync(Routes.Public.Auth.VerifyOtp(), request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        PublicVerifyOtpResponse body = await response.ReadAsAsync<PublicVerifyOtpResponse>();
        body.IsSuccess.Should().BeTrue();

        await using var verifyContext = CreateDbContext<IdentityDbContext>();
        var verifiedUser = await verifyContext.Users.FirstAsync(u => u.Id == user.Id);
        verifiedUser.IsVerified.Should().BeTrue();

        var consumedOtp = await verifyContext.Otps.FirstAsync(o => o.Id == otp.Id);
        consumedOtp.IsUsed.Should().BeTrue();
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

        var request = new PublicVerifyOtpRequestBuilder()
            .WithEmail(email)
            .WithCode(Otp.ValidCode)
            .WithPurpose(nameof(EnumOtpPurpose.EmailVerification))
            .Build();

        var response = await Client.PostAsJsonAsync(Routes.Public.Auth.VerifyOtp(), request);

        await response.ShouldBeProblem(HttpStatusCode.Gone);
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

        var request = new PublicVerifyOtpRequestBuilder()
            .WithEmail(email)
            .WithCode(Otp.ValidCode)
            .WithPurpose(nameof(EnumOtpPurpose.EmailVerification))
            .Build();

        var response = await Client.PostAsJsonAsync(Routes.Public.Auth.VerifyOtp(), request);

        await response.ShouldBeProblem(HttpStatusCode.TooManyRequests);
    }
}
