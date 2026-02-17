using _116.Identity.Application.Auth.Repositories;
using _116.Identity.Application.Auth.UseCases.Public.Commands.VerifyOtp;
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

namespace _116.Unit.Tests.Modules.Identity.Application.Auth.UseCases.Public.Commands.VerifyOtp;

/// <summary>
/// Unit tests for <see cref="PublicVerifyOtpHandler"/>.
/// </summary>
public class PublicVerifyOtpHandlerTests
{
    private readonly Mock<IAuthRepository> _authRepositoryMock;
    private readonly Mock<IOtpRepository> _otpRepositoryMock;
    private readonly Mock<IIdentityUnitOfWork> _unitOfWorkMock;
    private readonly PublicVerifyOtpHandler _handler;

    public PublicVerifyOtpHandlerTests()
    {
        _authRepositoryMock = MockAuthRepository.Create();
        _otpRepositoryMock = MockOtpRepository.Create();
        _unitOfWorkMock = MockIdentityUnitOfWork.Create();

        _handler = new PublicVerifyOtpHandler(
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
        string email = "user@example.com";
        string code = "123456";
        string purpose = EnumOtpPurpose.EmailVerification.ToString();
        UserEntity user = UserFactory.CreateUnverified();
        OtpEntity otp = OtpFactory.Create(user.Id, code);

        PublicVerifyOtpCommand command = new(Email: email, Code: code, Purpose: purpose);

        _authRepositoryMock.SetupGetUserWithRolesByEmailOrThrow(new Email(email), user);
        _otpRepositoryMock.SetupValidateOtp(otp);

        // Act
        PublicVerifyOtpResult result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_ShouldMarkOtpAsUsed()
    {
        // Arrange
        string email = "user@example.com";
        string code = "123456";
        string purpose = EnumOtpPurpose.EmailVerification.ToString();
        UserEntity user = UserFactory.CreateUnverified();
        OtpEntity otp = OtpFactory.Create(user.Id, code);

        PublicVerifyOtpCommand command = new(Email: email, Code: code, Purpose: purpose);

        _authRepositoryMock.SetupGetUserWithRolesByEmailOrThrow(new Email(email), user);
        _otpRepositoryMock.SetupValidateOtp(otp);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        otp.IsUsed.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_ShouldMarkUserAsVerified()
    {
        // Arrange
        string email = "user@example.com";
        string code = "123456";
        string purpose = EnumOtpPurpose.EmailVerification.ToString();
        UserEntity user = UserFactory.CreateUnverified();
        OtpEntity otp = OtpFactory.Create(user.Id, code);

        PublicVerifyOtpCommand command = new(Email: email, Code: code, Purpose: purpose);

        _authRepositoryMock.SetupGetUserWithRolesByEmailOrThrow(new Email(email), user);
        _otpRepositoryMock.SetupValidateOtp(otp);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        user.IsVerified.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_ShouldInvalidateExistingOtps()
    {
        // Arrange
        string email = "user@example.com";
        string code = "123456";
        string purpose = EnumOtpPurpose.EmailVerification.ToString();
        UserEntity user = UserFactory.CreateUnverified();
        OtpEntity otp = OtpFactory.Create(user.Id, code);

        PublicVerifyOtpCommand command = new(Email: email, Code: code, Purpose: purpose);

        _authRepositoryMock.SetupGetUserWithRolesByEmailOrThrow(new Email(email), user);
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
        string email = "user@example.com";
        string code = "123456";
        string purpose = EnumOtpPurpose.EmailVerification.ToString();
        UserEntity user = UserFactory.CreateUnverified();
        OtpEntity otp = OtpFactory.Create(user.Id, code);

        PublicVerifyOtpCommand command = new(Email: email, Code: code, Purpose: purpose);

        _authRepositoryMock.SetupGetUserWithRolesByEmailOrThrow(new Email(email), user);
        _otpRepositoryMock.SetupValidateOtp(otp);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _unitOfWorkMock.VerifyCommitCalled();
    }

    #endregion

    #region Failure Cases

    [Fact]
    public async Task Handle_WhenUserNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        string email = "nonexistent@example.com";
        PublicVerifyOtpCommand command = new(Email: email, Code: "123456", Purpose: "EmailVerification");

        _authRepositoryMock.SetupGetUserWithRolesByEmailOrThrowNotFound(new Email(email));

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_WhenUserAlreadyVerified_ShouldThrowConflictException()
    {
        // Arrange
        string email = "user@example.com";
        string code = "123456";
        string purpose = EnumOtpPurpose.EmailVerification.ToString();
        UserEntity user = UserFactory.CreateVerifiedActive();

        PublicVerifyOtpCommand command = new(Email: email, Code: code, Purpose: purpose);

        _authRepositoryMock.SetupGetUserWithRolesByEmailOrThrow(new Email(email), user);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ConflictException>();
    }

    [Fact]
    public async Task Handle_WhenOtpInvalid_ShouldThrowBadRequestException()
    {
        // Arrange
        string email = "user@example.com";
        string code = "wrong-code";
        string purpose = EnumOtpPurpose.EmailVerification.ToString();
        UserEntity user = UserFactory.CreateUnverified();

        PublicVerifyOtpCommand command = new(Email: email, Code: code, Purpose: purpose);

        _authRepositoryMock.SetupGetUserWithRolesByEmailOrThrow(new Email(email), user);
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
        string email = "user@example.com";
        string code = "123456";
        string purpose = EnumOtpPurpose.EmailVerification.ToString();
        UserEntity user = UserFactory.CreateUnverified();

        PublicVerifyOtpCommand command = new(Email: email, Code: code, Purpose: purpose);

        _authRepositoryMock.SetupGetUserWithRolesByEmailOrThrow(new Email(email), user);
        _otpRepositoryMock.SetupValidateOtpExpired(user.Id, code, EnumOtpPurpose.EmailVerification);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<AuthenticationException>();
    }

    #endregion

    #region Cancellation Token Tests

    [Fact]
    public async Task Handle_WithCancellationToken_ShouldPassToAuthRepository()
    {
        // Arrange
        string email = "user@example.com";
        string code = "123456";
        string purpose = EnumOtpPurpose.EmailVerification.ToString();
        UserEntity user = UserFactory.CreateUnverified();
        OtpEntity otp = OtpFactory.Create(user.Id, code);
        using CancellationTokenSource cts = new();

        PublicVerifyOtpCommand command = new(Email: email, Code: code, Purpose: purpose);

        _authRepositoryMock.SetupGetUserWithRolesByEmailOrThrow(new Email(email), user);
        _otpRepositoryMock.SetupValidateOtp(otp);

        // Act
        await _handler.Handle(command, cts.Token);

        // Assert
        _authRepositoryMock.Verify(x => x.GetUserWithRolesByEmailOrThrow(It.IsAny<Email>(), cts.Token), Times.Once);
    }

    [Fact]
    public async Task Handle_WithCancellationToken_ShouldPassToUnitOfWork()
    {
        // Arrange
        string email = "user@example.com";
        string code = "123456";
        string purpose = EnumOtpPurpose.EmailVerification.ToString();
        UserEntity user = UserFactory.CreateUnverified();
        OtpEntity otp = OtpFactory.Create(user.Id, code);
        using CancellationTokenSource cts = new();

        PublicVerifyOtpCommand command = new(Email: email, Code: code, Purpose: purpose);

        _authRepositoryMock.SetupGetUserWithRolesByEmailOrThrow(new Email(email), user);
        _otpRepositoryMock.SetupValidateOtp(otp);

        // Act
        await _handler.Handle(command, cts.Token);

        // Assert
        _unitOfWorkMock.Verify(x => x.CommitAsync(cts.Token), Times.Once);
    }

    #endregion
}
