using _116.Identity.Application.Auth.Services;
using _116.Identity.Application.Auth.UseCases.Public.Commands.ResetPassword.V1;
using _116.Identity.Domain.Enums;
using _116.Identity.Infrastructure.Persistence;
using _116.Tests.Fixtures.Builders.Requests.Identity;
using _116.Tests.Fixtures.Factories.Identity;

namespace _116.Integration.Tests.Modules.Identity.Application.Auth.UseCases.Public.Commands.ResetPassword.V1;

/// <summary>
/// Integration tests for the PublicResetPassword endpoint.
/// </summary>
[Collection("Database")]
public class PublicResetPasswordEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
{
    /// <summary>
    /// Verifies that a valid used OTP with matching code successfully resets the password
    /// and that the persisted password hash now matches the new password.
    /// </summary>
    [Fact]
    public async Task ResetPassword_WithValidOtp_ReturnsOk()
    {
        var passwordService = Api.Services.GetRequiredService<IPasswordService>();

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

        var request = new PublicResetPasswordRequestBuilder()
            .WithEmail("reset-valid@test.com")
            .WithCode(Otp.ValidCode)
            .WithNewPassword(TestAuth.ResetNewPassword)
            .Build();

        var response = await Client.PostAsJsonAsync(Routes.Public.Auth.ResetPassword(), request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        PublicResetPasswordResponse body = await response.ReadAsAsync<PublicResetPasswordResponse>();
        body.IsSuccess.Should().BeTrue();

        await using var verifyContext = CreateDbContext<IdentityDbContext>();
        var updated = await verifyContext.Users.FirstAsync(u => u.Id == userId);
        passwordService.Verify(request.NewPassword, updated.PasswordHash).Should().BeTrue();
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

        var request = new PublicResetPasswordRequestBuilder()
            .WithEmail("reset-invalid@test.com")
            .WithCode(Otp.InvalidCode)
            .WithNewPassword(TestAuth.ResetNewPassword)
            .Build();

        var response = await Client.PostAsJsonAsync(Routes.Public.Auth.ResetPassword(), request);

        await response.ShouldBeProblem(HttpStatusCode.BadRequest);
    }

    /// <summary>
    /// Verifies that submitting with an empty email returns a 400 Bad Request from the validator.
    /// </summary>
    [Fact]
    public async Task ResetPassword_WithEmptyEmail_ReturnsBadRequest()
    {
        Client.ClearAuthentication();

        var request = new PublicResetPasswordRequestBuilder()
            .WithEmail(string.Empty)
            .WithCode(Otp.ValidCode)
            .WithNewPassword(TestAuth.ResetNewPassword)
            .Build();

        var response = await Client.PostAsJsonAsync(Routes.Public.Auth.ResetPassword(), request);

        await response.ShouldBeProblem(HttpStatusCode.BadRequest);
    }
}
