using _116.Identity.Application.Auth.Repositories;
using _116.Identity.Application.Auth.Services;
using _116.Identity.Application.Auth.UseCases.Public.Commands.ForgotPassword;
using _116.Identity.Application.Shared.Persistence;
using _116.Identity.Domain.Entities;
using _116.Identity.Domain.Enums;
using _116.Tests.Fixtures.Factories;
using _116.Unit.Tests.Common.Mocks.Infrastructure;
using _116.Unit.Tests.Common.Mocks.Repositories;
using _116.Unit.Tests.Common.Mocks.Services;
using AwesomeAssertions;
using Moq;
using Xunit;

namespace _116.Unit.Tests.Modules.Identity.Application.Auth.UseCases.Public.Commands.ForgotPassword;

/// <summary>
/// Unit tests for <see cref="PublicForgotPasswordOtpFactory"/>.
/// </summary>
public class PublicForgotPasswordOtpFactoryTests
{
    private readonly Mock<IOtpRepository> _otpRepositoryMock;
    private readonly Mock<IOtpService> _otpServiceMock;
    private readonly Mock<IIdentityUnitOfWork> _unitOfWorkMock;
    private readonly PublicForgotPasswordOtpFactory _factory;

    public PublicForgotPasswordOtpFactoryTests()
    {
        _otpRepositoryMock = MockOtpRepository.Create();
        _otpServiceMock = MockOtpService.Create();
        _unitOfWorkMock = MockIdentityUnitOfWork.Create();

        _factory = new PublicForgotPasswordOtpFactory(
            _otpRepositoryMock.Object,
            _otpServiceMock.Object,
            _unitOfWorkMock.Object
        );
    }

    #region Success Cases

    [Fact]
    public async Task CreatePasswordResetOtpAsync_WithValidUserId_ShouldReturnOtp()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        OtpEntity otp = OtpFactory.CreateForPasswordReset(userId);

        _otpServiceMock.SetupCreateOtpReturns(otp);

        // Act
        OtpEntity result = await _factory.CreatePasswordResetOtpAsync(userId, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.UserId.Should().Be(userId);
        result.Purpose.Should().Be(EnumOtpPurpose.PasswordReset);
    }

    [Fact]
    public async Task CreatePasswordResetOtpAsync_ShouldCallOtpService()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        OtpEntity otp = OtpFactory.CreateForPasswordReset(userId);

        _otpServiceMock.SetupCreateOtpReturns(otp);

        // Act
        await _factory.CreatePasswordResetOtpAsync(userId, CancellationToken.None);

        // Assert
        _otpServiceMock.Verify(x => x.CreateOtp(userId, EnumOtpPurpose.PasswordReset), Times.Once);
    }

    [Fact]
    public async Task CreatePasswordResetOtpAsync_ShouldAddOtpToRepository()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        OtpEntity otp = OtpFactory.CreateForPasswordReset(userId);

        _otpServiceMock.SetupCreateOtpReturns(otp);

        // Act
        await _factory.CreatePasswordResetOtpAsync(userId, CancellationToken.None);

        // Assert
        _otpRepositoryMock.Verify(x => x.AddAsync(otp, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreatePasswordResetOtpAsync_ShouldCommitTransaction()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        OtpEntity otp = OtpFactory.CreateForPasswordReset(userId);

        _otpServiceMock.SetupCreateOtpReturns(otp);

        // Act
        await _factory.CreatePasswordResetOtpAsync(userId, CancellationToken.None);

        // Assert
        _unitOfWorkMock.Verify(x => x.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    #endregion

    #region Cancellation Token Tests

    [Fact]
    public async Task CreatePasswordResetOtpAsync_WithCancellationToken_ShouldPassToAllDependencies()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        OtpEntity otp = OtpFactory.CreateForPasswordReset(userId);
        using CancellationTokenSource cts = new();

        _otpServiceMock.SetupCreateOtpReturns(otp);

        // Act
        await _factory.CreatePasswordResetOtpAsync(userId, cts.Token);

        // Assert
        _otpRepositoryMock.Verify(x => x.AddAsync(otp, cts.Token), Times.Once);
        _unitOfWorkMock.Verify(x => x.CommitAsync(cts.Token), Times.Once);
    }

    #endregion
}
