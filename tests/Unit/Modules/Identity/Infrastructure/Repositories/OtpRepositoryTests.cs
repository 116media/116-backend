using _116.Identity.Application.Auth.Exceptions;
using _116.Identity.Application.Shared.Exceptions;
using _116.Identity.Domain.Entities;
using _116.Identity.Domain.Enums;
using _116.Identity.Infrastructure.Persistence;
using _116.Identity.Infrastructure.Repositories;
using _116.Shared.Application.Exceptions;
using _116.Unit.Tests.Common.Builders.Entities;
using AwesomeAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace _116.Unit.Tests.Modules.Identity.Infrastructure.Repositories;

/// <summary>
/// Unit tests for <see cref="OtpRepository"/>.
/// </summary>
public class OtpRepositoryTests : IDisposable
{
    private readonly IdentityDbContext _context;
    private readonly OtpRepository _repository;

    public OtpRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<IdentityDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new IdentityDbContext(options);
        _repository = new OtpRepository(_context);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }

    private OtpEntity CreateOtpWithCreatedAt(OtpBuilder? builder = null)
    {
        var otp = builder?.Build() ?? new OtpBuilder().Build();

        // Set CreatedAt using reflection to bypass OtpBuilder limitation
        typeof(OtpEntity).GetProperty("CreatedAt")!.SetValue(otp, DateTime.UtcNow);

        return otp;
    }

    #region AddAsync Tests

    [Fact]
    public async Task AddAsync_ShouldAddOtpToContext()
    {
        // Arrange
        var otp = CreateOtpWithCreatedAt();

        // Act
        await _repository.AddAsync(otp);
        await _context.SaveChangesAsync();

        // Assert
        var savedOtp = await _context.Otps.FirstOrDefaultAsync(o => o.Id == otp.Id);
        savedOtp.Should().NotBeNull();
        savedOtp!.Id.Should().Be(otp.Id);
    }

    #endregion

    #region ValidateOtpAsync Tests

    [Fact]
    public async Task ValidateOtpAsync_WhenOtpIsValid_ShouldReturnOtp()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var code = "123456";
        var purpose = EnumOtpPurpose.EmailVerification;

        var otp = CreateOtpWithCreatedAt(new OtpBuilder().WithUserId(userId).WithCode(code).WithPurpose(purpose));

        _context.Otps.Add(otp);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.ValidateOtpAsync(userId, code, purpose);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(otp.Id);
    }

    [Fact]
    public async Task ValidateOtpAsync_WhenOtpIsExpired_ShouldThrowOtpExpirationException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var code = "123456";
        var purpose = EnumOtpPurpose.EmailVerification;

        var otp = CreateOtpWithCreatedAt(
            new OtpBuilder().WithUserId(userId).WithCode(code).WithPurpose(purpose).AsExpired()
        );

        _context.Otps.Add(otp);
        await _context.SaveChangesAsync();

        // Act
        Func<Task> act = async () => await _repository.ValidateOtpAsync(userId, code, purpose);

        // Assert
        await act.Should().ThrowAsync<OtpExpirationException>();
    }

    [Fact]
    public async Task ValidateOtpAsync_WhenMaxAttemptsReached_ShouldThrowOtpAttemptsLimitException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var code = "123456";
        var purpose = EnumOtpPurpose.EmailVerification;

        var otp = CreateOtpWithCreatedAt(
            new OtpBuilder().WithUserId(userId).WithCode(code).WithPurpose(purpose).AsMaxAttemptsReached()
        );

        _context.Otps.Add(otp);
        await _context.SaveChangesAsync();

        // Act
        Func<Task> act = async () => await _repository.ValidateOtpAsync(userId, code, purpose);

        // Assert
        await act.Should().ThrowAsync<OtpAttemptsLimitException>();
    }

    [Fact]
    public async Task ValidateOtpAsync_WhenCodeIsInvalid_ShouldIncrementAttemptCountAndThrowBadRequestException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var correctCode = "123456";
        var wrongCode = "654321";
        var purpose = EnumOtpPurpose.EmailVerification;

        var otp = CreateOtpWithCreatedAt(
            new OtpBuilder().WithUserId(userId).WithCode(correctCode).WithPurpose(purpose)
        );

        _context.Otps.Add(otp);
        await _context.SaveChangesAsync();

        var initialAttemptCount = otp.AttemptCount;

        // Act
        Func<Task> act = async () => await _repository.ValidateOtpAsync(userId, wrongCode, purpose);

        // Assert
        await act.Should().ThrowAsync<BadRequestException>();

        var updatedOtp = await _context.Otps.FirstOrDefaultAsync(o => o.Id == otp.Id);
        updatedOtp!.AttemptCount.Should().Be(initialAttemptCount + 1);
    }

    [Fact]
    public async Task ValidateOtpAsync_WhenInvalidCodeReachesMaxAttempts_ShouldThrowOtpAttemptsLimitException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var correctCode = "123456";
        var wrongCode = "654321";
        var purpose = EnumOtpPurpose.EmailVerification;

        var otp = CreateOtpWithCreatedAt(
            new OtpBuilder().WithUserId(userId).WithCode(correctCode).WithPurpose(purpose).WithAttemptCount(2) // One less than max (max = 3)
        );

        _context.Otps.Add(otp);
        await _context.SaveChangesAsync();

        // Act
        Func<Task> act = async () => await _repository.ValidateOtpAsync(userId, wrongCode, purpose);

        // Assert
        await act.Should().ThrowAsync<OtpAttemptsLimitException>();
    }

    [Fact]
    public async Task ValidateOtpAsync_WhenNoMatchingOtpFound_ShouldCheckLatestValidOtp()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var correctCode = "123456";
        var wrongCode = "654321";
        var purpose = EnumOtpPurpose.EmailVerification;

        var latestOtp = CreateOtpWithCreatedAt(
            new OtpBuilder().WithUserId(userId).WithCode(correctCode).WithPurpose(purpose)
        );

        _context.Otps.Add(latestOtp);
        await _context.SaveChangesAsync();

        // Act - Try with wrong code
        Func<Task> act = async () => await _repository.ValidateOtpAsync(userId, wrongCode, purpose);

        // Assert
        await act.Should().ThrowAsync<BadRequestException>();

        var updatedOtp = await _context.Otps.FirstOrDefaultAsync(o => o.Id == latestOtp.Id);
        updatedOtp!.AttemptCount.Should().Be(1);
    }

    [Fact]
    public async Task ValidateOtpAsync_WhenLatestOtpIsExpired_ShouldThrowNotFoundException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var code = "123456";
        var wrongCode = "654321";
        var purpose = EnumOtpPurpose.EmailVerification;

        var latestOtp = CreateOtpWithCreatedAt(
            new OtpBuilder().WithUserId(userId).WithCode(code).WithPurpose(purpose).AsExpired()
        );

        _context.Otps.Add(latestOtp);
        await _context.SaveChangesAsync();

        // Act - Expired OTPs are filtered out by specification, so no valid OTP is found
        Func<Task> act = async () => await _repository.ValidateOtpAsync(userId, wrongCode, purpose);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task ValidateOtpAsync_WhenLatestOtpHasMaxAttempts_ShouldThrowOtpAttemptsLimitException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var code = "123456";
        var wrongCode = "654321";
        var purpose = EnumOtpPurpose.EmailVerification;

        var latestOtp = CreateOtpWithCreatedAt(
            new OtpBuilder().WithUserId(userId).WithCode(code).WithPurpose(purpose).AsMaxAttemptsReached()
        );

        _context.Otps.Add(latestOtp);
        await _context.SaveChangesAsync();

        // Act
        Func<Task> act = async () => await _repository.ValidateOtpAsync(userId, wrongCode, purpose);

        // Assert
        await act.Should().ThrowAsync<OtpAttemptsLimitException>();
    }

    [Fact]
    public async Task ValidateOtpAsync_WhenNoValidOtpFound_ShouldThrowNotFoundException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var code = "123456";
        var purpose = EnumOtpPurpose.EmailVerification;

        // Act - No OTP exists
        Func<Task> act = async () => await _repository.ValidateOtpAsync(userId, code, purpose);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task ValidateOtpAsync_WhenLatestOtpWrongCodeReachesMaxAttempts_ShouldThrowOtpAttemptsLimitException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var correctCode = "123456";
        var wrongCode = "654321";
        var purpose = EnumOtpPurpose.EmailVerification;

        var latestOtp = CreateOtpWithCreatedAt(
            new OtpBuilder().WithUserId(userId).WithCode(correctCode).WithPurpose(purpose).WithAttemptCount(2) // One less than max (max = 3)
        );

        _context.Otps.Add(latestOtp);
        await _context.SaveChangesAsync();

        // Act - Wrong code that will increment to max
        Func<Task> act = async () => await _repository.ValidateOtpAsync(userId, wrongCode, purpose);

        // Assert
        await act.Should().ThrowAsync<OtpAttemptsLimitException>();

        var updatedOtp = await _context.Otps.FirstOrDefaultAsync(o => o.Id == latestOtp.Id);
        updatedOtp!.AttemptCount.Should().Be(3);
    }

    [Fact]
    public async Task ValidateOtpAsync_ShouldReturnMostRecentMatchingOtp()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var code = "123456";
        var purpose = EnumOtpPurpose.EmailVerification;

        var olderOtp = CreateOtpWithCreatedAt(new OtpBuilder().WithUserId(userId).WithCode(code).WithPurpose(purpose));
        typeof(OtpEntity).GetProperty("CreatedAt")!.SetValue(olderOtp, DateTime.UtcNow.AddMinutes(-10));

        var newerOtp = CreateOtpWithCreatedAt(new OtpBuilder().WithUserId(userId).WithCode(code).WithPurpose(purpose));
        typeof(OtpEntity).GetProperty("CreatedAt")!.SetValue(newerOtp, DateTime.UtcNow);

        _context.Otps.AddRange(olderOtp, newerOtp);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.ValidateOtpAsync(userId, code, purpose);

        // Assert
        result.Id.Should().Be(newerOtp.Id);
    }

    #endregion

    #region ValidateUsedOtpAsync Tests

    [Fact]
    public async Task ValidateUsedOtpAsync_WhenOtpExistsAndIsUsed_ShouldReturnOtp()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var code = "123456";
        var purpose = EnumOtpPurpose.EmailVerification;

        var otp = CreateOtpWithCreatedAt(
            new OtpBuilder().WithUserId(userId).WithCode(code).WithPurpose(purpose).AsUsed()
        );

        _context.Otps.Add(otp);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.ValidateUsedOtpAsync(userId, code, purpose);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(otp.Id);
    }

    [Fact]
    public async Task ValidateUsedOtpAsync_WhenOtpDoesNotExist_ShouldThrowBadRequestException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var code = "123456";
        var purpose = EnumOtpPurpose.EmailVerification;

        // Act
        Func<Task> act = async () => await _repository.ValidateUsedOtpAsync(userId, code, purpose);

        // Assert
        await act.Should().ThrowAsync<BadRequestException>();
    }

    [Fact]
    public async Task ValidateUsedOtpAsync_WhenOtpIsExpired_ShouldThrowOtpExpirationException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var code = "123456";
        var purpose = EnumOtpPurpose.EmailVerification;

        var otp = CreateOtpWithCreatedAt(
            new OtpBuilder().WithUserId(userId).WithCode(code).WithPurpose(purpose).AsUsed().AsExpired()
        );

        _context.Otps.Add(otp);
        await _context.SaveChangesAsync();

        // Act
        Func<Task> act = async () => await _repository.ValidateUsedOtpAsync(userId, code, purpose);

        // Assert
        await act.Should().ThrowAsync<OtpExpirationException>();
    }

    [Fact]
    public async Task ValidateUsedOtpAsync_ShouldReturnMostRecentUsedOtp()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var code = "123456";
        var purpose = EnumOtpPurpose.EmailVerification;

        var olderOtp = CreateOtpWithCreatedAt(
            new OtpBuilder().WithUserId(userId).WithCode(code).WithPurpose(purpose).AsUsed()
        );
        typeof(OtpEntity).GetProperty("CreatedAt")!.SetValue(olderOtp, DateTime.UtcNow.AddMinutes(-10));

        var newerOtp = CreateOtpWithCreatedAt(
            new OtpBuilder().WithUserId(userId).WithCode(code).WithPurpose(purpose).AsUsed()
        );
        typeof(OtpEntity).GetProperty("CreatedAt")!.SetValue(newerOtp, DateTime.UtcNow);

        _context.Otps.AddRange(olderOtp, newerOtp);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.ValidateUsedOtpAsync(userId, code, purpose);

        // Assert
        result.Id.Should().Be(newerOtp.Id);
    }

    #endregion

    #region InvalidateExistingOtpsAsync Tests

    [Fact]
    public async Task InvalidateExistingOtpsAsync_WhenOtpsExist_ShouldMarkThemAsUsed()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var purpose = EnumOtpPurpose.EmailVerification;

        var otp1 = CreateOtpWithCreatedAt(new OtpBuilder().WithUserId(userId).WithPurpose(purpose));

        var otp2 = CreateOtpWithCreatedAt(new OtpBuilder().WithUserId(userId).WithPurpose(purpose));

        var otherUserOtp = CreateOtpWithCreatedAt(new OtpBuilder().WithUserId(Guid.NewGuid()).WithPurpose(purpose));

        _context.Otps.AddRange(otp1, otp2, otherUserOtp);
        await _context.SaveChangesAsync();

        // Act
        await _repository.InvalidateExistingOtpsAsync(userId, purpose);
        await _context.SaveChangesAsync();

        // Assert
        var updatedOtp1 = await _context.Otps.FirstOrDefaultAsync(o => o.Id == otp1.Id);
        updatedOtp1!.IsUsed.Should().BeTrue();

        var updatedOtp2 = await _context.Otps.FirstOrDefaultAsync(o => o.Id == otp2.Id);
        updatedOtp2!.IsUsed.Should().BeTrue();

        var updatedOtherUserOtp = await _context.Otps.FirstOrDefaultAsync(o => o.Id == otherUserOtp.Id);
        updatedOtherUserOtp!.IsUsed.Should().BeFalse();
    }

    [Fact]
    public async Task InvalidateExistingOtpsAsync_WhenNoOtpsExist_ShouldNotThrow()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var purpose = EnumOtpPurpose.EmailVerification;

        // Act
        Func<Task> act = async () => await _repository.InvalidateExistingOtpsAsync(userId, purpose);

        // Assert
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task InvalidateExistingOtpsAsync_ShouldOnlyInvalidateSpecificPurpose()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var emailVerificationOtp = CreateOtpWithCreatedAt(
            new OtpBuilder().WithUserId(userId).WithPurpose(EnumOtpPurpose.EmailVerification)
        );

        var passwordResetOtp = CreateOtpWithCreatedAt(
            new OtpBuilder().WithUserId(userId).WithPurpose(EnumOtpPurpose.PasswordReset)
        );

        _context.Otps.AddRange(emailVerificationOtp, passwordResetOtp);
        await _context.SaveChangesAsync();

        // Act
        await _repository.InvalidateExistingOtpsAsync(userId, EnumOtpPurpose.EmailVerification);
        await _context.SaveChangesAsync();

        // Assert
        var updatedEmailOtp = await _context.Otps.FirstOrDefaultAsync(o => o.Id == emailVerificationOtp.Id);
        updatedEmailOtp!.IsUsed.Should().BeTrue();

        var updatedPasswordOtp = await _context.Otps.FirstOrDefaultAsync(o => o.Id == passwordResetOtp.Id);
        updatedPasswordOtp!.IsUsed.Should().BeFalse();
    }

    #endregion

    #region CleanupExpiredOtpsAsync Tests

    [Fact]
    public async Task CleanupExpiredOtpsAsync_WhenExpiredOtpsExist_ShouldRemoveThemAndReturnCount()
    {
        // Arrange
        var expiredOtp1 = CreateOtpWithCreatedAt(new OtpBuilder().AsExpired());

        var expiredOtp2 = CreateOtpWithCreatedAt(new OtpBuilder().AsExpired());

        var activeOtp = CreateOtpWithCreatedAt(new OtpBuilder());

        _context.Otps.AddRange(expiredOtp1, expiredOtp2, activeOtp);
        await _context.SaveChangesAsync();

        // Act
        var count = await _repository.CleanupExpiredOtpsAsync();
        await _context.SaveChangesAsync();

        // Assert
        count.Should().Be(2);

        var remainingOtps = await _context.Otps.ToListAsync();
        remainingOtps.Should().HaveCount(1);
        remainingOtps.First().Id.Should().Be(activeOtp.Id);
    }

    [Fact]
    public async Task CleanupExpiredOtpsAsync_WhenNoExpiredOtps_ShouldReturnZero()
    {
        // Arrange
        var activeOtp = CreateOtpWithCreatedAt(new OtpBuilder());

        _context.Otps.Add(activeOtp);
        await _context.SaveChangesAsync();

        // Act
        var count = await _repository.CleanupExpiredOtpsAsync();

        // Assert
        count.Should().Be(0);

        var remainingOtps = await _context.Otps.ToListAsync();
        remainingOtps.Should().HaveCount(1);
    }

    [Fact]
    public async Task CleanupExpiredOtpsAsync_WhenNoOtpsExist_ShouldReturnZero()
    {
        // Arrange - No OTPs in database

        // Act
        var count = await _repository.CleanupExpiredOtpsAsync();

        // Assert
        count.Should().Be(0);
    }

    #endregion
}
