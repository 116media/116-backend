using _116.Identity.Application.Auth.Repositories;
using _116.Identity.Application.Auth.Services;
using _116.Identity.Application.Auth.UseCases.Admin.Commands.ResendOtp;
using _116.Identity.Application.Shared.Persistence;
using _116.Identity.Domain.Entities;
using _116.Identity.Domain.Enums;
using _116.Identity.Domain.ValueObjects;
using _116.Tests.Fixtures.Factories;
using _116.Unit.Tests.Common.Mocks.Infrastructure;
using _116.Unit.Tests.Common.Mocks.Repositories;
using _116.Unit.Tests.Common.Mocks.Services;
using AwesomeAssertions;
using Moq;
using Xunit;

namespace _116.Unit.Tests.Modules.Identity.Application.Auth.UseCases.Admin.Commands.ResendOtp;

/// <summary>
/// Unit tests for <see cref="AdminResendOtpFactory"/>.
/// </summary>
public class AdminResendOtpFactoryTests
{
    private readonly Mock<IOtpRepository> _otpRepositoryMock;
    private readonly Mock<IOtpService> _otpServiceMock;
    private readonly Mock<IIdentityUnitOfWork> _unitOfWorkMock;
    private readonly AdminResendOtpFactory _factory;

    public AdminResendOtpFactoryTests()
    {
        _otpRepositoryMock = MockOtpRepository.Create();
        _otpServiceMock = MockOtpService.Create();
        _unitOfWorkMock = MockIdentityUnitOfWork.Create();

        _factory = new AdminResendOtpFactory(_otpRepositoryMock.Object, _otpServiceMock.Object, _unitOfWorkMock.Object);
    }

    #region Success Cases

    [Fact]
    public async Task ResendOtpAsync_WithValidData_ShouldReturnNewOtp()
    {
        // Arrange
        var userId = Guid.NewGuid();
        OtpPurpose purpose = EnumOtpPurpose.PasswordReset;
        OtpEntity otp = OtpFactory.CreateForPasswordReset(userId);

        _otpServiceMock.SetupCreateOtpReturns(otp);

        // Act
        OtpEntity result = await _factory.ResendOtpAsync(userId, purpose, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.UserId.Should().Be(userId);
    }

    [Fact]
    public async Task ResendOtpAsync_ShouldInvalidateExistingOtps()
    {
        // Arrange
        var userId = Guid.NewGuid();
        OtpPurpose purpose = EnumOtpPurpose.EmailVerification;
        OtpEntity otp = OtpFactory.CreateForEmailVerification(userId);

        _otpServiceMock.SetupCreateOtpReturns(otp);

        // Act
        await _factory.ResendOtpAsync(userId, purpose, CancellationToken.None);

        // Assert
        _otpRepositoryMock.Verify(
            x => x.InvalidateExistingOtpsAsync(userId, purpose, It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    [Fact]
    public async Task ResendOtpAsync_ShouldCreateNewOtp()
    {
        // Arrange
        var userId = Guid.NewGuid();
        OtpPurpose purpose = EnumOtpPurpose.PasswordReset;
        OtpEntity otp = OtpFactory.CreateForPasswordReset(userId);

        _otpServiceMock.SetupCreateOtpReturns(otp);

        // Act
        await _factory.ResendOtpAsync(userId, purpose, CancellationToken.None);

        // Assert
        _otpServiceMock.Verify(x => x.CreateOtp(userId, purpose), Times.Once);
    }

    [Fact]
    public async Task ResendOtpAsync_ShouldAddNewOtpToRepository()
    {
        // Arrange
        var userId = Guid.NewGuid();
        OtpPurpose purpose = EnumOtpPurpose.PasswordReset;
        OtpEntity otp = OtpFactory.CreateForPasswordReset(userId);

        _otpServiceMock.SetupCreateOtpReturns(otp);

        // Act
        await _factory.ResendOtpAsync(userId, purpose, CancellationToken.None);

        // Assert
        _otpRepositoryMock.Verify(x => x.AddAsync(otp, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ResendOtpAsync_ShouldCommitTransaction()
    {
        // Arrange
        var userId = Guid.NewGuid();
        OtpPurpose purpose = EnumOtpPurpose.PasswordReset;
        OtpEntity otp = OtpFactory.CreateForPasswordReset(userId);

        _otpServiceMock.SetupCreateOtpReturns(otp);

        // Act
        await _factory.ResendOtpAsync(userId, purpose, CancellationToken.None);

        // Assert
        _unitOfWorkMock.Verify(x => x.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Theory]
    [InlineData(EnumOtpPurpose.EmailVerification)]
    [InlineData(EnumOtpPurpose.PasswordReset)]
    public async Task ResendOtpAsync_WithDifferentPurposes_ShouldWork(EnumOtpPurpose purposeEnum)
    {
        // Arrange
        var userId = Guid.NewGuid();
        OtpPurpose purpose = purposeEnum;
        OtpEntity otp =
            purposeEnum == EnumOtpPurpose.EmailVerification
                ? OtpFactory.CreateForEmailVerification(userId)
                : OtpFactory.CreateForPasswordReset(userId);

        _otpServiceMock.SetupCreateOtpReturns(otp);

        // Act
        OtpEntity result = await _factory.ResendOtpAsync(userId, purpose, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
    }

    #endregion

    #region Cancellation Token Tests

    [Fact]
    public async Task ResendOtpAsync_WithCancellationToken_ShouldPassToAllDependencies()
    {
        // Arrange
        var userId = Guid.NewGuid();
        OtpPurpose purpose = EnumOtpPurpose.PasswordReset;
        OtpEntity otp = OtpFactory.CreateForPasswordReset(userId);
        using CancellationTokenSource cts = new();

        _otpServiceMock.SetupCreateOtpReturns(otp);

        // Act
        await _factory.ResendOtpAsync(userId, purpose, cts.Token);

        // Assert
        _otpRepositoryMock.Verify(x => x.InvalidateExistingOtpsAsync(userId, purpose, cts.Token), Times.Once);
        _otpRepositoryMock.Verify(x => x.AddAsync(otp, cts.Token), Times.Once);
        _unitOfWorkMock.Verify(x => x.CommitAsync(cts.Token), Times.Once);
    }

    #endregion
}
