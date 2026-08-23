using _116.Identity.Application.Auth.Constants;
using _116.Identity.Application.Auth.Exceptions;
using _116.Identity.Application.Auth.UseCases.Admin.Commands.VerifyOtp.V1;
using _116.Identity.Application.Shared.Errors.Messages;
using _116.Identity.Domain.Enums;
using _116.Identity.Infrastructure.Persistence;
using _116.Tests.Fixtures.Builders.Requests.Identity;
using _116.Tests.Fixtures.Factories.Identity;
using FluentValidation;
using FluentValidation.Results;

namespace _116.Integration.Tests.Modules.Identity.Application.Auth.UseCases.Admin.Commands.VerifyOtp.V1;

/// <summary>
/// Integration tests for the AdminVerifyOtp endpoint.
/// </summary>
[Collection("Database")]
public class AdminVerifyOtpEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
{
    private const string AuthUrl = ApiRoutes.Admin.Auth;
    private const string VerifyOtpUrl = $"{AuthUrl}/{AuthRouteConstants.VerifyOtp}";

    private static string ValidationDetail(params (string Property, string Message)[] failures) =>
        new ValidationException(failures.Select(f => new ValidationFailure(f.Property, f.Message))).Message;

    [Fact]
    public async Task VerifyOtp_WithEmptyFields_ReturnsValidationError()
    {
        Client.ClearAuthentication();
        var request = new AdminVerifyOtpRequestBuilder()
            .WithEmail(string.Empty)
            .WithCode(string.Empty)
            .WithPurpose(string.Empty)
            .Build();

        var response = await Client.PostAsJsonAsync(VerifyOtpUrl, request);

        await response.ShouldBeProblem<ValidationException>(
            HttpStatusCode.BadRequest,
            ValidationDetail(
                ("Email", Localized<ValidationErrorMessage>(m => m.EmailRequired())),
                ("Code", Localized<ValidationErrorMessage>(m => m.OtpCodeRequired())),
                ("Purpose", Localized<ValidationErrorMessage>(m => m.OtpPurposeRequired()))
            )
        );
    }

    [Fact]
    public async Task VerifyOtp_WithValidOtp_MarksUserVerifiedAndConsumesOtp()
    {
        Client.ClearAuthentication();

        var email = $"admin-verify-ok-{Guid.NewGuid():N}@test.com";
        var adminRole = RoleFactory.CreateAdmin();
        var user = UserFactory.Create(email);
        user.Activate();

        var userRole = UserRoleFactory.Create(user.Id, adminRole.Id);
        var otp = OtpFactory.Create(user.Id, Otp.ValidCode, EnumOtpPurpose.EmailVerification);

        await using var seedContext = CreateDbContext<IdentityDbContext>();
        seedContext.Roles.Add(adminRole);
        seedContext.Users.Add(user);
        seedContext.UserRoles.Add(userRole);
        seedContext.Otps.Add(otp);
        await seedContext.SaveChangesAsync();

        var request = new AdminVerifyOtpRequestBuilder()
            .WithEmail(email)
            .WithCode(Otp.ValidCode)
            .WithPurpose(nameof(EnumOtpPurpose.EmailVerification))
            .Build();

        var response = await Client.PostAsJsonAsync(VerifyOtpUrl, request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        AdminVerifyOtpResponse body = await response.ReadAsAsync<AdminVerifyOtpResponse>();
        body.IsSuccess.Should().BeTrue();

        await using var verifyContext = CreateDbContext<IdentityDbContext>();
        var verifiedUser = await verifyContext.Users.FirstAsync(u => u.Id == user.Id);
        verifiedUser.IsVerified.Should().BeTrue();

        var consumedOtp = await verifyContext.Otps.FirstAsync(o => o.Id == otp.Id);
        consumedOtp.IsUsed.Should().BeTrue();
    }

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

        var request = new AdminVerifyOtpRequestBuilder()
            .WithEmail(email)
            .WithCode(Otp.ValidCode)
            .WithPurpose(nameof(EnumOtpPurpose.EmailVerification))
            .Build();

        var response = await Client.PostAsJsonAsync(VerifyOtpUrl, request);

        await response.ShouldBeProblem<OtpExpirationException>(
            HttpStatusCode.Gone,
            Localized<ValidationErrorMessage>(m => m.OtpExpired())
        );
    }

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

        var request = new AdminVerifyOtpRequestBuilder()
            .WithEmail(email)
            .WithCode(Otp.ValidCode)
            .WithPurpose(nameof(EnumOtpPurpose.EmailVerification))
            .Build();

        var response = await Client.PostAsJsonAsync(VerifyOtpUrl, request);

        await response.ShouldBeProblem<OtpAttemptsLimitException>(
            HttpStatusCode.TooManyRequests,
            Localized<ValidationErrorMessage>(m => m.MaxOtpAttemptsReached())
        );
    }
}
