using _116.BuildingBlocks.Application.Exceptions;
using _116.Identity.Application.Auth.Services;
using _116.Identity.Application.Auth.UseCases.Public.Commands.VerifyOtp;
using _116.Identity.Application.Shared.Persistence;
using _116.Identity.Application.Shared.Repositories;
using _116.Identity.Domain.Entities;
using _116.Identity.Domain.Enums;
using _116.Identity.Domain.ValueObjects;
using _116.Identity.TestData.Factories;
using _116.Identity.TestData.Mocks.Infrastructure;
using _116.Identity.TestData.Mocks.Repositories;
using _116.Tests.TestData.Helpers;
using AwesomeAssertions;
using Moq;
using Xunit;

namespace _116.Identity.Unit.Tests.Application.Auth.UseCases.Public.Commands.VerifyOtp;

/// <summary>
/// Unit tests for <see cref="PublicVerifyOtpHandler"/>: the already-verified gate, the OTP
/// consumption call, the aggregate transition and the commit. The consumption protocol itself is
/// covered by <c>OtpVerificationServiceTests</c>.
/// </summary>
public class PublicVerifyOtpHandlerTests
{
    private const string Email = "user@example.com";
    private const string Code = "123456";

    private readonly Mock<IAuthRepository> _authRepositoryMock;
    private readonly Mock<IOtpVerificationService> _otpVerificationServiceMock;
    private readonly Mock<IIdentityUnitOfWork> _unitOfWorkMock;
    private readonly PublicVerifyOtpHandler _handler;

    public PublicVerifyOtpHandlerTests()
    {
        _authRepositoryMock = MockAuthRepository.Create();
        _otpVerificationServiceMock = new Mock<IOtpVerificationService>();
        _unitOfWorkMock = MockIdentityUnitOfWork.Create();

        _handler = new PublicVerifyOtpHandler(
            _authRepositoryMock.Object,
            _otpVerificationServiceMock.Object,
            _unitOfWorkMock.Object,
            TestErrorsFactory.CreateIdentityI18n()
        );
    }

    private static PublicVerifyOtpCommand Command(EnumOtpPurpose purpose = EnumOtpPurpose.EmailVerification)
    {
        return new PublicVerifyOtpCommand(Email: Email, Code: Code, Purpose: purpose.ToString());
    }

    #region Success Cases

    [Fact]
    public async Task Handle_ShouldConsumeTheOtpForTheUserAndPurpose()
    {
        // Arrange
        UserEntity user = UserFactory.CreateUnverified();
        _authRepositoryMock.SetupGetUserWithRolesByEmailOrThrow(new Email(Email), user);

        // Act
        await _handler.Handle(Command(), CancellationToken.None);

        // Assert
        _otpVerificationServiceMock.Verify(
            x =>
                x.ConsumeOtpAsync(
                    user.Id,
                    It.Is<OtpPurpose>(p => p.Value == EnumOtpPurpose.EmailVerification),
                    Code,
                    It.IsAny<CancellationToken>()
                ),
            Times.Once
        );
    }

    [Fact]
    public async Task Handle_ShouldMarkUserAsVerified()
    {
        // Arrange
        UserEntity user = UserFactory.CreateUnverified();
        _authRepositoryMock.SetupGetUserWithRolesByEmailOrThrow(new Email(Email), user);

        // Act
        await _handler.Handle(Command(), CancellationToken.None);

        // Assert
        user.IsVerified.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_WithAPasswordResetPurpose_ShouldNotMarkUserAsVerified()
    {
        // Arrange
        UserEntity user = UserFactory.CreateUnverified();
        _authRepositoryMock.SetupGetUserWithRolesByEmailOrThrow(new Email(Email), user);

        // Act
        await _handler.Handle(Command(EnumOtpPurpose.PasswordReset), CancellationToken.None);

        // Assert
        user.IsVerified.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_WithAPasswordResetPurpose_ShouldAcceptAnAlreadyVerifiedUser()
    {
        // Arrange
        UserEntity user = UserFactory.CreateVerifiedActive();
        _authRepositoryMock.SetupGetUserWithRolesByEmailOrThrow(new Email(Email), user);

        // Act
        await _handler.Handle(Command(EnumOtpPurpose.PasswordReset), CancellationToken.None);

        // Assert
        _unitOfWorkMock.VerifyCommitCalled();
    }

    [Fact]
    public async Task Handle_ShouldCommitUnitOfWork()
    {
        // Arrange
        UserEntity user = UserFactory.CreateUnverified();
        _authRepositoryMock.SetupGetUserWithRolesByEmailOrThrow(new Email(Email), user);

        // Act
        await _handler.Handle(Command(), CancellationToken.None);

        // Assert
        _unitOfWorkMock.VerifyCommitCalled();
    }

    #endregion

    #region Failure Cases

    [Fact]
    public async Task Handle_WhenUserNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        _authRepositoryMock.SetupGetUserWithRolesByEmailOrThrowNotFound(new Email(Email));

        // Act
        Func<Task> act = async () => await _handler.Handle(Command(), CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_WhenUserAlreadyVerified_ShouldThrowConflictExceptionBeforeConsumingAnything()
    {
        // Arrange
        UserEntity user = UserFactory.CreateVerifiedActive();
        _authRepositoryMock.SetupGetUserWithRolesByEmailOrThrow(new Email(Email), user);

        // Act
        Func<Task> act = async () => await _handler.Handle(Command(), CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ConflictException>();
        _otpVerificationServiceMock.Verify(
            x =>
                x.ConsumeOtpAsync(
                    It.IsAny<Guid>(),
                    It.IsAny<OtpPurpose>(),
                    It.IsAny<string>(),
                    It.IsAny<CancellationToken>()
                ),
            Times.Never
        );
    }

    [Fact]
    public async Task Handle_WhenTheOtpIsRefused_ShouldNeitherVerifyTheUserNorCommit()
    {
        // Arrange
        UserEntity user = UserFactory.CreateUnverified();
        _authRepositoryMock.SetupGetUserWithRolesByEmailOrThrow(new Email(Email), user);
        _otpVerificationServiceMock
            .Setup(x =>
                x.ConsumeOtpAsync(
                    It.IsAny<Guid>(),
                    It.IsAny<OtpPurpose>(),
                    It.IsAny<string>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .ThrowsAsync(TestErrorsFactory.CreateIdentityI18n().User.InvalidOtpCode());

        // Act
        Func<Task> act = async () => await _handler.Handle(Command(), CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<BadRequestException>();
        user.IsVerified.Should().BeFalse();
        _unitOfWorkMock.VerifyCommitNotCalled();
    }

    #endregion

    #region Cancellation Token Tests

    [Fact]
    public async Task Handle_WithCancellationToken_ShouldPassToAuthRepository()
    {
        // Arrange
        UserEntity user = UserFactory.CreateUnverified();
        using CancellationTokenSource cts = new();
        _authRepositoryMock.SetupGetUserWithRolesByEmailOrThrow(new Email(Email), user);

        // Act
        await _handler.Handle(Command(), cts.Token);

        // Assert
        _authRepositoryMock.Verify(x => x.GetUserWithRolesByEmailOrThrow(It.IsAny<Email>(), cts.Token), Times.Once);
    }

    [Fact]
    public async Task Handle_WithCancellationToken_ShouldPassToUnitOfWork()
    {
        // Arrange
        UserEntity user = UserFactory.CreateUnverified();
        using CancellationTokenSource cts = new();
        _authRepositoryMock.SetupGetUserWithRolesByEmailOrThrow(new Email(Email), user);

        // Act
        await _handler.Handle(Command(), cts.Token);

        // Assert
        _unitOfWorkMock.Verify(x => x.CommitAsync(cts.Token), Times.Once);
    }

    #endregion
}
