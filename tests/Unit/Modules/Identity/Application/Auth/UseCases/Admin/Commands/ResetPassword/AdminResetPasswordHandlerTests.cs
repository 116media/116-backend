using _116.Identity.Application.Auth.Repositories;
using _116.Identity.Application.Auth.UseCases.Admin.Commands.ResetPassword;
using _116.Identity.Application.Auth.UseCases.Admin.Commands.ResetPassword.Contracts;
using _116.Identity.Domain.Entities;
using _116.Identity.Domain.Enums;
using _116.Shared.Application.Exceptions;
using _116.Tests.Fixtures.Factories;
using _116.Unit.Tests.Common.Mocks.Repositories;
using AwesomeAssertions;
using Moq;
using Xunit;

namespace _116.Unit.Tests.Modules.Identity.Application.Auth.UseCases.Admin.Commands.ResetPassword;

/// <summary>
/// Unit tests for <see cref="AdminResetPasswordHandler"/>.
/// </summary>
public class AdminResetPasswordHandlerTests
{
    private readonly Mock<IAdminResetPasswordAuthFactory> _authFactoryMock;
    private readonly Mock<IOtpRepository> _otpRepositoryMock;
    private readonly AdminResetPasswordHandler _handler;

    public AdminResetPasswordHandlerTests()
    {
        _authFactoryMock = new Mock<IAdminResetPasswordAuthFactory>();
        _otpRepositoryMock = MockOtpRepository.Create();

        _handler = new AdminResetPasswordHandler(_authFactoryMock.Object, _otpRepositoryMock.Object);
    }

    #region Success Cases

    [Fact]
    public async Task Handle_WithValidOtp_ShouldReturnSuccess()
    {
        // Arrange
        string email = "admin@example.com";
        string code = "123456";
        string newPassword = "NewPassword123!";
        UserEntity user = UserFactory.CreateVerifiedActive();
        OtpEntity otp = OtpFactory.CreateUsed(user.Id);

        AdminResetPasswordCommand command = new(Email: email, Code: code, NewPassword: newPassword);
        AdminResetPasswordAuthData authData = new(User: user);

        _authFactoryMock
            .Setup(x => x.GetUserForResetAsync(email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(authData);
        _otpRepositoryMock.SetupValidateUsedOtp(otp);
        _authFactoryMock
            .Setup(x => x.ResetPasswordAsync(user, newPassword, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        // Act
        AdminResetPasswordResult result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_ShouldGetUserForReset()
    {
        // Arrange
        string email = "admin@example.com";
        string code = "123456";
        string newPassword = "NewPassword123!";
        UserEntity user = UserFactory.CreateVerifiedActive();
        OtpEntity otp = OtpFactory.CreateUsed(user.Id);

        AdminResetPasswordCommand command = new(Email: email, Code: code, NewPassword: newPassword);
        AdminResetPasswordAuthData authData = new(User: user);

        _authFactoryMock
            .Setup(x => x.GetUserForResetAsync(email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(authData);
        _otpRepositoryMock.SetupValidateUsedOtp(otp);
        _authFactoryMock
            .Setup(x => x.ResetPasswordAsync(user, newPassword, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _authFactoryMock.Verify(x => x.GetUserForResetAsync(email, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldValidateUsedOtp()
    {
        // Arrange
        string email = "admin@example.com";
        string code = "123456";
        string newPassword = "NewPassword123!";
        UserEntity user = UserFactory.CreateVerifiedActive();
        OtpEntity otp = OtpFactory.CreateUsed(user.Id);

        AdminResetPasswordCommand command = new(Email: email, Code: code, NewPassword: newPassword);
        AdminResetPasswordAuthData authData = new(User: user);

        _authFactoryMock
            .Setup(x => x.GetUserForResetAsync(email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(authData);
        _otpRepositoryMock.SetupValidateUsedOtp(otp);
        _authFactoryMock
            .Setup(x => x.ResetPasswordAsync(user, newPassword, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _otpRepositoryMock.Verify(
            x => x.ValidateUsedOtpAsync(user.Id, code, EnumOtpPurpose.PasswordReset, It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    [Fact]
    public async Task Handle_ShouldResetPassword()
    {
        // Arrange
        string email = "admin@example.com";
        string code = "123456";
        string newPassword = "NewPassword123!";
        UserEntity user = UserFactory.CreateVerifiedActive();
        OtpEntity otp = OtpFactory.CreateUsed(user.Id);

        AdminResetPasswordCommand command = new(Email: email, Code: code, NewPassword: newPassword);
        AdminResetPasswordAuthData authData = new(User: user);

        _authFactoryMock
            .Setup(x => x.GetUserForResetAsync(email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(authData);
        _otpRepositoryMock.SetupValidateUsedOtp(otp);
        _authFactoryMock
            .Setup(x => x.ResetPasswordAsync(user, newPassword, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _authFactoryMock.Verify(
            x => x.ResetPasswordAsync(user, newPassword, It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    #endregion

    #region Failure Cases

    [Fact]
    public async Task Handle_WhenUserNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        string email = "nonexistent@example.com";
        AdminResetPasswordCommand command = new(Email: email, Code: "123456", NewPassword: "NewPassword123!");

        _authFactoryMock
            .Setup(x => x.GetUserForResetAsync(email, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new NotFoundException("User not found."));

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_WhenOtpNotValid_ShouldThrowNotFoundException()
    {
        // Arrange
        string email = "admin@example.com";
        string code = "wrong-code";
        UserEntity user = UserFactory.CreateVerifiedActive();

        AdminResetPasswordCommand command = new(Email: email, Code: code, NewPassword: "NewPassword123!");
        AdminResetPasswordAuthData authData = new(User: user);

        _authFactoryMock
            .Setup(x => x.GetUserForResetAsync(email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(authData);
        _otpRepositoryMock
            .Setup(x =>
                x.ValidateUsedOtpAsync(user.Id, code, EnumOtpPurpose.PasswordReset, It.IsAny<CancellationToken>())
            )
            .ThrowsAsync(new NotFoundException("OTP not found."));

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_WhenOtpNotValid_ShouldNotResetPassword()
    {
        // Arrange
        string email = "admin@example.com";
        string code = "wrong-code";
        UserEntity user = UserFactory.CreateVerifiedActive();

        AdminResetPasswordCommand command = new(Email: email, Code: code, NewPassword: "NewPassword123!");
        AdminResetPasswordAuthData authData = new(User: user);

        _authFactoryMock
            .Setup(x => x.GetUserForResetAsync(email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(authData);
        _otpRepositoryMock
            .Setup(x =>
                x.ValidateUsedOtpAsync(user.Id, code, EnumOtpPurpose.PasswordReset, It.IsAny<CancellationToken>())
            )
            .ThrowsAsync(new NotFoundException("OTP not found."));

        // Act
        try
        {
            await _handler.Handle(command, CancellationToken.None);
        }
        catch (NotFoundException)
        {
            // Expected
        }

        // Assert
        _authFactoryMock.Verify(
            x => x.ResetPasswordAsync(It.IsAny<UserEntity>(), It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never
        );
    }

    #endregion

    #region Cancellation Token Tests

    [Fact]
    public async Task Handle_WithCancellationToken_ShouldPassToAuthFactory()
    {
        // Arrange
        string email = "admin@example.com";
        string code = "123456";
        string newPassword = "NewPassword123!";
        UserEntity user = UserFactory.CreateVerifiedActive();
        OtpEntity otp = OtpFactory.CreateUsed(user.Id);
        using CancellationTokenSource cts = new();

        AdminResetPasswordCommand command = new(Email: email, Code: code, NewPassword: newPassword);
        AdminResetPasswordAuthData authData = new(User: user);

        _authFactoryMock
            .Setup(x => x.GetUserForResetAsync(email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(authData);
        _otpRepositoryMock.SetupValidateUsedOtp(otp);
        _authFactoryMock
            .Setup(x => x.ResetPasswordAsync(user, newPassword, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        // Act
        await _handler.Handle(command, cts.Token);

        // Assert
        _authFactoryMock.Verify(x => x.GetUserForResetAsync(email, cts.Token), Times.Once);
    }

    [Fact]
    public async Task Handle_WithCancellationToken_ShouldPassToOtpRepository()
    {
        // Arrange
        string email = "admin@example.com";
        string code = "123456";
        string newPassword = "NewPassword123!";
        UserEntity user = UserFactory.CreateVerifiedActive();
        OtpEntity otp = OtpFactory.CreateUsed(user.Id);
        using CancellationTokenSource cts = new();

        AdminResetPasswordCommand command = new(Email: email, Code: code, NewPassword: newPassword);
        AdminResetPasswordAuthData authData = new(User: user);

        _authFactoryMock
            .Setup(x => x.GetUserForResetAsync(email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(authData);
        _otpRepositoryMock.SetupValidateUsedOtp(otp);
        _authFactoryMock
            .Setup(x => x.ResetPasswordAsync(user, newPassword, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        // Act
        await _handler.Handle(command, cts.Token);

        // Assert
        _otpRepositoryMock.Verify(
            x => x.ValidateUsedOtpAsync(user.Id, code, EnumOtpPurpose.PasswordReset, cts.Token),
            Times.Once
        );
    }

    [Fact]
    public async Task Handle_WithCancellationToken_ShouldPassToResetPassword()
    {
        // Arrange
        string email = "admin@example.com";
        string code = "123456";
        string newPassword = "NewPassword123!";
        UserEntity user = UserFactory.CreateVerifiedActive();
        OtpEntity otp = OtpFactory.CreateUsed(user.Id);
        using CancellationTokenSource cts = new();

        AdminResetPasswordCommand command = new(Email: email, Code: code, NewPassword: newPassword);
        AdminResetPasswordAuthData authData = new(User: user);

        _authFactoryMock
            .Setup(x => x.GetUserForResetAsync(email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(authData);
        _otpRepositoryMock.SetupValidateUsedOtp(otp);
        _authFactoryMock
            .Setup(x => x.ResetPasswordAsync(user, newPassword, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        // Act
        await _handler.Handle(command, cts.Token);

        // Assert
        _authFactoryMock.Verify(x => x.ResetPasswordAsync(user, newPassword, cts.Token), Times.Once);
    }

    #endregion
}
