using _116.Identity.Application.Auth.Services;
using _116.Identity.Application.Auth.UseCases.Public.Commands.ResendOtp;
using _116.Identity.Application.Auth.UseCases.Public.Commands.ResendOtp.Contracts;
using _116.Identity.Application.Shared.Repositories;
using _116.Identity.Domain.Entities;
using _116.Identity.Domain.Enums;
using _116.Identity.Domain.ValueObjects;
using _116.Mailer.Contracts.Application;
using _116.Tests.Fixtures.Constants;
using _116.Tests.Fixtures.Factories.Identity;
using _116.Unit.Tests.Common.Mocks.Repositories;
using AwesomeAssertions;
using Moq;
using Xunit;

namespace _116.Unit.Tests.Modules.Identity.Application.Auth.UseCases.Public.Commands.ResendOtp;

/// <summary>
/// Unit tests for <see cref="PublicResendOtpHandler"/>.
/// </summary>
public class PublicResendOtpHandlerTests
{
    private readonly Mock<IMailer> _mailerMock = new();
    private readonly Mock<IPublicResendOtpFactory> _otpFactoryMock;
    private readonly Mock<IAuthRepository> _authRepositoryMock;
    private readonly PublicResendOtpHandler _handler;

    public PublicResendOtpHandlerTests()
    {
        _otpFactoryMock = new Mock<IPublicResendOtpFactory>();
        _authRepositoryMock = MockAuthRepository.Create();

        _handler = new PublicResendOtpHandler(_otpFactoryMock.Object, _authRepositoryMock.Object, _mailerMock.Object);
    }

    #region Success Cases

    [Fact]
    public async Task Handle_ShouldMailThePlainCodeWhileTheOtpEntityKeepsOnlyItsHash()
    {
        // Arrange
        string email = "user@example.com";
        string purpose = EnumOtpPurpose.EmailVerification.ToString();
        UserEntity user = UserFactory.CreateVerifiedActive();
        OtpEntity otp = OtpFactory.Create(user.Id);

        PublicResendOtpCommand command = new(Email: email, Purpose: purpose);

        _authRepositoryMock.SetupExistsByEmail(new Email(email), true);
        _authRepositoryMock.SetupGetUserWithRolesByEmailOrThrow(new Email(email), user);
        _authRepositoryMock.SetupIsUserAccountActiveReturnsTrue();
        _otpFactoryMock
            .Setup(x => x.ResendOtpAsync(user.Id, It.IsAny<OtpPurpose>(), It.IsAny<CancellationToken>()))
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
    public async Task Handle_WhenUserExists_ShouldCallOtpFactory()
    {
        // Arrange
        string email = "user@example.com";
        string purpose = EnumOtpPurpose.EmailVerification.ToString();
        UserEntity user = UserFactory.CreateVerifiedActive();
        OtpEntity otp = OtpFactory.Create(user.Id);

        PublicResendOtpCommand command = new(Email: email, Purpose: purpose);

        _authRepositoryMock.SetupExistsByEmail(new Email(email), true);
        _authRepositoryMock.SetupGetUserWithRolesByEmailOrThrow(new Email(email), user);
        _authRepositoryMock.SetupIsUserAccountActiveReturnsTrue();
        _otpFactoryMock
            .Setup(x => x.ResendOtpAsync(user.Id, It.IsAny<OtpPurpose>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new OtpCreationResult(otp, TestConstants.Otp.DefaultCode));

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _otpFactoryMock.Verify(
            x => x.ResendOtpAsync(user.Id, It.IsAny<OtpPurpose>(), It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    [Fact]
    public async Task Handle_WhenUserDoesNotExist_ShouldNotCallOtpFactory()
    {
        // Arrange
        string email = "nonexistent@example.com";
        string purpose = EnumOtpPurpose.EmailVerification.ToString();
        PublicResendOtpCommand command = new(Email: email, Purpose: purpose);

        _authRepositoryMock.SetupExistsByEmail(new Email(email), false);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _otpFactoryMock.Verify(
            x => x.ResendOtpAsync(It.IsAny<Guid>(), It.IsAny<OtpPurpose>(), It.IsAny<CancellationToken>()),
            Times.Never
        );
    }

    [Fact]
    public async Task Handle_ShouldCheckIfEmailExists()
    {
        // Arrange
        string email = "user@example.com";
        string purpose = EnumOtpPurpose.EmailVerification.ToString();
        PublicResendOtpCommand command = new(Email: email, Purpose: purpose);

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
        string purpose = EnumOtpPurpose.EmailVerification.ToString();
        UserEntity user = UserFactory.CreateVerifiedActive();
        OtpEntity otp = OtpFactory.Create(user.Id);

        PublicResendOtpCommand command = new(Email: email, Purpose: purpose);

        _authRepositoryMock.SetupExistsByEmail(new Email(email), true);
        _authRepositoryMock.SetupGetUserWithRolesByEmailOrThrow(new Email(email), user);
        _authRepositoryMock.SetupIsUserAccountActiveReturnsTrue();
        _otpFactoryMock
            .Setup(x => x.ResendOtpAsync(user.Id, It.IsAny<OtpPurpose>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new OtpCreationResult(otp, TestConstants.Otp.DefaultCode));

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _authRepositoryMock.Verify(x => x.IsUserAccountActive(It.IsAny<UserEntity>()), Times.Once);
    }

    #endregion

    #region Cancellation Token Tests

    [Fact]
    public async Task Handle_WithCancellationToken_ShouldPassToAuthRepository()
    {
        // Arrange
        string email = "user@example.com";
        string purpose = EnumOtpPurpose.EmailVerification.ToString();
        PublicResendOtpCommand command = new(Email: email, Purpose: purpose);
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
        string purpose = EnumOtpPurpose.EmailVerification.ToString();
        UserEntity user = UserFactory.CreateVerifiedActive();
        OtpEntity otp = OtpFactory.Create(user.Id);
        using CancellationTokenSource cts = new();

        PublicResendOtpCommand command = new(Email: email, Purpose: purpose);

        _authRepositoryMock.SetupExistsByEmail(new Email(email), true);
        _authRepositoryMock.SetupGetUserWithRolesByEmailOrThrow(new Email(email), user);
        _authRepositoryMock.SetupIsUserAccountActiveReturnsTrue();
        _otpFactoryMock
            .Setup(x => x.ResendOtpAsync(user.Id, It.IsAny<OtpPurpose>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new OtpCreationResult(otp, TestConstants.Otp.DefaultCode));

        // Act
        await _handler.Handle(command, cts.Token);

        // Assert
        _otpFactoryMock.Verify(x => x.ResendOtpAsync(user.Id, It.IsAny<OtpPurpose>(), cts.Token), Times.Once);
    }

    #endregion
}
