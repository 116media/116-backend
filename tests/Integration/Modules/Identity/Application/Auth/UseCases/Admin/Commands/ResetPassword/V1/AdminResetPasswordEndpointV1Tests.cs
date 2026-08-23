using _116.Identity.Application.Auth.Constants;
using _116.Identity.Application.Auth.Exceptions;
using _116.Identity.Application.Auth.Services;
using _116.Identity.Application.Auth.UseCases.Admin.Commands.ResetPassword.V1;
using _116.Identity.Application.Shared.Errors.Messages;
using _116.Identity.Domain.Enums;
using _116.Identity.Infrastructure.Persistence;
using _116.Shared.Application.Exceptions;
using _116.Tests.Fixtures.Builders.Requests.Identity;
using _116.Tests.Fixtures.Factories.Identity;
using FluentValidation;
using FluentValidation.Results;

namespace _116.Integration.Tests.Modules.Identity.Application.Auth.UseCases.Admin.Commands.ResetPassword.V1;

/// <summary>
/// Integration tests for the AdminResetPassword endpoint.
/// </summary>
[Collection("Database")]
public class AdminResetPasswordEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
{
    private const string AuthUrl = ApiRoutes.Admin.Auth;
    private const string ResetPasswordUrl = $"{AuthUrl}/{AuthRouteConstants.ResetPassword}";

    private static string ValidationDetail(params (string Property, string Message)[] failures) =>
        new ValidationException(failures.Select(f => new ValidationFailure(f.Property, f.Message))).Message;

    [Fact]
    public async Task ResetPassword_WithEmptyFields_ReturnsValidationError()
    {
        Client.ClearAuthentication();
        var request = new AdminResetPasswordRequestBuilder()
            .WithEmail(string.Empty)
            .WithCode(string.Empty)
            .WithNewPassword(string.Empty)
            .Build();

        var response = await Client.PostAsJsonAsync(ResetPasswordUrl, request);

        await response.ShouldBeProblem<ValidationException>(
            HttpStatusCode.BadRequest,
            ValidationDetail(
                ("Email", Localized<ValidationErrorMessage>(m => m.EmailRequired())),
                ("Code", Localized<ValidationErrorMessage>(m => m.OtpCodeRequired())),
                ("NewPassword", Localized<ValidationErrorMessage>(m => m.PasswordRequired()))
            )
        );
    }

    [Fact]
    public async Task ResetPassword_WithValidOtp_ReturnsOk()
    {
        var passwordService = Api.Services.GetRequiredService<IPasswordService>();

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

        var request = new AdminResetPasswordRequestBuilder()
            .WithEmail(email)
            .WithCode(Otp.ValidCode)
            .WithNewPassword("NewSecure@Pass1")
            .Build();

        var response = await Client.PostAsJsonAsync(ResetPasswordUrl, request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        AdminResetPasswordResponse body = await response.ReadAsAsync<AdminResetPasswordResponse>();
        body.IsSuccess.Should().BeTrue();

        await using var verifyContext = CreateDbContext<IdentityDbContext>();
        var updated = await verifyContext.Users.FirstAsync(u => u.Id == user.Id);
        passwordService.Verify(request.NewPassword, updated.PasswordHash).Should().BeTrue();
    }

    [Fact]
    public async Task ResetPassword_WithCodeNotMatchingTheVerifiedOtp_ReturnsBadRequest()
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

        var request = new AdminResetPasswordRequestBuilder()
            .WithEmail(email)
            .WithCode(Otp.InvalidCode)
            .WithNewPassword("NewSecure@Pass1")
            .Build();

        var response = await Client.PostAsJsonAsync(ResetPasswordUrl, request);

        await response.ShouldBeProblem<BadRequestException>(
            HttpStatusCode.BadRequest,
            Localized<ValidationErrorMessage>(m => m.OtpNotYetVerified())
        );
    }

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

        var request = new AdminResetPasswordRequestBuilder()
            .WithEmail(email)
            .WithCode(Otp.ValidCode)
            .WithNewPassword("NewSecure@Pass1")
            .Build();

        var response = await Client.PostAsJsonAsync(ResetPasswordUrl, request);

        await response.ShouldBeProblem<OtpExpirationException>(
            HttpStatusCode.Gone,
            Localized<ValidationErrorMessage>(m => m.OtpExpired())
        );
    }

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

        var request = new AdminResetPasswordRequestBuilder()
            .WithEmail(email)
            .WithCode(Otp.ValidCode)
            .WithNewPassword("NewSecure@Pass1")
            .Build();

        var response = await Client.PostAsJsonAsync(ResetPasswordUrl, request);

        await response.ShouldBeProblem<BadRequestException>(
            HttpStatusCode.BadRequest,
            Localized<ValidationErrorMessage>(m => m.OtpNotYetVerified())
        );
    }
}
