using _116.Identity.Application.Auth.Exceptions;
using _116.Identity.Application.Auth.UseCases.Public.Commands.VerifyOtp.V1;
using _116.Identity.Application.Shared.Errors.Messages;
using _116.Identity.Domain.Enums;
using _116.Identity.Infrastructure.Persistence;
using _116.Shared.Application.Exceptions;
using _116.Shared.Application.Exceptions.Messages;
using _116.Tests.Fixtures.Builders.Requests.Identity;
using _116.Tests.Fixtures.Factories.Identity;
using FluentValidation;
using FluentValidation.Results;

namespace _116.Integration.Tests.Modules.Identity.Application.Auth.UseCases.Public.Commands.VerifyOtp.V1;

/// <summary>
/// Integration tests for the PublicVerifyOtp endpoint.
/// </summary>
[Collection("Database")]
public class PublicVerifyOtpEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
{
    private static string ValidationDetail(string property, string message) =>
        new ValidationException([new ValidationFailure(property, message)]).Message;

    [Fact]
    public async Task VerifyOtp_WithEmptyEmail_ReturnsValidationError()
    {
        Client.ClearAuthentication();
        var request = new PublicVerifyOtpRequestBuilder().WithEmail(string.Empty).Build();

        var response = await Client.PostAsJsonAsync(Routes.Public.Auth.VerifyOtp(), request);

        await response.ShouldBeProblem<ValidationException>(
            HttpStatusCode.BadRequest,
            ValidationDetail("Email", Localized<ValidationErrorMessage>(m => m.EmailRequired()))
        );
    }

    [Fact]
    public async Task VerifyOtp_WithNonExistentEmail_ReturnsNotFound()
    {
        Client.ClearAuthentication();
        var request = new PublicVerifyOtpRequestBuilder()
            .WithEmail("nonexistent@test.com")
            .WithCode(Otp.InvalidCode)
            .Build();

        var response = await Client.PostAsJsonAsync(Routes.Public.Auth.VerifyOtp(), request);

        await response.ShouldBeProblem<NotFoundException>(
            HttpStatusCode.NotFound,
            Localized<SharedExceptionMessage>(m => m.EntityNotFound("User"))
        );
    }

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

    [Fact]
    public async Task VerifyOtp_WithWrongCodeWhileAValidOtpExists_ReturnsBadRequestAndIncrementsAttemptCount()
    {
        Client.ClearAuthentication();
        Client.DefaultRequestHeaders.Add("Accept-Language", "en");

        var email = $"pub-wrong-code-{Guid.NewGuid():N}@test.com";
        var user = UserFactory.Create(email);
        user.Activate();

        var otp = OtpFactory.Create(user.Id, Otp.ValidCode, EnumOtpPurpose.EmailVerification);

        await SeedAsync<IdentityDbContext>(context =>
        {
            context.Users.Add(user);
            context.Otps.Add(otp);
        });

        var request = new PublicVerifyOtpRequestBuilder()
            .WithEmail(email)
            .WithCode(Otp.InvalidCode)
            .WithPurpose(nameof(EnumOtpPurpose.EmailVerification))
            .Build();

        var response = await Client.PostAsJsonAsync(Routes.Public.Auth.VerifyOtp(), request);

        await response.ShouldBeProblem<BadRequestException>(
            HttpStatusCode.BadRequest,
            Localized<ValidationErrorMessage>(m => m.InvalidOtpCode(), LocalizedMessage.EnglishCulture)
        );

        await using var verifyContext = CreateDbContext<IdentityDbContext>();
        var persistedOtp = await verifyContext.Otps.FirstAsync(o => o.Id == otp.Id);
        persistedOtp.AttemptCount.Should().Be(1);
        persistedOtp.IsUsed.Should().BeFalse();

        var persistedUser = await verifyContext.Users.FirstAsync(u => u.Id == user.Id);
        persistedUser.IsVerified.Should().BeFalse();
    }

    [Fact]
    public async Task VerifyOtp_WhenAccountAlreadyVerified_ReturnsConflictAndLeavesOtpUnconsumed()
    {
        Client.ClearAuthentication();
        Client.DefaultRequestHeaders.Add("Accept-Language", "en");

        var email = $"pub-already-verified-{Guid.NewGuid():N}@test.com";
        var user = UserFactory.Create(email);
        user.Activate();
        user.MarkAsVerified();

        var otp = OtpFactory.Create(user.Id, Otp.ValidCode, EnumOtpPurpose.EmailVerification);

        await SeedAsync<IdentityDbContext>(context =>
        {
            context.Users.Add(user);
            context.Otps.Add(otp);
        });

        var request = new PublicVerifyOtpRequestBuilder()
            .WithEmail(email)
            .WithCode(Otp.ValidCode)
            .WithPurpose(nameof(EnumOtpPurpose.EmailVerification))
            .Build();

        var response = await Client.PostAsJsonAsync(Routes.Public.Auth.VerifyOtp(), request);

        await response.ShouldBeProblem<ConflictException>(
            HttpStatusCode.Conflict,
            Localized<ValidationErrorMessage>(m => m.AccountAlreadyVerified(), LocalizedMessage.EnglishCulture)
        );

        await using var verifyContext = CreateDbContext<IdentityDbContext>();
        var persistedOtp = await verifyContext.Otps.FirstAsync(o => o.Id == otp.Id);
        persistedOtp.IsUsed.Should().BeFalse();
        persistedOtp.AttemptCount.Should().Be(0);
    }

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

        await response.ShouldBeProblem<OtpExpirationException>(
            HttpStatusCode.Gone,
            Localized<ValidationErrorMessage>(m => m.OtpExpired())
        );
    }

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

        await response.ShouldBeProblem<OtpAttemptsLimitException>(
            HttpStatusCode.TooManyRequests,
            Localized<ValidationErrorMessage>(m => m.MaxOtpAttemptsReached())
        );
    }
}
