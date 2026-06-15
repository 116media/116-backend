using System.Text.Json;
using _116.Identity.Domain.Enums;
using _116.Identity.Infrastructure.Persistence;
using _116.Tests.Fixtures.Factories.Identity;

namespace _116.Integration.Tests.Modules.Identity.Application.Auth.UseCases.Public.Commands.ResetPassword.V1;

/// <summary>
/// Integration tests for the PublicResetPassword endpoint.
/// </summary>
[Collection("Database")]
public class PublicResetPasswordEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
{
    private const string ResetPasswordUrl = $"{ApiRoutes.Public.Auth}/reset-password";

    /// <summary>
    /// Verifies that a valid used OTP with matching code successfully resets the password.
    /// </summary>
    [Fact]
    public async Task ResetPassword_WithValidOtp_ReturnsOk()
    {
        await using var seedContext = CreateDbContext<IdentityDbContext>();

        var userId = Guid.NewGuid();
        var user = UserFactory.CreateWithId(userId, "reset-valid@test.com");
        user.MarkAsVerified();
        user.Activate();

        var otp = OtpFactory.CreateUsed(userId, Otp.ValidCode, EnumOtpPurpose.PasswordReset);

        seedContext.Users.Add(user);
        seedContext.Otps.Add(otp);
        await seedContext.SaveChangesAsync();

        Client.ClearAuthentication();

        var request = new
        {
            Email = "reset-valid@test.com",
            Code = Otp.ValidCode,
            NewPassword = "NewSecure123!abc",
        };

        var response = await Client.PostAsJsonAsync(ResetPasswordUrl, request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    /// <summary>
    /// Verifies that providing an incorrect OTP code returns a 400 Bad Request.
    /// </summary>
    [Fact]
    public async Task ResetPassword_WithInvalidOtpCode_ReturnsBadRequest()
    {
        await using var seedContext = CreateDbContext<IdentityDbContext>();

        var userId = Guid.NewGuid();
        var user = UserFactory.CreateWithId(userId, "reset-invalid@test.com");
        user.MarkAsVerified();
        user.Activate();

        var otp = OtpFactory.CreateUsed(userId, Otp.ValidCode, EnumOtpPurpose.PasswordReset);

        seedContext.Users.Add(user);
        seedContext.Otps.Add(otp);
        await seedContext.SaveChangesAsync();

        Client.ClearAuthentication();

        var request = new
        {
            Email = "reset-invalid@test.com",
            Code = Otp.InvalidCode,
            NewPassword = "NewSecure123!abc",
        };

        var response = await Client.PostAsJsonAsync(ResetPasswordUrl, request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    /// <summary>
    /// Verifies that submitting with an empty email returns a 400 Bad Request from the validator.
    /// </summary>
    [Fact]
    public async Task ResetPassword_WithEmptyEmail_ReturnsBadRequest()
    {
        Client.ClearAuthentication();

        var request = new
        {
            Email = "",
            Code = Otp.ValidCode,
            NewPassword = "NewSecure123!abc",
        };

        var response = await Client.PostAsJsonAsync(ResetPasswordUrl, request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
