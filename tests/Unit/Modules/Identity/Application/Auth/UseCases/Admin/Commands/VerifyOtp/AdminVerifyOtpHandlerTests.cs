using _116.Identity.Application.Auth.Repositories;
using _116.Identity.Application.Auth.UseCases.Admin.Commands.VerifyOtp;
using _116.Identity.Application.Shared.Persistence;
using _116.Identity.Application.Shared.Repositories;
using _116.Identity.Domain.Entities;
using _116.Identity.Domain.Enums;
using _116.Identity.Domain.ValueObjects;
using _116.Shared.Application.Exceptions;
using _116.Tests.Fixtures.Factories;
using _116.Unit.Tests.Common.Mocks.Infrastructure;
using _116.Unit.Tests.Common.Mocks.Repositories;
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
    private readonly Mock<IIdentityUnitOfWork> _unitOfWorkMock;
    private readonly AdminVerifyOtpHandler _handler;

    public AdminVerifyOtpHandlerTests()
    {
        _authRepositoryMock = MockAuthRepository.Create();
        _otpRepositoryMock = MockOtpRepository.Create();
        _unitOfWorkMock = MockIdentityUnitOfWork.Create();

        _handler = new AdminVerifyOtpHandler(
            _authRepositoryMock.Object,
            _otpRepositoryMock.Object,
            _unitOfWorkMock.Object
        );
    }

    #region Success Cases

    [Fact]
    public async Task Handle_WithValidOtp_ShouldReturnSuccess()
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
        _otpRepositoryMock.SetupValidateOtp(otp);

        // Act
        AdminVerifyOtpResult result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
    }

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
        _otpRepositoryMock.SetupValidateOtp(otp);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert - OTP should be marked as used
        otp.IsUsed.Should().BeTrue();
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
        _otpRepositoryMock.SetupValidateOtp(otp);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _otpRepositoryMock.Verify(
            x => x.InvalidateExistingOtpsAsync(user.Id, It.IsAny<EnumOtpPurpose>(), It.IsAny<CancellationToken>()),
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
        _otpRepositoryMock.SetupValidateOtp(otp);

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
        _otpRepositoryMock.SetupValidateOtp(otp);

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
        _otpRepositoryMock.SetupValidateOtpNotFound(user.Id, code, EnumOtpPurpose.EmailVerification);

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
        _otpRepositoryMock.SetupValidateOtpInvalidCode(user.Id, code, EnumOtpPurpose.EmailVerification);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<BadRequestException>();
    }

    [Fact]
    public async Task Handle_WhenOtpExpired_ShouldThrowAuthenticationException()
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
        _otpRepositoryMock.SetupValidateOtpExpired(user.Id, code, EnumOtpPurpose.EmailVerification);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<AuthenticationException>();
    }

    [Fact]
    public async Task Handle_WhenOtpInvalid_ShouldNotCommit()
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
        _otpRepositoryMock.SetupValidateOtpInvalidCode(user.Id, code, EnumOtpPurpose.EmailVerification);

        // Act
        try
        {
            await _handler.Handle(command, CancellationToken.None);
        }
        catch (BadRequestException)
        {
            // Expected
        }

        // Assert
        _unitOfWorkMock.VerifyCommitNotCalled();
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
        _otpRepositoryMock.SetupValidateOtp(otp);

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
        _otpRepositoryMock.SetupValidateOtp(otp);

        // Act
        await _handler.Handle(command, cts.Token);

        // Assert
        _otpRepositoryMock.Verify(
            x => x.ValidateOtpAsync(user.Id, code, It.IsAny<EnumOtpPurpose>(), cts.Token),
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
        _otpRepositoryMock.SetupValidateOtp(otp);

        // Act
        await _handler.Handle(command, cts.Token);

        // Assert
        _unitOfWorkMock.Verify(x => x.CommitAsync(cts.Token), Times.Once);
    }

    #endregion
}
