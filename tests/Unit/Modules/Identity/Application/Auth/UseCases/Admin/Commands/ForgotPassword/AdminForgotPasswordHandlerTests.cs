using _116.Identity.Application.Auth.UseCases.Admin.Commands.ForgotPassword;
using _116.Identity.Application.Auth.UseCases.Admin.Commands.ForgotPassword.Contracts;
using _116.Identity.Application.Shared.Repositories;
using _116.Identity.Domain.Entities;
using _116.Identity.Domain.ValueObjects;
using _116.Tests.Fixtures.Factories;
using _116.Unit.Tests.Common.Mocks.Repositories;
using AwesomeAssertions;
using Moq;
using Xunit;

namespace _116.Unit.Tests.Modules.Identity.Application.Auth.UseCases.Admin.Commands.ForgotPassword;

/// <summary>
/// Unit tests for <see cref="AdminForgotPasswordHandler"/>.
/// </summary>
public class AdminForgotPasswordHandlerTests
{
    private readonly Mock<IAdminForgotPasswordOtpFactory> _otpFactoryMock;
    private readonly Mock<IAuthRepository> _authRepositoryMock;
    private readonly AdminForgotPasswordHandler _handler;

    public AdminForgotPasswordHandlerTests()
    {
        _otpFactoryMock = new Mock<IAdminForgotPasswordOtpFactory>();
        _authRepositoryMock = MockAuthRepository.Create();

        _handler = new AdminForgotPasswordHandler(_otpFactoryMock.Object, _authRepositoryMock.Object);
    }

    #region Success Cases

    [Fact]
    public async Task Handle_WhenUserExists_ShouldReturnSuccess()
    {
        // Arrange
        string email = "admin@example.com";
        UserEntity user = UserFactory.CreateVerifiedActive();
        AdminForgotPasswordCommand command = new(Email: email);

        _authRepositoryMock.SetupExistsByEmail(new Email(email), true);
        _authRepositoryMock.SetupGetUserWithRolesByEmailOrThrow(new Email(email), user);
        _authRepositoryMock.SetupIsUserAdminReturnsTrue();
        _authRepositoryMock.SetupIsUserAccountActiveReturnsTrue();
        _otpFactoryMock
            .Setup(x => x.CreatePasswordResetOtpAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(OtpFactory.CreateForPasswordReset(user.Id));

        // Act
        AdminForgotPasswordResult result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Email.Should().Be(email);
    }

    [Fact]
    public async Task Handle_WhenUserExists_ShouldCreatePasswordResetOtp()
    {
        // Arrange
        string email = "admin@example.com";
        UserEntity user = UserFactory.CreateVerifiedActive();
        AdminForgotPasswordCommand command = new(Email: email);

        _authRepositoryMock.SetupExistsByEmail(new Email(email), true);
        _authRepositoryMock.SetupGetUserWithRolesByEmailOrThrow(new Email(email), user);
        _authRepositoryMock.SetupIsUserAdminReturnsTrue();
        _authRepositoryMock.SetupIsUserAccountActiveReturnsTrue();
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
        AdminForgotPasswordCommand command = new(Email: email);

        _authRepositoryMock.SetupExistsByEmail(new Email(email), false);

        // Act
        AdminForgotPasswordResult result = await _handler.Handle(command, CancellationToken.None);

        // Assert - Returns success to prevent user enumeration
        result.IsSuccess.Should().BeTrue();
        result.Email.Should().Be(email);
    }

    [Fact]
    public async Task Handle_WhenUserDoesNotExist_ShouldNotCreateOtp()
    {
        // Arrange
        string email = "nonexistent@example.com";
        AdminForgotPasswordCommand command = new(Email: email);

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
        string email = "admin@example.com";
        AdminForgotPasswordCommand command = new(Email: email);

        _authRepositoryMock.SetupExistsByEmail(new Email(email), false);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _authRepositoryMock.Verify(
            x => x.ExistsByEmailAsync(It.IsAny<Email>(), It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    #endregion

    #region Cancellation Token Tests

    [Fact]
    public async Task Handle_WithCancellationToken_ShouldPassToAuthRepository()
    {
        // Arrange
        string email = "admin@example.com";
        AdminForgotPasswordCommand command = new(Email: email);
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
        string email = "admin@example.com";
        UserEntity user = UserFactory.CreateVerifiedActive();
        AdminForgotPasswordCommand command = new(Email: email);
        using CancellationTokenSource cts = new();

        _authRepositoryMock.SetupExistsByEmail(new Email(email), true);
        _authRepositoryMock.SetupGetUserWithRolesByEmailOrThrow(new Email(email), user);
        _authRepositoryMock.SetupIsUserAdminReturnsTrue();
        _authRepositoryMock.SetupIsUserAccountActiveReturnsTrue();
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
