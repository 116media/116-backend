using _116.Identity.Domain.Enums;
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

    /// <summary>
    /// Verifies that submitting an expired OTP returns 410 Gone.
    /// Covers the OtpExpirationExceptionHandler path.
    /// </summary>
    [Fact]
    public async Task VerifyOtp_WithExpiredOtp_ReturnsGone()
    {
        Client.ClearAuthentication();

        var email = $"admin-expired-{Guid.NewGuid():N}@test.com";
        var adminRole = RoleFactory.CreateAdmin();
        var user = UserFactory.Create(email);
        user.Activate();

        var userRole = UserRoleFactory.Create(user.Id, adminRole.Id);
        var otp = OtpFactory.CreateExpired(user.Id, Otp.ValidCode, EnumOtpPurpose.EmailVerification);

        await using var seedContext = CreateDbContext<IdentityDbContext>();
        seedContext.Roles.Add(adminRole);
        seedContext.Users.Add(user);
        seedContext.UserRoles.Add(userRole);
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

        var email = $"admin-maxotp-{Guid.NewGuid():N}@test.com";
        var adminRole = RoleFactory.CreateAdmin();
        var user = UserFactory.Create(email);
        user.Activate();

        var userRole = UserRoleFactory.Create(user.Id, adminRole.Id);
        var otp = OtpFactory.CreateMaxAttemptsReached(user.Id, Otp.ValidCode, EnumOtpPurpose.EmailVerification);

        await using var seedContext = CreateDbContext<IdentityDbContext>();
        seedContext.Roles.Add(adminRole);
        seedContext.Users.Add(user);
        seedContext.UserRoles.Add(userRole);
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
