using _116.Identity.Application.Auth.UseCases.Public.Commands.ForgotPassword;
using _116.Identity.Application.Auth.UseCases.Public.Commands.ForgotPassword.Contracts;
using _116.Identity.Application.Shared.Repositories;
using _116.Identity.Domain.Entities;
using _116.Identity.Domain.ValueObjects;
using _116.Unit.Tests.Common.Factories;
using _116.Unit.Tests.Common.Mocks.Repositories;
using AwesomeAssertions;
using Moq;
using Xunit;

namespace _116.Unit.Tests.Modules.Identity.Application.Auth.Public.ForgotPassword;

/// <summary>
/// Unit tests for <see cref="PublicForgotPasswordHandler"/>.
/// </summary>
public class PublicForgotPasswordHandlerTests
{
    private readonly Mock<IPublicForgotPasswordOtpFactory> _otpFactoryMock;
    private readonly Mock<IAuthRepository> _authRepositoryMock;
    private readonly PublicForgotPasswordHandler _handler;

    public PublicForgotPasswordHandlerTests()
    {
        _otpFactoryMock = new Mock<IPublicForgotPasswordOtpFactory>();
        _authRepositoryMock = MockAuthRepository.Create();

        _handler = new PublicForgotPasswordHandler(_otpFactoryMock.Object, _authRepositoryMock.Object);
    }

    #region Success Cases

    [Fact]
    public async Task Handle_WhenUserExists_ShouldReturnSuccess()
    {
        // Arrange
        string email = "user@example.com";
        UserEntity user = UserFactory.CreateVerifiedActive();
        PublicForgotPasswordCommand command = new(Email: email);

        _authRepositoryMock.SetupExistsByEmail(new Email(email), true);
        _authRepositoryMock.SetupGetUserWithRolesByEmailOrThrow(new Email(email), user);
        _authRepositoryMock.SetupIsUserAccountActiveReturnsTrue();
        _authRepositoryMock.SetupIsUserAccountVerifiedReturnsTrue();
        _otpFactoryMock
            .Setup(x => x.CreatePasswordResetOtpAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(OtpFactory.CreateForPasswordReset(user.Id));

        // Act
        PublicForgotPasswordResult result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Email.Should().Be(email);
    }

    [Fact]
    public async Task Handle_WhenUserExists_ShouldCreatePasswordResetOtp()
    {
        // Arrange
        string email = "user@example.com";
        UserEntity user = UserFactory.CreateVerifiedActive();
        PublicForgotPasswordCommand command = new(Email: email);

        _authRepositoryMock.SetupExistsByEmail(new Email(email), true);
        _authRepositoryMock.SetupGetUserWithRolesByEmailOrThrow(new Email(email), user);
        _authRepositoryMock.SetupIsUserAccountActiveReturnsTrue();
        _authRepositoryMock.SetupIsUserAccountVerifiedReturnsTrue();
        _otpFactoryMock
            .Setup(x => x.CreatePasswordResetOtpAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(OtpFactory.CreateForPasswordReset(user.Id));

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _otpFactoryMock.Verify(x => x.CreatePasswordResetOtpAsync(user.Id, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenUserDoesNotExist_ShouldStillReturnSuccess()
    {
        // Arrange - Security: prevent user enumeration
        string email = "nonexistent@example.com";
        PublicForgotPasswordCommand command = new(Email: email);

        _authRepositoryMock.SetupExistsByEmail(new Email(email), false);

        // Act
        PublicForgotPasswordResult result = await _handler.Handle(command, CancellationToken.None);

        // Assert - Returns success to prevent user enumeration
        result.IsSuccess.Should().BeTrue();
        result.Email.Should().Be(email);
    }

    [Fact]
    public async Task Handle_WhenUserDoesNotExist_ShouldNotCreateOtp()
    {
        // Arrange
        string email = "nonexistent@example.com";
        PublicForgotPasswordCommand command = new(Email: email);

        _authRepositoryMock.SetupExistsByEmail(new Email(email), false);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _otpFactoryMock.Verify(
            x => x.CreatePasswordResetOtpAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
            Times.Never
        );
    }

    [Fact]
    public async Task Handle_ShouldCheckIfEmailExists()
    {
        // Arrange
        string email = "user@example.com";
        PublicForgotPasswordCommand command = new(Email: email);

        _authRepositoryMock.SetupExistsByEmail(new Email(email), false);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _authRepositoryMock.Verify(
            x => x.ExistsByEmailAsync(It.IsAny<Email>(), It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    [Fact]
    public async Task Handle_ShouldValidateUserAccountIsActive()
    {
        // Arrange
        string email = "user@example.com";
        UserEntity user = UserFactory.CreateVerifiedActive();
        PublicForgotPasswordCommand command = new(Email: email);

        _authRepositoryMock.SetupExistsByEmail(new Email(email), true);
        _authRepositoryMock.SetupGetUserWithRolesByEmailOrThrow(new Email(email), user);
        _authRepositoryMock.SetupIsUserAccountActiveReturnsTrue();
        _authRepositoryMock.SetupIsUserAccountVerifiedReturnsTrue();
        _otpFactoryMock
            .Setup(x => x.CreatePasswordResetOtpAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(OtpFactory.CreateForPasswordReset(user.Id));

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _authRepositoryMock.Verify(x => x.IsUserAccountActive(It.IsAny<UserEntity>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldValidateUserAccountIsVerified()
    {
        // Arrange
        string email = "user@example.com";
        UserEntity user = UserFactory.CreateVerifiedActive();
        PublicForgotPasswordCommand command = new(Email: email);

        _authRepositoryMock.SetupExistsByEmail(new Email(email), true);
        _authRepositoryMock.SetupGetUserWithRolesByEmailOrThrow(new Email(email), user);
        _authRepositoryMock.SetupIsUserAccountActiveReturnsTrue();
        _authRepositoryMock.SetupIsUserAccountVerifiedReturnsTrue();
        _otpFactoryMock
            .Setup(x => x.CreatePasswordResetOtpAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(OtpFactory.CreateForPasswordReset(user.Id));

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _authRepositoryMock.Verify(x => x.IsUserAccountVerified(It.IsAny<UserEntity>()), Times.Once);
    }

    #endregion

    #region Cancellation Token Tests

    [Fact]
    public async Task Handle_WithCancellationToken_ShouldPassToAuthRepository()
    {
        // Arrange
        string email = "user@example.com";
        PublicForgotPasswordCommand command = new(Email: email);
        using CancellationTokenSource cts = new();

        _authRepositoryMock.SetupExistsByEmail(new Email(email), false);

        // Act
        await _handler.Handle(command, cts.Token);

        // Assert
        _authRepositoryMock.Verify(x => x.ExistsByEmailAsync(It.IsAny<Email>(), cts.Token), Times.Once);
    }

    [Fact]
    public async Task Handle_WithCancellationToken_ShouldPassToOtpFactory()
    {
        // Arrange
        string email = "user@example.com";
        UserEntity user = UserFactory.CreateVerifiedActive();
        PublicForgotPasswordCommand command = new(Email: email);
        using CancellationTokenSource cts = new();

        _authRepositoryMock.SetupExistsByEmail(new Email(email), true);
        _authRepositoryMock.SetupGetUserWithRolesByEmailOrThrow(new Email(email), user);
        _authRepositoryMock.SetupIsUserAccountActiveReturnsTrue();
        _authRepositoryMock.SetupIsUserAccountVerifiedReturnsTrue();
        _otpFactoryMock
            .Setup(x => x.CreatePasswordResetOtpAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(OtpFactory.CreateForPasswordReset(user.Id));

        // Act
        await _handler.Handle(command, cts.Token);

        // Assert
        _otpFactoryMock.Verify(x => x.CreatePasswordResetOtpAsync(user.Id, cts.Token), Times.Once);
    }

    #endregion
}
