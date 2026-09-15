using _116.Identity.Application.Auth.Exceptions;
using _116.Identity.Application.Auth.Factories;
using _116.Identity.Application.Auth.Repositories;
using _116.Identity.Application.Auth.Services;
using _116.Identity.Application.Auth.UseCases.Admin.Commands.VerifyOtp;
using _116.Identity.Application.Shared.Persistence;
using _116.Identity.Application.Shared.Repositories;
using _116.Identity.Domain.Entities;
using _116.Identity.Domain.Enums;
using _116.Identity.Domain.ValueObjects;
using _116.Shared.Application.Exceptions;
using _116.Tests.Fixtures.Factories.Identity;
using _116.Tests.Fixtures.Helpers;
using _116.Unit.Tests.Common.Mocks.Infrastructure;
using _116.Unit.Tests.Common.Mocks.Repositories;
using _116.Unit.Tests.Common.Mocks.Services;
using AwesomeAssertions;
using Moq;
using Xunit;

namespace _116.Unit.Tests.Modules.Identity.Application.Auth.UseCases.Admin.Commands.VerifyOtp;

/// <summary>
/// Unit tests for <see cref="AdminVerifyOtpHandler"/>.
/// </summary>
public class AdminVerifyOtpHandlerTests
{
    private readonly Mock<IAuthRepository> _authRepositoryMock;
    private readonly Mock<IOtpRepository> _otpRepositoryMock;
    private readonly Mock<IOtpService> _otpServiceMock;
    private readonly Mock<IAccountLockoutRepository> _lockoutRepositoryMock;
    private readonly Mock<IIdentityUnitOfWork> _unitOfWorkMock;
    private readonly AdminVerifyOtpHandler _handler;

    public AdminVerifyOtpHandlerTests()
    {
        _authRepositoryMock = MockAuthRepository.Create();
        _otpRepositoryMock = MockOtpRepository.Create();
        _otpServiceMock = MockOtpService.Create();
        _lockoutRepositoryMock = new Mock<IAccountLockoutRepository>();
        _unitOfWorkMock = MockIdentityUnitOfWork.Create();

        var otpVerificationFactory = new OtpVerificationFactory(
            _otpServiceMock.Object,
            _lockoutRepositoryMock.Object,
            _unitOfWorkMock.Object,
            TimeProvider.System,
            TestErrorsFactory.CreateIdentityI18n()
        );

        _handler = new AdminVerifyOtpHandler(
            _authRepositoryMock.Object,
            _otpRepositoryMock.Object,
            otpVerificationFactory,
            _lockoutRepositoryMock.Object,
            _unitOfWorkMock.Object,
            TimeProvider.System
        );
    }

    #region Success Cases

    [Fact]
    public async Task Handle_ShouldMarkOtpAsUsed()
    {
        // Arrange
        string email = "admin@example.com";
        string code = "123456";
        string purpose = EnumOtpPurpose.EmailVerification.ToString();
        UserEntity user = UserFactory.CreateVerifiedActive();
        OtpEntity otp = OtpFactory.Create(user.Id, code);

        AdminVerifyOtpCommand command = new(Email: email, Code: code, Purpose: purpose);

        _authRepositoryMock.SetupGetUserWithRolesByEmailOrThrow(new Email(email), user);
        _authRepositoryMock.SetupIsUserAdminReturnsTrue();
        _authRepositoryMock.SetupIsUserAccountActiveReturnsTrue();
        _otpRepositoryMock.SetupGetLatestOutstandingOtp(otp);
        _otpServiceMock.SetupVerifySuccess(code);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert - OTP should be marked as used
        otp.IsUsed.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_ShouldMarkUserAsVerified()
    {
        // Arrange
        string email = "admin@example.com";
        string code = "123456";
        string purpose = EnumOtpPurpose.EmailVerification.ToString();
        UserEntity user = UserFactory.CreateUnverified();
        OtpEntity otp = OtpFactory.Create(user.Id, code);

        AdminVerifyOtpCommand command = new(Email: email, Code: code, Purpose: purpose);

        _authRepositoryMock.SetupGetUserWithRolesByEmailOrThrow(new Email(email), user);
        _authRepositoryMock.SetupIsUserAdminReturnsTrue();
        _authRepositoryMock.SetupIsUserAccountActiveReturnsTrue();
        _otpRepositoryMock.SetupGetLatestOutstandingOtp(otp);
        _otpServiceMock.SetupVerifySuccess(code);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        user.IsVerified.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_WithAPasswordResetPurpose_ShouldNotMarkUserAsVerified()
    {
        // Arrange
        string email = "admin@example.com";
        string code = "123456";
        string purpose = EnumOtpPurpose.PasswordReset.ToString();
        UserEntity user = UserFactory.CreateUnverified();
        OtpEntity otp = OtpFactory.Create(user.Id, code, EnumOtpPurpose.PasswordReset);

        AdminVerifyOtpCommand command = new(Email: email, Code: code, Purpose: purpose);

        _authRepositoryMock.SetupGetUserWithRolesByEmailOrThrow(new Email(email), user);
        _authRepositoryMock.SetupIsUserAdminReturnsTrue();
        _authRepositoryMock.SetupIsUserAccountActiveReturnsTrue();
        _otpRepositoryMock.SetupGetLatestOutstandingOtp(otp);
        _otpServiceMock.SetupVerifySuccess(code);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        user.IsVerified.Should().BeFalse();
        otp.IsUsed.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_ShouldClearTheAccountOtpFailureCounter()
    {
        // Arrange
        string email = "admin@example.com";
        string code = "123456";
        string purpose = EnumOtpPurpose.EmailVerification.ToString();
        UserEntity user = UserFactory.CreateVerifiedActive();
        OtpEntity otp = OtpFactory.Create(user.Id, code);

        AdminVerifyOtpCommand command = new(Email: email, Code: code, Purpose: purpose);

        _authRepositoryMock.SetupGetUserWithRolesByEmailOrThrow(new Email(email), user);
        _authRepositoryMock.SetupIsUserAdminReturnsTrue();
        _authRepositoryMock.SetupIsUserAccountActiveReturnsTrue();
        _otpRepositoryMock.SetupGetLatestOutstandingOtp(otp);
        _otpServiceMock.SetupVerifySuccess(code);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _lockoutRepositoryMock.Verify(x => x.ClearFailedOtpAsync(user.Id, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldInvalidateExistingOtps()
    {
        // Arrange
        string email = "admin@example.com";
        string code = "123456";
        string purpose = EnumOtpPurpose.EmailVerification.ToString();
        UserEntity user = UserFactory.CreateVerifiedActive();
        OtpEntity otp = OtpFactory.Create(user.Id, code);

        AdminVerifyOtpCommand command = new(Email: email, Code: code, Purpose: purpose);

        _authRepositoryMock.SetupGetUserWithRolesByEmailOrThrow(new Email(email), user);
        _authRepositoryMock.SetupIsUserAdminReturnsTrue();
        _authRepositoryMock.SetupIsUserAccountActiveReturnsTrue();
        _otpRepositoryMock.SetupGetLatestOutstandingOtp(otp);
        _otpServiceMock.SetupVerifySuccess(code);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _otpRepositoryMock.Verify(
            x =>
                x.InvalidateExistingOtpsAsync(
                    user.Id,
                    It.IsAny<EnumOtpPurpose>(),
                    It.IsAny<Guid?>(),
                    It.IsAny<CancellationToken>()
                ),
            Times.Once
        );
    }

    [Fact]
    public async Task Handle_ShouldCommitUnitOfWork()
    {
        // Arrange
        string email = "admin@example.com";
        string code = "123456";
        string purpose = EnumOtpPurpose.EmailVerification.ToString();
        UserEntity user = UserFactory.CreateVerifiedActive();
        OtpEntity otp = OtpFactory.Create(user.Id, code);

        AdminVerifyOtpCommand command = new(Email: email, Code: code, Purpose: purpose);

        _authRepositoryMock.SetupGetUserWithRolesByEmailOrThrow(new Email(email), user);
        _authRepositoryMock.SetupIsUserAdminReturnsTrue();
        _authRepositoryMock.SetupIsUserAccountActiveReturnsTrue();
        _otpRepositoryMock.SetupGetLatestOutstandingOtp(otp);
        _otpServiceMock.SetupVerifySuccess(code);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _unitOfWorkMock.VerifyCommitCalled();
    }

    [Fact]
    public async Task Handle_ShouldValidateUserIsAdmin()
    {
        // Arrange
        string email = "admin@example.com";
        string code = "123456";
        string purpose = EnumOtpPurpose.EmailVerification.ToString();
        UserEntity user = UserFactory.CreateVerifiedActive();
        OtpEntity otp = OtpFactory.Create(user.Id, code);

        AdminVerifyOtpCommand command = new(Email: email, Code: code, Purpose: purpose);

        _authRepositoryMock.SetupGetUserWithRolesByEmailOrThrow(new Email(email), user);
        _authRepositoryMock.SetupIsUserAdminReturnsTrue();
        _authRepositoryMock.SetupIsUserAccountActiveReturnsTrue();
        _otpRepositoryMock.SetupGetLatestOutstandingOtp(otp);
        _otpServiceMock.SetupVerifySuccess(code);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _authRepositoryMock.Verify(x => x.IsUserAdmin(It.IsAny<UserEntity>()), Times.Once);
    }

    #endregion

    #region Failure Cases

    [Fact]
    public async Task Handle_WhenUserNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        string email = "nonexistent@example.com";
        AdminVerifyOtpCommand command = new(Email: email, Code: "123456", Purpose: "EmailVerification");

        _authRepositoryMock.SetupGetUserWithRolesByEmailOrThrowNotFound(new Email(email));

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_WhenOtpNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        string email = "admin@example.com";
        string code = "999999";
        string purpose = EnumOtpPurpose.EmailVerification.ToString();
        UserEntity user = UserFactory.CreateVerifiedActive();

        AdminVerifyOtpCommand command = new(Email: email, Code: code, Purpose: purpose);

        _authRepositoryMock.SetupGetUserWithRolesByEmailOrThrow(new Email(email), user);
        _authRepositoryMock.SetupIsUserAdminReturnsTrue();
        _authRepositoryMock.SetupIsUserAccountActiveReturnsTrue();
        _otpRepositoryMock.SetupGetLatestOutstandingOtpNotFound(user.Id, EnumOtpPurpose.EmailVerification);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_WhenOtpInvalid_ShouldThrowBadRequestException()
    {
        // Arrange
        string email = "admin@example.com";
        string code = "wrong-code";
        string purpose = EnumOtpPurpose.EmailVerification.ToString();
        UserEntity user = UserFactory.CreateVerifiedActive();

        AdminVerifyOtpCommand command = new(Email: email, Code: code, Purpose: purpose);

        _authRepositoryMock.SetupGetUserWithRolesByEmailOrThrow(new Email(email), user);
        _authRepositoryMock.SetupIsUserAdminReturnsTrue();
        _authRepositoryMock.SetupIsUserAccountActiveReturnsTrue();
        OtpEntity otp = OtpFactory.Create(user.Id, "123456");
        _otpRepositoryMock.SetupGetLatestOutstandingOtp(otp);
        _otpServiceMock.SetupVerifyFailure(code);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<BadRequestException>();
    }

    [Fact]
    public async Task Handle_WhenOtpExpired_ShouldThrowOtpExpirationException()
    {
        // Arrange
        string email = "admin@example.com";
        string code = "123456";
        string purpose = EnumOtpPurpose.EmailVerification.ToString();
        UserEntity user = UserFactory.CreateVerifiedActive();

        AdminVerifyOtpCommand command = new(Email: email, Code: code, Purpose: purpose);

        _authRepositoryMock.SetupGetUserWithRolesByEmailOrThrow(new Email(email), user);
        _authRepositoryMock.SetupIsUserAdminReturnsTrue();
        _authRepositoryMock.SetupIsUserAccountActiveReturnsTrue();
        OtpEntity otp = OtpFactory.CreateExpired(user.Id, EnumOtpPurpose.EmailVerification);
        _otpRepositoryMock.SetupGetLatestOutstandingOtp(otp);
        _otpServiceMock.SetupVerifySuccess(code);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert — expiry is judged before the code and consumes nothing
        await act.Should().ThrowAsync<OtpExpirationException>();
        otp.AttemptCount.Should().Be(0);
        _unitOfWorkMock.VerifyCommitNotCalled();
    }

    [Fact]
    public async Task Handle_WhenOtpInvalid_ShouldCommitTheConsumedAttemptBeforeThrowing()
    {
        // Arrange
        string email = "admin@example.com";
        string code = "wrong-code";
        string purpose = EnumOtpPurpose.EmailVerification.ToString();
        UserEntity user = UserFactory.CreateVerifiedActive();
        OtpEntity otp = OtpFactory.Create(user.Id, "123456");

        AdminVerifyOtpCommand command = new(Email: email, Code: code, Purpose: purpose);

        _authRepositoryMock.SetupGetUserWithRolesByEmailOrThrow(new Email(email), user);
        _authRepositoryMock.SetupIsUserAdminReturnsTrue();
        _authRepositoryMock.SetupIsUserAccountActiveReturnsTrue();
        _otpRepositoryMock.SetupGetLatestOutstandingOtp(otp);
        _otpServiceMock.SetupVerifyFailure(code);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert — the consumed attempt is metered and committed before the throw
        await act.Should().ThrowAsync<BadRequestException>();
        otp.AttemptCount.Should().Be(1);
        _lockoutRepositoryMock.Verify(
            x => x.RegisterFailedOtpAsync(user.Id, It.IsAny<CancellationToken>()),
            Times.Once
        );
        _unitOfWorkMock.VerifyCommitCalled();
    }

    #endregion

    #region Cancellation Token Tests

    [Fact]
    public async Task Handle_WithCancellationToken_ShouldPassToAuthRepository()
    {
        // Arrange
        string email = "admin@example.com";
        string code = "123456";
        string purpose = EnumOtpPurpose.EmailVerification.ToString();
        UserEntity user = UserFactory.CreateVerifiedActive();
        OtpEntity otp = OtpFactory.Create(user.Id, code);
        using CancellationTokenSource cts = new();

        AdminVerifyOtpCommand command = new(Email: email, Code: code, Purpose: purpose);

        _authRepositoryMock.SetupGetUserWithRolesByEmailOrThrow(new Email(email), user);
        _authRepositoryMock.SetupIsUserAdminReturnsTrue();
        _authRepositoryMock.SetupIsUserAccountActiveReturnsTrue();
        _otpRepositoryMock.SetupGetLatestOutstandingOtp(otp);
        _otpServiceMock.SetupVerifySuccess(code);

        // Act
        await _handler.Handle(command, cts.Token);

        // Assert
        _authRepositoryMock.Verify(x => x.GetUserWithRolesByEmailOrThrow(It.IsAny<Email>(), cts.Token), Times.Once);
    }

    [Fact]
    public async Task Handle_WithCancellationToken_ShouldPassToOtpRepository()
    {
        // Arrange
        string email = "admin@example.com";
        string code = "123456";
        string purpose = EnumOtpPurpose.EmailVerification.ToString();
        UserEntity user = UserFactory.CreateVerifiedActive();
        OtpEntity otp = OtpFactory.Create(user.Id, code);
        using CancellationTokenSource cts = new();

        AdminVerifyOtpCommand command = new(Email: email, Code: code, Purpose: purpose);

        _authRepositoryMock.SetupGetUserWithRolesByEmailOrThrow(new Email(email), user);
        _authRepositoryMock.SetupIsUserAdminReturnsTrue();
        _authRepositoryMock.SetupIsUserAccountActiveReturnsTrue();
        _otpRepositoryMock.SetupGetLatestOutstandingOtp(otp);
        _otpServiceMock.SetupVerifySuccess(code);

        // Act
        await _handler.Handle(command, cts.Token);

        // Assert
        _otpRepositoryMock.Verify(
            x => x.GetLatestOutstandingOtpOrThrowAsync(user.Id, It.IsAny<EnumOtpPurpose>(), cts.Token),
            Times.Once
        );
    }

    [Fact]
    public async Task Handle_WithCancellationToken_ShouldPassToUnitOfWork()
    {
        // Arrange
        string email = "admin@example.com";
        string code = "123456";
        string purpose = EnumOtpPurpose.EmailVerification.ToString();
        UserEntity user = UserFactory.CreateVerifiedActive();
        OtpEntity otp = OtpFactory.Create(user.Id, code);
        using CancellationTokenSource cts = new();

        AdminVerifyOtpCommand command = new(Email: email, Code: code, Purpose: purpose);

        _authRepositoryMock.SetupGetUserWithRolesByEmailOrThrow(new Email(email), user);
        _authRepositoryMock.SetupIsUserAdminReturnsTrue();
        _authRepositoryMock.SetupIsUserAccountActiveReturnsTrue();
        _otpRepositoryMock.SetupGetLatestOutstandingOtp(otp);
        _otpServiceMock.SetupVerifySuccess(code);

        // Act
        await _handler.Handle(command, cts.Token);

        // Assert
        _unitOfWorkMock.Verify(x => x.CommitAsync(cts.Token), Times.Once);
    }

    #endregion
}
