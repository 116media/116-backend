using _116.BuildingBlocks.Constants;
using _116.Identity.Application.Auth.Exceptions;
using _116.Identity.Application.Auth.Repositories;
using _116.Identity.Application.Shared.Repositories;
using _116.Identity.Domain.Entities;
using _116.Identity.Domain.Enums;
using _116.Identity.Infrastructure.Persistence;
using _116.Shared.Application.Exceptions;
using _116.Tests.Fixtures.Factories.Identity;

namespace _116.Integration.Tests.Workflows;

/// <summary>
/// Flows covering the OTP defects Stage 5 closes: a superseded code must not pass as verified, a
/// spent code must not be replayable, and the attempt allowance must survive a resend.
/// </summary>
[Collection("Database")]
public class OtpConsumptionFlowTests(PostgresFixture postgres) : BaseRepositoryTest(postgres)
{
    /// <summary>
    /// Seeds a verified, active account to hang OTP rows off.
    /// </summary>
    /// <returns>The seeded user.</returns>
    private async Task<UserEntity> SeedUserAsync()
    {
        await using IdentityDbContext context = CreateDbContext<IdentityDbContext>();
        UserEntity user = UserFactory.CreateVerifiedActive();
        context.Users.Add(user);
        await context.SaveChangesAsync();
        return user;
    }

    [Fact]
    public async Task ResetPassword_RefusesACodeThatWasOnlySupersededByAResend()
    {
        // Arrange — a code the owner never verified, replaced by a newer one
        UserEntity user = await SeedUserAsync();
        const string supersededCode = "111111";

        await using (IdentityDbContext seedContext = CreateDbContext<IdentityDbContext>())
        {
            seedContext.Otps.Add(OtpFactory.Create(user.Id, supersededCode, EnumOtpPurpose.PasswordReset));
            await seedContext.SaveChangesAsync();
        }

        var repository = Resolve<IOtpRepository>();
        await repository.InvalidateExistingOtpsAsync(user.Id, EnumOtpPurpose.PasswordReset);

        await using (IdentityDbContext commitContext = CreateDbContext<IdentityDbContext>())
        {
            await commitContext.SaveChangesAsync();
        }

        // Act & Assert — invalidation consumes rather than marking used, so the reset lookup that
        // accepts verified codes can no longer be satisfied by one nobody verified.
        var replay = () => repository.ValidateUsedOtpAsync(user.Id, supersededCode, EnumOtpPurpose.PasswordReset);
        await replay.Should().ThrowAsync<BadRequestException>();
    }

    [Fact]
    public async Task ResetPassword_RefusesACodeThatHasAlreadyBeenSpent()
    {
        // Arrange — a verified code, as the reset flow requires
        UserEntity user = await SeedUserAsync();
        const string verifiedCode = "222222";

        OtpEntity seeded = await SeedAsync(user.Id, verifiedCode);

        var repository = Resolve<IOtpRepository>();
        OtpEntity accepted = await repository.ValidateUsedOtpAsync(user.Id, verifiedCode, EnumOtpPurpose.PasswordReset);
        accepted.Id.Should().Be(seeded.Id);

        // Act — spend it the way the reset handler does
        await using (IdentityDbContext consumeContext = CreateDbContext<IdentityDbContext>())
        {
            OtpEntity tracked = await consumeContext.Otps.FirstAsync(o => o.Id == seeded.Id);
            tracked.MarkAsConsumed();
            await consumeContext.SaveChangesAsync();
        }

        // Assert — the same code cannot drive a second reset
        var replay = () => repository.ValidateUsedOtpAsync(user.Id, verifiedCode, EnumOtpPurpose.PasswordReset);
        await replay.Should().ThrowAsync<BadRequestException>();
    }

    [Fact]
    public async Task VerifyOtp_AccountAttemptCounterSurvivesAResend()
    {
        // Arrange
        UserEntity user = await SeedUserAsync();

        await using (IdentityDbContext seedContext = CreateDbContext<IdentityDbContext>())
        {
            seedContext.Otps.Add(OtpFactory.Create(user.Id, "333333", EnumOtpPurpose.EmailVerification));
            await seedContext.SaveChangesAsync();
        }

        var repository = Resolve<IOtpRepository>();
        var lockoutRepository = Resolve<IAccountLockoutRepository>();

        // Act — exhaust the per-code allowance, then rotate the code as a resend would
        for (int attempt = 0; attempt < UserConstants.MaxOtpAttempts - 1; attempt++)
        {
            var wrong = () => repository.ValidateOtpAsync(user.Id, "000000", EnumOtpPurpose.EmailVerification);
            await wrong.Should().ThrowAsync<BadRequestException>();
        }

        var exhausting = () => repository.ValidateOtpAsync(user.Id, "000000", EnumOtpPurpose.EmailVerification);
        await exhausting.Should().ThrowAsync<OtpAttemptsLimitException>();

        await using (IdentityDbContext rotateContext = CreateDbContext<IdentityDbContext>())
        {
            List<OtpEntity> existing = await rotateContext
                .Otps.Where(o => o.UserId == user.Id && o.ConsumedAt == null)
                .ToListAsync();
            existing.ForEach(o => o.MarkAsConsumed());
            rotateContext.Otps.Add(OtpFactory.Create(user.Id, "444444", EnumOtpPurpose.EmailVerification));
            await rotateContext.SaveChangesAsync();
        }

        // Assert — the per-code allowance reset, but the account counter did not, which is the
        // whole point: resend used to hand an attacker a fresh three guesses indefinitely.
        AccountLockoutState afterResend = await lockoutRepository.GetAsync(user.Id, CancellationToken.None);
        afterResend.OtpFailedAttempts.Should().Be(UserConstants.MaxOtpAttempts);
    }

    /// <summary>
    /// Seeds a code that has been verified but not yet spent.
    /// </summary>
    /// <param name="userId">The account the code belongs to.</param>
    /// <param name="code">The plaintext code.</param>
    /// <returns>The seeded OTP.</returns>
    private async Task<OtpEntity> SeedAsync(Guid userId, string code)
    {
        await using IdentityDbContext context = CreateDbContext<IdentityDbContext>();
        OtpEntity otp = OtpFactory.CreateUsed(userId, code, EnumOtpPurpose.PasswordReset);
        context.Otps.Add(otp);
        await context.SaveChangesAsync();
        return otp;
    }
}
