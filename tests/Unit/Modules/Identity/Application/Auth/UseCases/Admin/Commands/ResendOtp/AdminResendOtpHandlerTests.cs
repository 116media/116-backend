using _116.Identity.Application.Auth.Services;
using _116.Identity.Application.Auth.UseCases.Admin.Commands.ResendOtp;
using _116.Identity.Application.Auth.UseCases.Admin.Commands.ResendOtp.Contracts;
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

namespace _116.Unit.Tests.Modules.Identity.Application.Auth.UseCases.Admin.Commands.ResendOtp;

/// <summary>
/// Unit tests for <see cref="AdminResendOtpHandler"/>.
/// </summary>
public class AdminResendOtpHandlerTests
{
    private readonly Mock<IMailer> _mailerMock = new();
    private readonly Mock<IAdminResendOtpFactory> _otpFactoryMock;
    private readonly Mock<IAuthRepository> _authRepositoryMock;
    private readonly AdminResendOtpHandler _handler;

    public AdminResendOtpHandlerTests()
    {
        _otpFactoryMock = new Mock<IAdminResendOtpFactory>();
        _authRepositoryMock = MockAuthRepository.Create();

        _handler = new AdminResendOtpHandler(_otpFactoryMock.Object, _authRepositoryMock.Object, _mailerMock.Object);
    }

    #region Success Cases

    [Fact]
    public async Task Handle_ShouldMailThePlainCodeWhileTheOtpEntityKeepsOnlyItsHash()
    {
        // Arrange
        string email = "admin@example.com";
        string purpose = EnumOtpPurpose.EmailVerification.ToString();
        UserEntity user = UserFactory.CreateVerifiedActive();
        OtpEntity otp = OtpFactory.Create(user.Id);

        AdminResendOtpCommand command = new(Email: email, Purpose: purpose);

        _authRepositoryMock.SetupExistsByEmail(new Email(email), true);
        _authRepositoryMock.SetupGetUserWithRolesByEmailOrThrow(new Email(email), user);
        _authRepositoryMock.SetupIsUserAdminReturnsTrue();
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
        string email = "admin@example.com";
        string purpose = EnumOtpPurpose.EmailVerification.ToString();
        UserEntity user = UserFactory.CreateVerifiedActive();
        OtpEntity otp = OtpFactory.Create(user.Id);

        AdminResendOtpCommand command = new(Email: email, Purpose: purpose);

        _authRepositoryMock.SetupExistsByEmail(new Email(email), true);
        _authRepositoryMock.SetupGetUserWithRolesByEmailOrThrow(new Email(email), user);
        _authRepositoryMock.SetupIsUserAdminReturnsTrue();
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
    public async Task Handle_WhenTheFactoryMintsNothing_ShouldSucceedWithoutMailing()
    {
        // Arrange
        string email = "admin@example.com";
        string purpose = EnumOtpPurpose.EmailVerification.ToString();
        UserEntity user = UserFactory.CreateVerifiedActive();

        AdminResendOtpCommand command = new(Email: email, Purpose: purpose);

        _authRepositoryMock.SetupExistsByEmail(new Email(email), true);
        _authRepositoryMock.SetupGetUserWithRolesByEmailOrThrow(new Email(email), user);
        _authRepositoryMock.SetupIsUserAdminReturnsTrue();
        _authRepositoryMock.SetupIsUserAccountActiveReturnsTrue();
        _otpFactoryMock
            .Setup(x => x.ResendOtpAsync(user.Id, It.IsAny<OtpPurpose>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((OtpCreationResult?)null);

        // Act
        AdminResendOtpResult result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
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
    public async Task Handle_WhenUserDoesNotExist_ShouldNotCallOtpFactory()
    {
        // Arrange
        string email = "nonexistent@example.com";
        string purpose = EnumOtpPurpose.EmailVerification.ToString();
        AdminResendOtpCommand command = new(Email: email, Purpose: purpose);

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
        string email = "admin@example.com";
        string purpose = EnumOtpPurpose.EmailVerification.ToString();
        AdminResendOtpCommand command = new(Email: email, Purpose: purpose);

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
    public async Task Handle_ShouldValidateUserIsAdmin()
    {
        // Arrange
        string email = "admin@example.com";
        string purpose = EnumOtpPurpose.EmailVerification.ToString();
        UserEntity user = UserFactory.CreateVerifiedActive();
        OtpEntity otp = OtpFactory.Create(user.Id);

        AdminResendOtpCommand command = new(Email: email, Purpose: purpose);

        _authRepositoryMock.SetupExistsByEmail(new Email(email), true);
        _authRepositoryMock.SetupGetUserWithRolesByEmailOrThrow(new Email(email), user);
        _authRepositoryMock.SetupIsUserAdminReturnsTrue();
        _authRepositoryMock.SetupIsUserAccountActiveReturnsTrue();
        _otpFactoryMock
            .Setup(x => x.ResendOtpAsync(user.Id, It.IsAny<OtpPurpose>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new OtpCreationResult(otp, TestConstants.Otp.DefaultCode));

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _authRepositoryMock.Verify(x => x.IsUserAdmin(It.IsAny<UserEntity>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldValidateUserAccountIsActive()
    {
        // Arrange
        string email = "admin@example.com";
        string purpose = EnumOtpPurpose.EmailVerification.ToString();
        UserEntity user = UserFactory.CreateVerifiedActive();
        OtpEntity otp = OtpFactory.Create(user.Id);

        AdminResendOtpCommand command = new(Email: email, Purpose: purpose);

        _authRepositoryMock.SetupExistsByEmail(new Email(email), true);
        _authRepositoryMock.SetupGetUserWithRolesByEmailOrThrow(new Email(email), user);
        _authRepositoryMock.SetupIsUserAdminReturnsTrue();
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
        string email = "admin@example.com";
        string purpose = EnumOtpPurpose.EmailVerification.ToString();
        AdminResendOtpCommand command = new(Email: email, Purpose: purpose);
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
        string purpose = EnumOtpPurpose.EmailVerification.ToString();
        UserEntity user = UserFactory.CreateVerifiedActive();
        OtpEntity otp = OtpFactory.Create(user.Id);
        using CancellationTokenSource cts = new();

        AdminResendOtpCommand command = new(Email: email, Purpose: purpose);

        _authRepositoryMock.SetupExistsByEmail(new Email(email), true);
        _authRepositoryMock.SetupGetUserWithRolesByEmailOrThrow(new Email(email), user);
        _authRepositoryMock.SetupIsUserAdminReturnsTrue();
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
