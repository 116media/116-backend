using _116.Identity.Application.Auth.Repositories;
using _116.Identity.Application.Auth.UseCases.Public.Commands.ResetPassword;
using _116.Identity.Application.Auth.UseCases.Public.Commands.ResetPassword.Contracts;
using _116.Identity.Domain.Entities;
using _116.Identity.Domain.Enums;
using _116.Shared.Application.Exceptions;
using _116.Unit.Tests.Common.Factories;
using _116.Unit.Tests.Common.Mocks.Repositories;
using AwesomeAssertions;
using Moq;
using Xunit;

namespace _116.Unit.Tests.Modules.Identity.Application.Auth.Public.ResetPassword;

/// <summary>
/// Unit tests for <see cref="PublicResetPasswordHandler"/>.
/// </summary>
public class PublicResetPasswordHandlerTests
{
    private readonly Mock<IPublicResetPasswordAuthFactory> _authFactoryMock;
    private readonly Mock<IOtpRepository> _otpRepositoryMock;
    private readonly PublicResetPasswordHandler _handler;

    public PublicResetPasswordHandlerTests()
    {
        _authFactoryMock = new Mock<IPublicResetPasswordAuthFactory>();
        _otpRepositoryMock = MockOtpRepository.Create();

        _handler = new PublicResetPasswordHandler(_authFactoryMock.Object, _otpRepositoryMock.Object);
    }

    #region Success Cases

    [Fact]
    public async Task Handle_WithValidOtp_ShouldReturnSuccess()
    {
        // Arrange
        string email = "user@example.com";
        string code = "123456";
        string newPassword = "NewPassword123!";
        UserEntity user = UserFactory.CreateVerifiedActive();
        OtpEntity otp = OtpFactory.CreateUsed(user.Id);

        PublicResetPasswordCommand command = new(Email: email, Code: code, NewPassword: newPassword);
        PublicResetPasswordAuthData authData = new(User: user);

        _authFactoryMock
            .Setup(x => x.GetUserForResetAsync(email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(authData);
        _otpRepositoryMock.SetupValidateUsedOtp(otp);
        _authFactoryMock
            .Setup(x => x.ResetPasswordAsync(user, newPassword, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        // Act
        PublicResetPasswordResult result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_ShouldGetUserForReset()
    {
        // Arrange
        string email = "user@example.com";
        string code = "123456";
        string newPassword = "NewPassword123!";
        UserEntity user = UserFactory.CreateVerifiedActive();
        OtpEntity otp = OtpFactory.CreateUsed(user.Id);

        PublicResetPasswordCommand command = new(Email: email, Code: code, NewPassword: newPassword);
        PublicResetPasswordAuthData authData = new(User: user);

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
        string email = "user@example.com";
        string code = "123456";
        string newPassword = "NewPassword123!";
        UserEntity user = UserFactory.CreateVerifiedActive();
        OtpEntity otp = OtpFactory.CreateUsed(user.Id);

        PublicResetPasswordCommand command = new(Email: email, Code: code, NewPassword: newPassword);
        PublicResetPasswordAuthData authData = new(User: user);

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
        string email = "user@example.com";
        string code = "123456";
        string newPassword = "NewPassword123!";
        UserEntity user = UserFactory.CreateVerifiedActive();
        OtpEntity otp = OtpFactory.CreateUsed(user.Id);

        PublicResetPasswordCommand command = new(Email: email, Code: code, NewPassword: newPassword);
        PublicResetPasswordAuthData authData = new(User: user);

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
        PublicResetPasswordCommand command = new(Email: email, Code: "123456", NewPassword: "NewPassword123!");

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
        string email = "user@example.com";
        string code = "wrong-code";
        UserEntity user = UserFactory.CreateVerifiedActive();

        PublicResetPasswordCommand command = new(Email: email, Code: code, NewPassword: "NewPassword123!");
        PublicResetPasswordAuthData authData = new(User: user);

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
        string email = "user@example.com";
        string code = "wrong-code";
        UserEntity user = UserFactory.CreateVerifiedActive();

        PublicResetPasswordCommand command = new(Email: email, Code: code, NewPassword: "NewPassword123!");
        PublicResetPasswordAuthData authData = new(User: user);

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
        string email = "user@example.com";
        string code = "123456";
        string newPassword = "NewPassword123!";
        UserEntity user = UserFactory.CreateVerifiedActive();
        OtpEntity otp = OtpFactory.CreateUsed(user.Id);
        using CancellationTokenSource cts = new();

        PublicResetPasswordCommand command = new(Email: email, Code: code, NewPassword: newPassword);
        PublicResetPasswordAuthData authData = new(User: user);

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
        string email = "user@example.com";
        string code = "123456";
        string newPassword = "NewPassword123!";
        UserEntity user = UserFactory.CreateVerifiedActive();
        OtpEntity otp = OtpFactory.CreateUsed(user.Id);
        using CancellationTokenSource cts = new();

        PublicResetPasswordCommand command = new(Email: email, Code: code, NewPassword: newPassword);
        PublicResetPasswordAuthData authData = new(User: user);

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

    #endregion
}
