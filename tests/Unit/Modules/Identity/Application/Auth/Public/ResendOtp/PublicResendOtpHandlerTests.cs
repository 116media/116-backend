using _116.Identity.Application.Auth.UseCases.Public.Commands.ResendOtp;
using _116.Identity.Application.Auth.UseCases.Public.Commands.ResendOtp.Contracts;
using _116.Identity.Application.Shared.Repositories;
using _116.Identity.Domain.Entities;
using _116.Identity.Domain.Enums;
using _116.Identity.Domain.ValueObjects;
using _116.Unit.Tests.Common.Factories;
using _116.Unit.Tests.Common.Mocks.Repositories;
using AwesomeAssertions;
using Moq;
using Xunit;

namespace _116.Unit.Tests.Modules.Identity.Application.Auth.Public.ResendOtp;

/// <summary>
/// Unit tests for <see cref="PublicResendOtpHandler"/>.
/// </summary>
public class PublicResendOtpHandlerTests
{
    private readonly Mock<IPublicResendOtpFactory> _otpFactoryMock;
    private readonly Mock<IAuthRepository> _authRepositoryMock;
    private readonly PublicResendOtpHandler _handler;

    public PublicResendOtpHandlerTests()
    {
        _otpFactoryMock = new Mock<IPublicResendOtpFactory>();
        _authRepositoryMock = MockAuthRepository.Create();

        _handler = new PublicResendOtpHandler(_otpFactoryMock.Object, _authRepositoryMock.Object);
    }

    #region Success Cases

    [Fact]
    public async Task Handle_WhenUserExists_ShouldReturnSuccess()
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
            .ReturnsAsync(otp);

        // Act
        PublicResendOtpResult result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
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
            .ReturnsAsync(otp);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _otpFactoryMock.Verify(
            x => x.ResendOtpAsync(user.Id, It.IsAny<OtpPurpose>(), It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    [Fact]
    public async Task Handle_WhenUserDoesNotExist_ShouldStillReturnSuccess()
    {
        // Arrange - Security: prevent user enumeration
        string email = "nonexistent@example.com";
        string purpose = EnumOtpPurpose.EmailVerification.ToString();
        PublicResendOtpCommand command = new(Email: email, Purpose: purpose);

        _authRepositoryMock.SetupExistsByEmail(new Email(email), false);

        // Act
        PublicResendOtpResult result = await _handler.Handle(command, CancellationToken.None);

        // Assert - Returns success to prevent user enumeration
        result.IsSuccess.Should().BeTrue();
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
            .ReturnsAsync(otp);

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
            .ReturnsAsync(otp);

        // Act
        await _handler.Handle(command, cts.Token);

        // Assert
        _otpFactoryMock.Verify(x => x.ResendOtpAsync(user.Id, It.IsAny<OtpPurpose>(), cts.Token), Times.Once);
    }

    #endregion
}
