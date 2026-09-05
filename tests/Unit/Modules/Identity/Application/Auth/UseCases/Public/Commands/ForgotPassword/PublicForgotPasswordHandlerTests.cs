using _116.Identity.Application.Auth.Services;
using _116.Identity.Application.Auth.UseCases.Public.Commands.ForgotPassword;
using _116.Identity.Application.Auth.UseCases.Public.Commands.ForgotPassword.Contracts;
using _116.Identity.Application.Shared.Repositories;
using _116.Identity.Domain.Entities;
using _116.Identity.Domain.ValueObjects;
using _116.Mailer.Contracts.Application;
using _116.Mailer.Contracts.Domain;
using _116.Tests.Fixtures.Constants;
using _116.Tests.Fixtures.Factories.Identity;
using _116.Unit.Tests.Common.Mocks.Repositories;
using AwesomeAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace _116.Unit.Tests.Modules.Identity.Application.Auth.UseCases.Public.Commands.ForgotPassword;

/// <summary>
/// Unit tests for <see cref="PublicForgotPasswordHandler"/>.
/// </summary>
public class PublicForgotPasswordHandlerTests
{
    private readonly Mock<IMailer> _mailerMock = new();
    private readonly Mock<ILogger<PublicForgotPasswordHandler>> _loggerMock = new();
    private readonly Mock<IPublicForgotPasswordOtpFactory> _otpFactoryMock;
    private readonly Mock<IAuthRepository> _authRepositoryMock;
    private readonly PublicForgotPasswordHandler _handler;

    public PublicForgotPasswordHandlerTests()
    {
        _otpFactoryMock = new Mock<IPublicForgotPasswordOtpFactory>();
        _authRepositoryMock = MockAuthRepository.Create();

        _handler = new PublicForgotPasswordHandler(
            _otpFactoryMock.Object,
            _authRepositoryMock.Object,
            _mailerMock.Object,
            _loggerMock.Object
        );
    }

    #region Success Cases

    [Fact]
    public async Task Handle_ShouldMailThePlainCodeWhileTheOtpEntityKeepsOnlyItsHash()
    {
        // Arrange
        string email = "user@example.com";
        UserEntity user = UserFactory.CreateVerifiedActive();
        PublicForgotPasswordCommand command = new(Email: email);
        OtpEntity otp = OtpFactory.CreateForPasswordReset(user.Id);

        _authRepositoryMock.SetupExistsByEmail(new Email(email), true);
        _authRepositoryMock.SetupGetUserWithRolesByEmailOrThrow(new Email(email), user);
        _otpFactoryMock
            .Setup(x => x.CreatePasswordResetOtpAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new OtpCreationResult(otp, TestConstants.Otp.DefaultCode));

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _mailerMock.Verify(
            x =>
                x.EnqueueAsync(
                    It.IsAny<EnumEmailTemplate>(),
                    It.IsAny<EmailRecipient>(),
                    It.Is<IReadOnlyDictionary<string, string>>(t =>
                        t["otpCode"] == TestConstants.Otp.DefaultCode && t["otpCode"] != otp.CodeHash
                    ),
                    It.IsAny<string>(),
                    It.IsAny<CancellationToken>()
                ),
            Times.Once
        );
        otp.CodeHash.Should().NotBe(TestConstants.Otp.DefaultCode);
    }

    [Fact]
    public async Task Handle_WhenUserExists_ShouldReturnSuccess()
    {
        // Arrange
        string email = "user@example.com";
        UserEntity user = UserFactory.CreateVerifiedActive();
        PublicForgotPasswordCommand command = new(Email: email);

        _authRepositoryMock.SetupExistsByEmail(new Email(email), true);
        _authRepositoryMock.SetupGetUserWithRolesByEmailOrThrow(new Email(email), user);
        _otpFactoryMock
            .Setup(x => x.CreatePasswordResetOtpAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                new OtpCreationResult(OtpFactory.CreateForPasswordReset(user.Id), TestConstants.Otp.DefaultCode)
            );

        // Act
        PublicForgotPasswordResult result = await _handler.Handle(command, CancellationToken.None);

        // Assert
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
        _otpFactoryMock
            .Setup(x => x.CreatePasswordResetOtpAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                new OtpCreationResult(OtpFactory.CreateForPasswordReset(user.Id), TestConstants.Otp.DefaultCode)
            );

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _otpFactoryMock.Verify(x => x.CreatePasswordResetOtpAsync(user.Id, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenUserDoesNotExist_ShouldStillReturnSuccess()
    {
        // Arrange
        string email = "nonexistent@example.com";
        PublicForgotPasswordCommand command = new(Email: email);

        _authRepositoryMock.SetupExistsByEmail(new Email(email), false);

        // Act
        PublicForgotPasswordResult result = await _handler.Handle(command, CancellationToken.None);

        // Assert
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

    #endregion

    #region Ineligible Account Cases

    [Fact]
    public async Task Handle_WhenAccountIsInactive_ShouldReturnTheSameNeutralResult()
    {
        // Arrange
        string email = "inactive@example.com";
        UserEntity user = UserFactory.CreateInactive();
        PublicForgotPasswordCommand command = new(Email: email);

        _authRepositoryMock.SetupExistsByEmail(new Email(email), true);
        _authRepositoryMock.SetupGetUserWithRolesByEmailOrThrow(new Email(email), user);

        // Act
        PublicForgotPasswordResult result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Email.Should().Be(email);
    }

    [Fact]
    public async Task Handle_WhenAccountIsInactive_ShouldNotCreateOtp()
    {
        // Arrange
        string email = "inactive@example.com";
        UserEntity user = UserFactory.CreateInactive();
        PublicForgotPasswordCommand command = new(Email: email);

        _authRepositoryMock.SetupExistsByEmail(new Email(email), true);
        _authRepositoryMock.SetupGetUserWithRolesByEmailOrThrow(new Email(email), user);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _otpFactoryMock.Verify(
            x => x.CreatePasswordResetOtpAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
            Times.Never
        );
    }

    [Fact]
    public async Task Handle_WhenAccountIsInactive_ShouldNotEnqueueMail()
    {
        // Arrange
        string email = "inactive@example.com";
        UserEntity user = UserFactory.CreateInactive();
        PublicForgotPasswordCommand command = new(Email: email);

        _authRepositoryMock.SetupExistsByEmail(new Email(email), true);
        _authRepositoryMock.SetupGetUserWithRolesByEmailOrThrow(new Email(email), user);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _mailerMock.Verify(
            x =>
                x.EnqueueAsync(
                    It.IsAny<EnumEmailTemplate>(),
                    It.IsAny<EmailRecipient>(),
                    It.IsAny<IReadOnlyDictionary<string, string>>(),
                    It.IsAny<string>(),
                    It.IsAny<CancellationToken>()
                ),
            Times.Never
        );
    }

    [Fact]
    public async Task Handle_WhenAccountIsUnverified_ShouldReturnTheSameNeutralResult()
    {
        // Arrange
        string email = "unverified@example.com";
        UserEntity user = UserFactory.CreateUnverified();
        PublicForgotPasswordCommand command = new(Email: email);

        _authRepositoryMock.SetupExistsByEmail(new Email(email), true);
        _authRepositoryMock.SetupGetUserWithRolesByEmailOrThrow(new Email(email), user);

        // Act
        PublicForgotPasswordResult result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Email.Should().Be(email);
    }

    [Fact]
    public async Task Handle_WhenAccountIsUnverified_ShouldNotCreateOtp()
    {
        // Arrange
        string email = "unverified@example.com";
        UserEntity user = UserFactory.CreateUnverified();
        PublicForgotPasswordCommand command = new(Email: email);

        _authRepositoryMock.SetupExistsByEmail(new Email(email), true);
        _authRepositoryMock.SetupGetUserWithRolesByEmailOrThrow(new Email(email), user);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _otpFactoryMock.Verify(
            x => x.CreatePasswordResetOtpAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
            Times.Never
        );
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
        _otpFactoryMock
            .Setup(x => x.CreatePasswordResetOtpAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                new OtpCreationResult(OtpFactory.CreateForPasswordReset(user.Id), TestConstants.Otp.DefaultCode)
            );

        // Act
        await _handler.Handle(command, cts.Token);

        // Assert
        _otpFactoryMock.Verify(x => x.CreatePasswordResetOtpAsync(user.Id, cts.Token), Times.Once);
    }

    #endregion
}
