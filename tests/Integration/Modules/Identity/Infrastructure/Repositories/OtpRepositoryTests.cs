using _116.Identity.Application.Auth.Exceptions;
using _116.Identity.Application.Auth.Repositories;
using _116.Identity.Domain.Enums;
using _116.Identity.Domain.ValueObjects;
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
        saved.Purpose.Value.Should().Be(EnumOtpPurpose.EmailVerification);
    }

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
        saved!.CodeHash.Should().StartWith("h1:");
        saved.CodeHash.Should().NotBe(Otp.ValidCode);
        saved.Purpose.Value.Should().Be(EnumOtpPurpose.PasswordReset);
    }

    [Fact]
    public async Task GetLatestOutstandingOtpOrThrowAsync_WithAnOutstandingOtp_ShouldReturnIt()
    {
        await using var seedContext = CreateDbContext<IdentityDbContext>();
        var user = UserFactory.CreateVerifiedActive();
        seedContext.Users.Add(user);
        var otp = OtpFactory.Create(user.Id, Otp.ValidCode, EnumOtpPurpose.EmailVerification);
        seedContext.Otps.Add(otp);
        await seedContext.SaveChangesAsync();

        var repo = Resolve<IOtpRepository>();

        var result = await repo.GetLatestOutstandingOtpOrThrowAsync(user.Id, EnumOtpPurpose.EmailVerification);

        result.Should().NotBeNull();
        result.Id.Should().Be(otp.Id);
        result.UserId.Should().Be(user.Id);
        result.CodeHash.Should().NotBe(Otp.ValidCode);
    }

    [Fact]
    public async Task GetLatestOutstandingOtpOrThrowAsync_WhenNoneExists_ShouldThrow()
    {
        var repo = Resolve<IOtpRepository>();
        var nonExistentUserId = Guid.NewGuid();

        var act = () => repo.GetLatestOutstandingOtpOrThrowAsync(nonExistentUserId, EnumOtpPurpose.EmailVerification);

        await act.Should().ThrowAsync<NotFoundException>();
    }

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

    [Fact]
    public async Task InvalidateExistingOtpsAsync_ShouldConsumeAllExistingOtps()
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
        var emailVerification = new OtpPurpose(EnumOtpPurpose.EmailVerification);
        var otps = await verifyContext
            .Otps.Where(o => o.UserId == user.Id && o.Purpose == emailVerification)
            .ToListAsync();

        // Superseded codes are consumed, not marked used: "used" means the owner verified it, and
        // the password-reset lookup accepts used codes. Conflating the two made a code nobody
        // verified into a valid reset credential.
        otps.Should().HaveCount(2);
        otps.Should().OnlyContain(o => o.ConsumedAt != null);
        otps.Should().NotContain(o => o.IsUsed);
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

        deletedCount.Should().Be(2);

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
