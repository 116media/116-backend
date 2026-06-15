using _116.Identity.Domain.Enums;
using _116.Identity.Infrastructure.Persistence;
using _116.Tests.Fixtures.Factories.Identity;

namespace _116.Integration.Tests.Modules.Identity.Application.Auth.UseCases.Admin.Commands.ResetPassword.V1;

/// <summary>
/// Integration tests for the AdminResetPassword endpoint.
/// </summary>
[Collection("Database")]
public class AdminResetPasswordEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
{
    private const string AuthUrl = ApiRoutes.Admin.Auth;

    [Fact]
    public async Task ResetPassword_WithEmptyFields_ReturnsValidationError()
    {
        Client.ClearAuthentication();
        var request = new
        {
            Email = "",
            Code = "",
            NewPassword = "",
        };

        var response = await Client.PostAsJsonAsync($"{AuthUrl}/reset-password", request);

        response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.UnprocessableEntity);
    }

    /// <summary>
    /// Verifies that a valid OTP code that has been previously verified allows
    /// the admin user to reset their password successfully.
    /// </summary>
    [Fact]
    public async Task ResetPassword_WithValidOtp_ReturnsOk()
    {
        Client.ClearAuthentication();

        var email = $"admin-reset-ok-{Guid.NewGuid():N}@test.com";
        var adminRole = RoleFactory.CreateAdmin();
        var user = UserFactory.Create(email);
        user.Activate();

        var userRole = UserRoleFactory.Create(user.Id, adminRole.Id);
        var otp = OtpFactory.CreateUsed(user.Id, Otp.ValidCode, EnumOtpPurpose.PasswordReset);

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
            NewPassword = "NewSecure@Pass1",
        };

        var response = await Client.PostAsJsonAsync($"{AuthUrl}/reset-password", request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    /// <summary>
    /// Verifies that providing an incorrect OTP code returns a 400 Bad Request,
    /// indicating that the OTP has not yet been verified via the verify-otp endpoint.
    /// </summary>
    [Fact]
    public async Task ResetPassword_WithInvalidOtpCode_ReturnsBadRequest()
    {
        Client.ClearAuthentication();

        var email = $"admin-reset-bad-{Guid.NewGuid():N}@test.com";
        var adminRole = RoleFactory.CreateAdmin();
        var user = UserFactory.Create(email);
        user.Activate();

        var userRole = UserRoleFactory.Create(user.Id, adminRole.Id);
        var otp = OtpFactory.CreateUsed(user.Id, Otp.ValidCode, EnumOtpPurpose.PasswordReset);

        await using var seedContext = CreateDbContext<IdentityDbContext>();
        seedContext.Roles.Add(adminRole);
        seedContext.Users.Add(user);
        seedContext.UserRoles.Add(userRole);
        seedContext.Otps.Add(otp);
        await seedContext.SaveChangesAsync();

        var request = new
        {
            Email = email,
            Code = Otp.InvalidCode,
            NewPassword = "NewSecure@Pass1",
        };

        var response = await Client.PostAsJsonAsync($"{AuthUrl}/reset-password", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    /// <summary>
    /// Verifies that attempting to reset a password with an expired OTP returns 410 Gone,
    /// covering the OtpExpirationExceptionHandler path.
    /// </summary>
    [Fact]
    public async Task ResetPassword_WithExpiredOtp_ReturnsGone()
    {
        Client.ClearAuthentication();

        var email = $"admin-reset-exp-{Guid.NewGuid():N}@test.com";
        var adminRole = RoleFactory.CreateAdmin();
        var user = UserFactory.Create(email);
        user.Activate();

        var userRole = UserRoleFactory.Create(user.Id, adminRole.Id);
        var otp = OtpFactory.CreateUsedAndExpired(user.Id, Otp.ValidCode, EnumOtpPurpose.PasswordReset);

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
            NewPassword = "NewSecure@Pass1",
        };

        var response = await Client.PostAsJsonAsync($"{AuthUrl}/reset-password", request);

        response.StatusCode.Should().Be(HttpStatusCode.Gone);
    }

    /// <summary>
    /// Verifies that attempting to reset a password when the OTP has not yet been verified
    /// via the verify-otp endpoint returns a 400 Bad Request.
    /// This covers the scenario where the OTP has max attempts reached but was never marked as used.
    /// </summary>
    [Fact]
    public async Task ResetPassword_WithUnverifiedOtp_ReturnsBadRequest()
    {
        Client.ClearAuthentication();

        var email = $"admin-reset-maxotp-{Guid.NewGuid():N}@test.com";
        var adminRole = RoleFactory.CreateAdmin();
        var user = UserFactory.Create(email);
        user.Activate();

        var userRole = UserRoleFactory.Create(user.Id, adminRole.Id);
        var otp = OtpFactory.CreateMaxAttemptsReached(user.Id, Otp.ValidCode, EnumOtpPurpose.PasswordReset);

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
            NewPassword = "NewSecure@Pass1",
        };

        var response = await Client.PostAsJsonAsync($"{AuthUrl}/reset-password", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
