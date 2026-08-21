using _116.Identity.Application.Auth.Repositories;
using _116.Identity.Application.Auth.Services;
using _116.Identity.Application.Auth.UseCases.Public.Commands.SignUp;
using _116.Identity.Application.Auth.UseCases.Public.Commands.SignUp.Contracts;
using _116.Identity.Application.Shared.Persistence;
using _116.Identity.Application.Shared.Repositories;
using _116.Identity.Domain.Entities;
using _116.Identity.Domain.Enums;
using _116.Identity.Domain.ValueObjects;
using _116.Mailer.Contracts.Application;
using _116.Mailer.Contracts.Domain;
using _116.Shared.Application.Exceptions;
using _116.Tests.Fixtures.Constants;
using _116.Tests.Fixtures.Factories.Identity;
using _116.Tests.Fixtures.Helpers;
using _116.Unit.Tests.Common.Mocks.Infrastructure;
using _116.Unit.Tests.Common.Mocks.Repositories;
using _116.Unit.Tests.Common.Mocks.Services;
using AwesomeAssertions;
using Moq;
using Xunit;

namespace _116.Unit.Tests.Modules.Identity.Application.Auth.UseCases.Public.Commands.SignUp;

/// <summary>
/// Unit tests for <see cref="PublicSignUpAuthFactory"/>.
/// </summary>
public class PublicSignUpAuthFactoryTests
{
    private readonly Mock<IAuthRepository> _authRepositoryMock;
    private readonly Mock<IOtpRepository> _otpRepositoryMock;
    private readonly Mock<IPasswordService> _passwordServiceMock;
    private readonly Mock<IOtpService> _otpServiceMock;
    private readonly Mock<IUserTokenStateRepository> _tokenStateRepositoryMock;
    private readonly Mock<IIdentityUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IMailer> _mailerMock = new();
    private readonly PublicSignUpAuthFactory _factory;

    public PublicSignUpAuthFactoryTests()
    {
        _authRepositoryMock = MockAuthRepository.Create();
        _otpRepositoryMock = MockOtpRepository.Create();
        _passwordServiceMock = MockPasswordService.Create();
        _otpServiceMock = MockOtpService.Create();
        _tokenStateRepositoryMock = new Mock<IUserTokenStateRepository>();
        _unitOfWorkMock = MockIdentityUnitOfWork.Create();

        _factory = new PublicSignUpAuthFactory(
            _authRepositoryMock.Object,
            _otpRepositoryMock.Object,
            _passwordServiceMock.Object,
            _otpServiceMock.Object,
            _tokenStateRepositoryMock.Object,
            _unitOfWorkMock.Object,
            TestErrorsFactory.CreateUserErrors(),
            _mailerMock.Object
        );
    }

    #region Success Cases

    [Fact]
    public async Task RegisterAsync_ShouldMailThePlainCodeWhilePersistingOnlyItsHash()
    {
        // Arrange
        string email = "newuser@example.com";
        string userName = "newuser";
        string password = "ValidPassword123!";
        UserEntity user = UserFactory.CreateVerifiedActive();
        OtpEntity otp = OtpFactory.CreateForEmailVerification(Guid.NewGuid());

        _authRepositoryMock.SetupValidateUniqueCredentialsSuccess();
        _passwordServiceMock.SetupHashReturns("hashed");
        _otpServiceMock.SetupCreateOtpReturns(otp, TestConstants.Otp.DefaultCode);
        _authRepositoryMock.SetupGetUserWithRolesByCredentials(user);

        // Act
        await _factory.RegisterAsync(email, userName, password, CancellationToken.None);

        // Assert
        _mailerMock.Verify(
            x =>
                x.EnqueueAsync(
                    EnumEmailTemplate.EmailVerificationOtp,
                    It.IsAny<EmailRecipient>(),
                    It.Is<IReadOnlyDictionary<string, string>>(t => t["otpCode"] == TestConstants.Otp.DefaultCode),
                    It.IsAny<string>(),
                    It.IsAny<CancellationToken>()
                ),
            Times.Once
        );
        _otpRepositoryMock.Verify(
            x =>
                x.AddAsync(
                    It.Is<OtpEntity>(o => o.CodeHash != TestConstants.Otp.DefaultCode),
                    It.IsAny<CancellationToken>()
                ),
            Times.Once
        );
    }

    [Fact]
    public async Task RegisterAsync_WithValidData_ShouldReturnAuthData()
    {
        // Arrange
        string email = "newuser@example.com";
        string userName = "newuser";
        string password = "ValidPassword123!";
        string hashedPassword = "hashed_password";
        UserEntity user = UserFactory.CreateVerifiedActive();
        OtpEntity otp = OtpFactory.CreateForEmailVerification(Guid.NewGuid());

        _authRepositoryMock.SetupValidateUniqueCredentialsSuccess();
        _passwordServiceMock.SetupHashReturns(hashedPassword);
        _otpServiceMock.SetupCreateOtpReturns(otp);
        _authRepositoryMock.SetupGetUserWithRolesByCredentials(user);

        // Act
        PublicSignUpAuthData result = await _factory.RegisterAsync(email, userName, password, CancellationToken.None);

        // Assert
        result.User.Should().BeSameAs(user);
    }

    [Fact]
    public async Task RegisterAsync_ShouldValidateUniqueCredentials()
    {
        // Arrange
        string email = "newuser@example.com";
        string userName = "newuser";
        string password = "ValidPassword123!";
        UserEntity user = UserFactory.CreateVerifiedActive();
        OtpEntity otp = OtpFactory.CreateForEmailVerification(Guid.NewGuid());

        _authRepositoryMock.SetupValidateUniqueCredentialsSuccess();
        _passwordServiceMock.SetupHashReturns("hashed");
        _otpServiceMock.SetupCreateOtpReturns(otp);
        _authRepositoryMock.SetupGetUserWithRolesByCredentials(user);

        // Act
        await _factory.RegisterAsync(email, userName, password, CancellationToken.None);

        // Assert
        _authRepositoryMock.Verify(
            x =>
                x.ValidateUniqueCredentialsAsync(
                    It.Is<Email>(e => e.Value == email),
                    userName,
                    It.IsAny<CancellationToken>()
                ),
            Times.Once
        );
    }

    [Fact]
    public async Task RegisterAsync_ShouldHashPassword()
    {
        // Arrange
        string email = "newuser@example.com";
        string userName = "newuser";
        string password = "ValidPassword123!";
        UserEntity user = UserFactory.CreateVerifiedActive();
        OtpEntity otp = OtpFactory.CreateForEmailVerification(Guid.NewGuid());

        _authRepositoryMock.SetupValidateUniqueCredentialsSuccess();
        _passwordServiceMock.SetupHashReturns("hashed");
        _otpServiceMock.SetupCreateOtpReturns(otp);
        _authRepositoryMock.SetupGetUserWithRolesByCredentials(user);

        // Act
        await _factory.RegisterAsync(email, userName, password, CancellationToken.None);

        // Assert
        _passwordServiceMock.Verify(x => x.Hash(password), Times.Once);
    }

    [Fact]
    public async Task RegisterAsync_ShouldAddUser()
    {
        // Arrange
        string email = "newuser@example.com";
        string userName = "newuser";
        string password = "ValidPassword123!";
        UserEntity user = UserFactory.CreateVerifiedActive();
        OtpEntity otp = OtpFactory.CreateForEmailVerification(Guid.NewGuid());

        _authRepositoryMock.SetupValidateUniqueCredentialsSuccess();
        _passwordServiceMock.SetupHashReturns("hashed");
        _otpServiceMock.SetupCreateOtpReturns(otp);
        _authRepositoryMock.SetupGetUserWithRolesByCredentials(user);

        // Act
        await _factory.RegisterAsync(email, userName, password, CancellationToken.None);

        // Assert
        _authRepositoryMock.VerifyAddCalled();
        _tokenStateRepositoryMock.Verify(
            x => x.AddAsync(It.IsAny<UserTokenStateEntity>(), It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    [Fact]
    public async Task RegisterAsync_ShouldAssignVisitorRole()
    {
        // Arrange
        string email = "newuser@example.com";
        string userName = "newuser";
        string password = "ValidPassword123!";
        UserEntity user = UserFactory.CreateVerifiedActive();
        OtpEntity otp = OtpFactory.CreateForEmailVerification(Guid.NewGuid());

        _authRepositoryMock.SetupValidateUniqueCredentialsSuccess();
        _passwordServiceMock.SetupHashReturns("hashed");
        _otpServiceMock.SetupCreateOtpReturns(otp);
        _authRepositoryMock.SetupGetUserWithRolesByCredentials(user);

        // Act
        await _factory.RegisterAsync(email, userName, password, CancellationToken.None);

        // Assert
        _authRepositoryMock.Verify(
            x => x.AssignVisitorRoleAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    [Fact]
    public async Task RegisterAsync_ShouldCreateVerificationOtp()
    {
        // Arrange
        string email = "newuser@example.com";
        string userName = "newuser";
        string password = "ValidPassword123!";
        UserEntity user = UserFactory.CreateVerifiedActive();
        OtpEntity otp = OtpFactory.CreateForEmailVerification(Guid.NewGuid());

        _authRepositoryMock.SetupValidateUniqueCredentialsSuccess();
        _passwordServiceMock.SetupHashReturns("hashed");
        _otpServiceMock.SetupCreateOtpReturns(otp);
        _authRepositoryMock.SetupGetUserWithRolesByCredentials(user);

        // Act
        await _factory.RegisterAsync(email, userName, password, CancellationToken.None);

        // Assert
        _otpServiceMock.Verify(x => x.CreateOtp(It.IsAny<Guid>(), EnumOtpPurpose.EmailVerification), Times.Once);
    }

    [Fact]
    public async Task RegisterAsync_ShouldAddOtp()
    {
        // Arrange
        string email = "newuser@example.com";
        string userName = "newuser";
        string password = "ValidPassword123!";
        UserEntity user = UserFactory.CreateVerifiedActive();
        OtpEntity otp = OtpFactory.CreateForEmailVerification(Guid.NewGuid());

        _authRepositoryMock.SetupValidateUniqueCredentialsSuccess();
        _passwordServiceMock.SetupHashReturns("hashed");
        _otpServiceMock.SetupCreateOtpReturns(otp);
        _authRepositoryMock.SetupGetUserWithRolesByCredentials(user);

        // Act
        await _factory.RegisterAsync(email, userName, password, CancellationToken.None);

        // Assert
        _otpRepositoryMock.Verify(x => x.AddAsync(otp, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RegisterAsync_ShouldCommitTransaction()
    {
        // Arrange
        string email = "newuser@example.com";
        string userName = "newuser";
        string password = "ValidPassword123!";
        UserEntity user = UserFactory.CreateVerifiedActive();
        OtpEntity otp = OtpFactory.CreateForEmailVerification(Guid.NewGuid());

        _authRepositoryMock.SetupValidateUniqueCredentialsSuccess();
        _passwordServiceMock.SetupHashReturns("hashed");
        _otpServiceMock.SetupCreateOtpReturns(otp);
        _authRepositoryMock.SetupGetUserWithRolesByCredentials(user);

        // Act
        await _factory.RegisterAsync(email, userName, password, CancellationToken.None);

        // Assert
        _unitOfWorkMock.Verify(x => x.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    #endregion

    #region Failure Cases

    [Fact]
    public async Task RegisterAsync_WhenEmailAlreadyExists_ShouldThrowConflictException()
    {
        // Arrange
        string email = "existing@example.com";
        string userName = "newuser";
        string password = "ValidPassword123!";

        _authRepositoryMock
            .Setup(x => x.ValidateUniqueCredentialsAsync(It.IsAny<Email>(), userName, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ConflictException("Email already exists."));

        // Act
        Func<Task> act = async () => await _factory.RegisterAsync(email, userName, password, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ConflictException>();
    }

    [Fact]
    public async Task RegisterAsync_WhenUserNameAlreadyExists_ShouldThrowConflictException()
    {
        // Arrange
        string email = "newuser@example.com";
        string userName = "existinguser";
        string password = "ValidPassword123!";

        _authRepositoryMock
            .Setup(x => x.ValidateUniqueCredentialsAsync(It.IsAny<Email>(), userName, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ConflictException("Username already exists."));

        // Act
        Func<Task> act = async () => await _factory.RegisterAsync(email, userName, password, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ConflictException>();
    }

    [Fact]
    public async Task RegisterAsync_WhenCredentialsNotUnique_ShouldNotAddUser()
    {
        // Arrange
        string email = "existing@example.com";
        string userName = "existinguser";
        string password = "ValidPassword123!";

        _authRepositoryMock
            .Setup(x => x.ValidateUniqueCredentialsAsync(It.IsAny<Email>(), userName, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ConflictException("Credentials already exist."));

        // Act
        Func<Task> act = async () => await _factory.RegisterAsync(email, userName, password, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ConflictException>();
        _authRepositoryMock.Verify(x => x.AddAsync(It.IsAny<UserEntity>(), It.IsAny<CancellationToken>()), Times.Never);
        _tokenStateRepositoryMock.Verify(
            x => x.AddAsync(It.IsAny<UserTokenStateEntity>(), It.IsAny<CancellationToken>()),
            Times.Never
        );
    }

    #endregion

    #region Cancellation Token Tests

    [Fact]
    public async Task RegisterAsync_WithCancellationToken_ShouldPassToAllDependencies()
    {
        // Arrange
        string email = "newuser@example.com";
        string userName = "newuser";
        string password = "ValidPassword123!";
        UserEntity user = UserFactory.CreateVerifiedActive();
        OtpEntity otp = OtpFactory.CreateForEmailVerification(Guid.NewGuid());
        using CancellationTokenSource cts = new();

        _authRepositoryMock.SetupValidateUniqueCredentialsSuccess();
        _passwordServiceMock.SetupHashReturns("hashed");
        _otpServiceMock.SetupCreateOtpReturns(otp);
        _authRepositoryMock.SetupGetUserWithRolesByCredentials(user);

        // Act
        await _factory.RegisterAsync(email, userName, password, cts.Token);

        // Assert
        _authRepositoryMock.Verify(
            x => x.ValidateUniqueCredentialsAsync(It.IsAny<Email>(), userName, cts.Token),
            Times.Once
        );
        _unitOfWorkMock.Verify(x => x.CommitAsync(cts.Token), Times.Once);
    }

    #endregion
}
