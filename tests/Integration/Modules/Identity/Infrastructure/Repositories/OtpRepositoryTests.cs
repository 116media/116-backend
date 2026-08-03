using _116.Identity.Application.Auth.Exceptions;
using _116.Identity.Application.Auth.Repositories;
using _116.Identity.Domain.Enums;
using _116.Identity.Infrastructure.Persistence;
using _116.Shared.Application.Exceptions;
using _116.Tests.Fixtures.Factories.Identity;

namespace _116.Integration.Tests.Modules.Identity.Infrastructure.Repositories;

/// <summary>
/// Integration tests for <see cref="IOtpRepository"/> verifying
/// OTP creation, validation, invalidation, and cleanup operations
/// against a real PostgreSQL database.
/// </summary>
[Collection("Database")]
public class OtpRepositoryTests(PostgresFixture postgres) : BaseRepositoryTest(postgres)
{
    [Fact]
    public async Task AddAsync_ShouldPersistOtpToDatabase()
    {
        await using var seedContext = CreateDbContext<IdentityDbContext>();
        var user = UserFactory.CreateVerifiedActive();
        seedContext.Users.Add(user);
        await seedContext.SaveChangesAsync();

        var (repo, db) = CreateScopedRepository<IOtpRepository, IdentityDbContext>();

        var otp = OtpFactory.CreateValid(user.Id);
        await repo.AddAsync(otp);
        await db.SaveChangesAsync();

        await using var verifyContext = CreateDbContext<IdentityDbContext>();
        var saved = await verifyContext.Otps.FindAsync(otp.Id);

        saved.Should().NotBeNull();
        saved!.UserId.Should().Be(user.Id);
        saved.Purpose.Should().Be(EnumOtpPurpose.EmailVerification);
    }

    /// <summary>
    /// Verifies that a persisted OTP keeps only the hash of its code: the row that reaches
    /// PostgreSQL can never be read back as a usable credential.
    /// </summary>
    [Fact]
    public async Task AddAsync_ShouldStoreTheCodeHashAndNeverThePlaintext()
    {
        await using var seedContext = CreateDbContext<IdentityDbContext>();
        var user = UserFactory.CreateVerifiedActive();
        seedContext.Users.Add(user);
        await seedContext.SaveChangesAsync();

        var (repo, db) = CreateScopedRepository<IOtpRepository, IdentityDbContext>();

        var otp = OtpFactory.Create(user.Id, Otp.ValidCode, EnumOtpPurpose.PasswordReset);
        await repo.AddAsync(otp);
        await db.SaveChangesAsync();

        await using var verifyContext = CreateDbContext<IdentityDbContext>();
        var saved = await verifyContext.Otps.FindAsync(otp.Id);

        saved.Should().NotBeNull();
        saved!.CodeHash.Should().StartWith("v1:");
        saved.CodeHash.Should().NotBe(Otp.ValidCode);
        saved.Purpose.Should().Be(EnumOtpPurpose.PasswordReset);
    }

    /// <summary>
    /// Verifies that <see cref="IOtpRepository.ValidateOtpAsync" /> accepts the plaintext code by
    /// comparing it against the stored hash, exercising the identity lookup in
    /// <c>OtpRepository</c> via <c>OtpForValidationSpecification</c>.
    /// </summary>
    [Fact]
    public async Task ValidateOtpAsync_ValidOtp_ShouldReturnMatchingOtp()
    {
        await using var seedContext = CreateDbContext<IdentityDbContext>();
        var user = UserFactory.CreateVerifiedActive();
        seedContext.Users.Add(user);
        var otp = OtpFactory.Create(user.Id, Otp.ValidCode, EnumOtpPurpose.EmailVerification);
        seedContext.Otps.Add(otp);
        await seedContext.SaveChangesAsync();

        var repo = Resolve<IOtpRepository>();

        var result = await repo.ValidateOtpAsync(user.Id, Otp.ValidCode, EnumOtpPurpose.EmailVerification);

        result.Should().NotBeNull();
        result.Id.Should().Be(otp.Id);
        result.UserId.Should().Be(user.Id);
        result.CodeHash.Should().NotBe(Otp.ValidCode);
    }

    [Fact]
    public async Task ValidateOtpAsync_NonExistentOtp_ShouldThrow()
    {
        var repo = Resolve<IOtpRepository>();
        var nonExistentUserId = Guid.NewGuid();

        var act = () => repo.ValidateOtpAsync(nonExistentUserId, Otp.InvalidCode, EnumOtpPurpose.EmailVerification);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    /// <summary>
    /// Verifies that <see cref="IOtpRepository.ValidateUsedOtpAsync" /> matches a consumed OTP by
    /// hash comparison, exercising the used-row lookup in <c>OtpRepository</c> via
    /// <c>OtpForUsedValidationSpecification</c>.
    /// </summary>
    [Fact]
    public async Task ValidateUsedOtpAsync_UsedOtp_ShouldReturnMatchingOtp()
    {
        await using var seedContext = CreateDbContext<IdentityDbContext>();
        var user = UserFactory.CreateVerifiedActive();
        seedContext.Users.Add(user);
        var otp = OtpFactory.CreateUsed(user.Id, Otp.ValidCode, EnumOtpPurpose.PasswordReset);
        seedContext.Otps.Add(otp);
        await seedContext.SaveChangesAsync();

        var repo = Resolve<IOtpRepository>();

        var result = await repo.ValidateUsedOtpAsync(user.Id, Otp.ValidCode, EnumOtpPurpose.PasswordReset);

        result.Should().NotBeNull();
        result.Id.Should().Be(otp.Id);
        result.IsUsed.Should().BeTrue();
    }

    /// <summary>
    /// Verifies that a code that does not match the stored hash is rejected and consumes one of
    /// the allowed attempts, and that exhausting them switches the failure to the attempts-limit
    /// error. Drives <c>OtpRepository.ValidateOtpAsync</c> through the real DI-resolved repository.
    /// </summary>
    [Fact]
    public async Task ValidateOtpAsync_WithAWrongCode_ShouldConsumeAttemptsUntilTheLimitIsReached()
    {
        await using var seedContext = CreateDbContext<IdentityDbContext>();
        var user = UserFactory.CreateVerifiedActive();
        seedContext.Users.Add(user);
        var otp = OtpFactory.Create(user.Id, Otp.ValidCode, EnumOtpPurpose.EmailVerification);
        seedContext.Otps.Add(otp);
        await seedContext.SaveChangesAsync();

        var repo = Resolve<IOtpRepository>();

        var firstAttempt = () => repo.ValidateOtpAsync(user.Id, Otp.InvalidCode, EnumOtpPurpose.EmailVerification);
        await firstAttempt.Should().ThrowAsync<BadRequestException>();

        await using (var afterFirst = CreateDbContext<IdentityDbContext>())
        {
            (await afterFirst.Otps.FirstAsync(o => o.Id == otp.Id)).AttemptCount.Should().Be(1);
        }

        var secondAttempt = () => repo.ValidateOtpAsync(user.Id, Otp.InvalidCode, EnumOtpPurpose.EmailVerification);
        await secondAttempt.Should().ThrowAsync<BadRequestException>();

        // The third wrong code exhausts the allowance, so the failure changes shape.
        var thirdAttempt = () => repo.ValidateOtpAsync(user.Id, Otp.InvalidCode, EnumOtpPurpose.EmailVerification);
        await thirdAttempt.Should().ThrowAsync<OtpAttemptsLimitException>();

        await using var verifyContext = CreateDbContext<IdentityDbContext>();
        (await verifyContext.Otps.FirstAsync(o => o.Id == otp.Id)).AttemptCount.Should().Be(3);
    }

    [Fact]
    public async Task InvalidateExistingOtpsAsync_ShouldMarkAllExistingOtpsAsUsed()
    {
        await using var seedContext = CreateDbContext<IdentityDbContext>();
        var user = UserFactory.CreateVerifiedActive();
        seedContext.Users.Add(user);
        var otp1 = OtpFactory.Create(user.Id, "111111", EnumOtpPurpose.EmailVerification);
        var otp2 = OtpFactory.Create(user.Id, "222222", EnumOtpPurpose.EmailVerification);
        seedContext.Otps.AddRange(otp1, otp2);
        await seedContext.SaveChangesAsync();

        var (repo, db) = CreateScopedRepository<IOtpRepository, IdentityDbContext>();

        await repo.InvalidateExistingOtpsAsync(user.Id, EnumOtpPurpose.EmailVerification);
        await db.SaveChangesAsync();

        await using var verifyContext = CreateDbContext<IdentityDbContext>();
        var otps = await verifyContext
            .Otps.Where(o => o.UserId == user.Id && o.Purpose == EnumOtpPurpose.EmailVerification)
            .ToListAsync();

        otps.Should().HaveCount(2);
        otps.Should().OnlyContain(o => o.IsUsed);
    }

    [Fact]
    public async Task CleanupExpiredOtpsAsync_ShouldDeleteExpiredOtpsAndReturnCount()
    {
        await using var seedContext = CreateDbContext<IdentityDbContext>();
        var user = UserFactory.CreateVerifiedActive();
        seedContext.Users.Add(user);
        var expired1 = OtpFactory.CreateExpired(user.Id, EnumOtpPurpose.EmailVerification);
        var expired2 = OtpFactory.CreateExpired(user.Id, EnumOtpPurpose.PasswordReset);
        seedContext.Otps.AddRange(expired1, expired2);
        await seedContext.SaveChangesAsync();

        var (repo, db) = CreateScopedRepository<IOtpRepository, IdentityDbContext>();

        var deletedCount = await repo.CleanupExpiredOtpsAsync();
        await db.SaveChangesAsync();

        deletedCount.Should().BeGreaterThanOrEqualTo(2);

        await using var verifyContext = CreateDbContext<IdentityDbContext>();
        var remaining = await verifyContext.Otps.Where(o => o.Id == expired1.Id || o.Id == expired2.Id).ToListAsync();

        remaining.Should().BeEmpty();
    }

    [Fact]
    public async Task CleanupExpiredOtpsAsync_NoExpiredOtps_ShouldReturnZero()
    {
        await using var seedContext = CreateDbContext<IdentityDbContext>();
        var user = UserFactory.CreateVerifiedActive();
        seedContext.Users.Add(user);
        var validOtp = OtpFactory.CreateValid(user.Id);
        seedContext.Otps.Add(validOtp);
        await seedContext.SaveChangesAsync();

        var repo = Resolve<IOtpRepository>();

        var deletedCount = await repo.CleanupExpiredOtpsAsync();

        deletedCount.Should().Be(0);

        await using var verifyContext = CreateDbContext<IdentityDbContext>();
        var remaining = await verifyContext.Otps.FindAsync(validOtp.Id);

        remaining.Should().NotBeNull();
    }
}
