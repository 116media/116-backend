using _116.Identity.Application.Auth.Services;
using _116.Identity.Application.Auth.UseCases.Admin.Commands.ForgotPassword;
using _116.Identity.Application.Auth.UseCases.Admin.Commands.ForgotPassword.Contracts;
using _116.Identity.Application.Shared.Repositories;
using _116.Identity.Domain.Entities;
using _116.Identity.Domain.ValueObjects;
using _116.Mailer.Contracts.Application;
using _116.Tests.Fixtures.Constants;
using _116.Tests.Fixtures.Factories.Identity;
using _116.Unit.Tests.Common.Mocks.Repositories;
using AwesomeAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace _116.Unit.Tests.Modules.Identity.Application.Auth.UseCases.Admin.Commands.ForgotPassword;

/// <summary>
/// Unit tests for <see cref="AdminForgotPasswordHandler"/>. Eligibility is decided by the
/// repository query, so an ineligible address reaches the handler as a null user; which kind of
/// ineligibility it was is covered by the repository's own integration tests.
/// </summary>
public class AdminForgotPasswordHandlerTests
{
    private readonly Mock<ILogger<AdminForgotPasswordHandler>> _loggerMock = new();
    private readonly Mock<IAdminForgotPasswordOtpFactory> _otpFactoryMock;
    private readonly Mock<IAuthRepository> _authRepositoryMock;
    private readonly AdminForgotPasswordHandler _handler;

    public AdminForgotPasswordHandlerTests()
    {
        _otpFactoryMock = new Mock<IAdminForgotPasswordOtpFactory>();
        _authRepositoryMock = MockAuthRepository.Create();

        _handler = new AdminForgotPasswordHandler(
            _otpFactoryMock.Object,
            _authRepositoryMock.Object,
            _loggerMock.Object
        );
    }

    #region Success Cases

    [Fact]
    public async Task Handle_ShouldMailThePlainCodeWhileTheOtpEntityKeepsOnlyItsHash()
    {
        // Arrange
        string email = "admin@example.com";
        UserEntity user = UserFactory.CreateAdmin();
        AdminForgotPasswordCommand command = new(Email: email);
        OtpEntity otp = OtpFactory.CreateForPasswordReset(user.Id);

        _authRepositoryMock.SetupGetActiveAdminByEmail(new Email(email), user);
        _otpFactoryMock
            .Setup(x => x.CreatePasswordResetOtpAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new OtpCreationResult(otp, TestConstants.Otp.DefaultCode));

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        otp.CodeHash.Should().NotBe(TestConstants.Otp.DefaultCode);
    }

    [Fact]
    public async Task Handle_WhenTheAddressBelongsToAnActiveAdmin_ShouldReturnSuccess()
    {
        // Arrange
        string email = "admin@example.com";
        UserEntity user = UserFactory.CreateAdmin();
        AdminForgotPasswordCommand command = new(Email: email);

        _authRepositoryMock.SetupGetActiveAdminByEmail(new Email(email), user);
        _otpFactoryMock
            .Setup(x => x.CreatePasswordResetOtpAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                new OtpCreationResult(OtpFactory.CreateForPasswordReset(user.Id), TestConstants.Otp.DefaultCode)
            );

        // Act
        AdminForgotPasswordResult result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Email.Should().Be(email);
    }

    [Fact]
    public async Task Handle_WhenTheAddressBelongsToAnActiveAdmin_ShouldCreatePasswordResetOtp()
    {
        // Arrange
        string email = "admin@example.com";
        UserEntity user = UserFactory.CreateAdmin();
        AdminForgotPasswordCommand command = new(Email: email);

        _authRepositoryMock.SetupGetActiveAdminByEmail(new Email(email), user);
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

    #endregion

    #region Ineligible Account Cases

    [Fact]
    public async Task Handle_WhenNoEligibleAdminOwnsTheAddress_ShouldReturnTheSameNeutralResult()
    {
        // Arrange
        string email = "nonexistent@example.com";
        AdminForgotPasswordCommand command = new(Email: email);

        _authRepositoryMock.SetupGetActiveAdminByEmail(new Email(email), user: null);

        // Act
        AdminForgotPasswordResult result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Email.Should().Be(email);
    }

    [Fact]
    public async Task Handle_WhenNoEligibleAdminOwnsTheAddress_ShouldNotCreateOtp()
    {
        // Arrange
        string email = "nonexistent@example.com";
        AdminForgotPasswordCommand command = new(Email: email);

        _authRepositoryMock.SetupGetActiveAdminByEmail(new Email(email), user: null);

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
        string email = "admin@example.com";
        AdminForgotPasswordCommand command = new(Email: email);
        using CancellationTokenSource cts = new();

        _authRepositoryMock.SetupGetActiveAdminByEmail(new Email(email), user: null);

        // Act
        await _handler.Handle(command, cts.Token);

        // Assert
        _authRepositoryMock.Verify(x => x.GetActiveAdminByEmailAsync(It.IsAny<Email>(), cts.Token), Times.Once);
    }

    [Fact]
    public async Task Handle_WithCancellationToken_ShouldPassToOtpFactory()
    {
        // Arrange
        string email = "admin@example.com";
        UserEntity user = UserFactory.CreateAdmin();
        AdminForgotPasswordCommand command = new(Email: email);
        using CancellationTokenSource cts = new();

        _authRepositoryMock.SetupGetActiveAdminByEmail(new Email(email), user);
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
