using _116.BuildingBlocks.Application.Exceptions;
using _116.Identity.Application.Auth.Exceptions;
using _116.Identity.Application.Auth.Ports;
using _116.Identity.Application.Auth.Repositories;
using _116.Identity.Application.Auth.Services;
using _116.Identity.Application.Shared.Persistence;
using _116.Identity.Application.Shared.Repositories;
using _116.Identity.Domain.Entities;
using _116.Identity.Domain.Enums;
using _116.Identity.Domain.ValueObjects;
using _116.Identity.TestData.Factories;
using _116.Identity.TestData.Mocks.Infrastructure;
using _116.Identity.TestData.Mocks.Repositories;
using _116.Identity.TestData.Mocks.Services;
using _116.Tests.TestData.Helpers;
using AwesomeAssertions;
using Moq;
using Xunit;

namespace _116.Identity.Unit.Tests.Application.Auth.Services;

/// <summary>
/// Unit tests for <see cref="OtpVerificationService"/>: the consumption protocol shared by the
/// VerifyOtp handlers.
/// </summary>
public class OtpVerificationServiceTests
{
    private const string Code = "123456";
    private static readonly OtpPurpose Purpose = new(EnumOtpPurpose.EmailVerification);

    private readonly Guid _userId = Guid.NewGuid();
    private readonly Mock<IOtpRepository> _otpRepositoryMock;
    private readonly Mock<IOtpService> _otpServiceMock;
    private readonly Mock<IAccountLockoutRepository> _lockoutRepositoryMock;
    private readonly Mock<IIdentityUnitOfWork> _unitOfWorkMock;
    private readonly OtpVerificationService _service;

    public OtpVerificationServiceTests()
    {
        _otpRepositoryMock = MockOtpRepository.Create();
        _otpServiceMock = MockOtpService.Create();
        _lockoutRepositoryMock = new Mock<IAccountLockoutRepository>();
        _unitOfWorkMock = MockIdentityUnitOfWork.Create();

        _service = new OtpVerificationService(
            _otpRepositoryMock.Object,
            _otpServiceMock.Object,
            _lockoutRepositoryMock.Object,
            _unitOfWorkMock.Object,
            TimeProvider.System,
            TestErrorsFactory.CreateIdentityI18n()
        );
    }

    #region Valid Code

    [Fact]
    public async Task ConsumeOtpAsync_WithAValidCode_ShouldMarkTheOtpUsed()
    {
        // Arrange
        OtpEntity otp = OtpFactory.Create(_userId, Code);
        _otpRepositoryMock.SetupGetLatestOutstandingOtp(otp);
        _otpServiceMock.SetupVerifySuccess(Code);

        // Act
        await _service.ConsumeOtpAsync(_userId, Purpose, Code, CancellationToken.None);

        // Assert
        otp.IsUsed.Should().BeTrue();
    }

    [Fact]
    public async Task ConsumeOtpAsync_WithAValidCode_ShouldInvalidateTheSiblingOtps()
    {
        // Arrange
        OtpEntity otp = OtpFactory.Create(_userId, Code);
        _otpRepositoryMock.SetupGetLatestOutstandingOtp(otp);
        _otpServiceMock.SetupVerifySuccess(Code);

        // Act
        await _service.ConsumeOtpAsync(_userId, Purpose, Code, CancellationToken.None);

        // Assert
        _otpRepositoryMock.Verify(
            x =>
                x.InvalidateExistingOtpsAsync(
                    _userId,
                    EnumOtpPurpose.EmailVerification,
                    otp.Id,
                    It.IsAny<CancellationToken>()
                ),
            Times.Once
        );
    }

    [Fact]
    public async Task ConsumeOtpAsync_WithAValidCode_ShouldClearTheAccountFailureCounter()
    {
        // Arrange
        OtpEntity otp = OtpFactory.Create(_userId, Code);
        _otpRepositoryMock.SetupGetLatestOutstandingOtp(otp);
        _otpServiceMock.SetupVerifySuccess(Code);

        // Act
        await _service.ConsumeOtpAsync(_userId, Purpose, Code, CancellationToken.None);

        // Assert
        _lockoutRepositoryMock.Verify(x => x.ClearFailedOtpAsync(_userId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ConsumeOtpAsync_WithAValidCode_ShouldLeaveTheCommitToTheCaller()
    {
        // Arrange
        OtpEntity otp = OtpFactory.Create(_userId, Code);
        _otpRepositoryMock.SetupGetLatestOutstandingOtp(otp);
        _otpServiceMock.SetupVerifySuccess(Code);

        // Act
        await _service.ConsumeOtpAsync(_userId, Purpose, Code, CancellationToken.None);

        // Assert
        _unitOfWorkMock.VerifyCommitNotCalled();
    }

    [Fact]
    public async Task ConsumeOtpAsync_WithCancellationToken_ShouldPassItToTheOtpRepository()
    {
        // Arrange
        OtpEntity otp = OtpFactory.Create(_userId, Code);
        using CancellationTokenSource cts = new();
        _otpRepositoryMock.SetupGetLatestOutstandingOtp(otp);
        _otpServiceMock.SetupVerifySuccess(Code);

        // Act
        await _service.ConsumeOtpAsync(_userId, Purpose, Code, cts.Token);

        // Assert
        _otpRepositoryMock.Verify(
            x => x.GetLatestOutstandingOtpOrThrowAsync(_userId, EnumOtpPurpose.EmailVerification, cts.Token),
            Times.Once
        );
    }

    #endregion

    #region Missed Code

    [Fact]
    public async Task ConsumeOtpAsync_WhenNoOtpIsOutstanding_ShouldThrowNotFoundException()
    {
        // Arrange
        _otpRepositoryMock.SetupGetLatestOutstandingOtpNotFound(_userId, EnumOtpPurpose.EmailVerification);

        // Act
        Func<Task> act = async () => await _service.ConsumeOtpAsync(_userId, Purpose, Code, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task ConsumeOtpAsync_WithAWrongCode_ShouldMeterAndCommitTheMissBeforeThrowing()
    {
        // Arrange
        OtpEntity otp = OtpFactory.Create(_userId, Code);
        _otpRepositoryMock.SetupGetLatestOutstandingOtp(otp);
        _otpServiceMock.SetupVerifyFailure("wrong-code");

        // Act
        Func<Task> act = async () =>
            await _service.ConsumeOtpAsync(_userId, Purpose, "wrong-code", CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<BadRequestException>();
        otp.AttemptCount.Should().Be(1);
        _lockoutRepositoryMock.Verify(
            x => x.RegisterFailedOtpAsync(_userId, It.IsAny<CancellationToken>()),
            Times.Once
        );
        _unitOfWorkMock.VerifyCommitCalled();
    }

    [Fact]
    public async Task ConsumeOtpAsync_WithAWrongCode_ShouldNotConsumeTheOtp()
    {
        // Arrange
        OtpEntity otp = OtpFactory.Create(_userId, Code);
        _otpRepositoryMock.SetupGetLatestOutstandingOtp(otp);
        _otpServiceMock.SetupVerifyFailure("wrong-code");

        // Act
        Func<Task> act = async () =>
            await _service.ConsumeOtpAsync(_userId, Purpose, "wrong-code", CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<BadRequestException>();
        otp.IsUsed.Should().BeFalse();
        _otpRepositoryMock.Verify(
            x =>
                x.InvalidateExistingOtpsAsync(
                    It.IsAny<Guid>(),
                    It.IsAny<EnumOtpPurpose>(),
                    It.IsAny<Guid?>(),
                    It.IsAny<CancellationToken>()
                ),
            Times.Never
        );
    }

    [Fact]
    public async Task ConsumeOtpAsync_WithAnExpiredOtp_ShouldThrowWithoutConsumingAnAttempt()
    {
        // Arrange
        OtpEntity otp = OtpFactory.CreateExpired(_userId, EnumOtpPurpose.EmailVerification);
        _otpRepositoryMock.SetupGetLatestOutstandingOtp(otp);
        _otpServiceMock.SetupVerifySuccess(Code);

        // Act
        Func<Task> act = async () => await _service.ConsumeOtpAsync(_userId, Purpose, Code, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<OtpExpirationException>();
        otp.AttemptCount.Should().Be(0);
        _lockoutRepositoryMock.Verify(
            x => x.RegisterFailedOtpAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
            Times.Never
        );
        _unitOfWorkMock.VerifyCommitNotCalled();
    }

    [Fact]
    public async Task ConsumeOtpAsync_WhenTheAttemptsAreExhausted_ShouldThrowOtpAttemptsLimitException()
    {
        // Arrange
        OtpEntity otp = OtpFactory.CreateMaxAttemptsReached(_userId, Code, EnumOtpPurpose.EmailVerification);
        _otpRepositoryMock.SetupGetLatestOutstandingOtp(otp);
        _otpServiceMock.SetupVerifySuccess(Code);

        // Act
        Func<Task> act = async () => await _service.ConsumeOtpAsync(_userId, Purpose, Code, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<OtpAttemptsLimitException>();
        otp.IsUsed.Should().BeFalse();
    }

    #endregion
}
