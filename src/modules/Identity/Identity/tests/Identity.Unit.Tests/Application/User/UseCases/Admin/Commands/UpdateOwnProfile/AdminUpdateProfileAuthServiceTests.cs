using _116.Identity.Application.Shared.Errors;
using _116.Identity.Application.Shared.Persistence;
using _116.Identity.Application.Shared.Repositories;
using _116.Identity.Application.User.UseCases.Admin.Commands.UpdateOwnProfile;
using _116.Identity.Application.User.UseCases.Admin.Commands.UpdateOwnProfile.Contracts;
using _116.Identity.Domain.Entities;
using _116.Identity.TestData.Factories;
using _116.Identity.TestData.Mocks.Infrastructure;
using _116.Identity.TestData.Mocks.Repositories;
using _116.Tests.TestData.Helpers;
using AwesomeAssertions;
using Moq;
using Xunit;

namespace _116.Identity.Unit.Tests.Application.User.UseCases.Admin.Commands.UpdateOwnProfile;

/// <summary>
/// Unit tests for <see cref="AdminUpdateProfileAuthService"/>.
/// </summary>
public class AdminUpdateProfileAuthServiceTests
{
    private readonly Mock<IAuthRepository> _authRepositoryMock;
    private readonly Mock<IIdentityUnitOfWork> _unitOfWorkMock;
    private readonly UserErrors _userErrors;
    private readonly AdminUpdateProfileAuthService _service;

    public AdminUpdateProfileAuthServiceTests()
    {
        _authRepositoryMock = MockAuthRepository.Create();
        _unitOfWorkMock = MockIdentityUnitOfWork.Create();
        _userErrors = TestErrorsFactory.CreateUserErrors();
        _service = new AdminUpdateProfileAuthService(_authRepositoryMock.Object, _unitOfWorkMock.Object, _userErrors);
    }

    #region UpdateProfileAsync Tests

    [Fact]
    public async Task UpdateProfileAsync_WithValidData_ShouldReturnAuthData()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var sessionId = Guid.NewGuid();
        string userName = "newusername";
        string countryName = "Rwanda";
        string countryIsoCode = "RW";
        string countryDialCode = "+250";
        string partialPhoneNumber = "788123456";

        UserEntity user = UserFactory.CreateWithId(userId);

        _authRepositoryMock.SetupGetUserWithRolesAndPermissionsById(user);

        _authRepositoryMock.SetupIsUserAccountActiveReturnsTrue();

        _authRepositoryMock.SetupIsSessionValid(sessionId);

        _authRepositoryMock
            .Setup(x => x.ExistsByUserNameAsync(userName, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _authRepositoryMock
            .Setup(x => x.GetUserByPhoneNumberAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((UserEntity?)null);

        _unitOfWorkMock.Setup(x => x.CommitAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        // Act
        AdminUpdateProfileAuthData result = await _service.UpdateProfileAsync(
            userId,
            sessionId,
            userName,
            countryName,
            countryIsoCode,
            countryDialCode,
            partialPhoneNumber,
            null,
            CancellationToken.None
        );

        // Assert
        result.User.Should().Be(user);
    }

    [Fact]
    public async Task UpdateProfileAsync_ShouldValidateUserIsActive()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var sessionId = Guid.NewGuid();
        UserEntity user = UserFactory.CreateWithId(userId);

        _authRepositoryMock.SetupGetUserWithRolesAndPermissionsById(user);

        _authRepositoryMock.SetupIsUserAccountActiveReturnsTrue();

        _authRepositoryMock.SetupIsSessionValid(sessionId);

        _unitOfWorkMock.Setup(x => x.CommitAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        // Act
        await _service.UpdateProfileAsync(
            userId,
            sessionId,
            null,
            null,
            null,
            null,
            null,
            null,
            CancellationToken.None
        );

        // Assert
        _authRepositoryMock.Verify(x => x.IsUserAccountActive(user), Times.Once);
    }

    [Fact]
    public async Task UpdateProfileAsync_ShouldValidateSession()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var sessionId = Guid.NewGuid();
        UserEntity user = UserFactory.CreateWithId(userId);

        _authRepositoryMock.SetupGetUserWithRolesAndPermissionsById(user);

        _authRepositoryMock.SetupIsUserAccountActiveReturnsTrue();

        _authRepositoryMock.SetupIsSessionValid(sessionId);

        _unitOfWorkMock.Setup(x => x.CommitAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        // Act
        await _service.UpdateProfileAsync(
            userId,
            sessionId,
            null,
            null,
            null,
            null,
            null,
            null,
            CancellationToken.None
        );

        // Assert
        _authRepositoryMock.Verify(x => x.IsSessionValidAsync(sessionId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateProfileAsync_WithNewUsername_ShouldCheckUniqueness()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var sessionId = Guid.NewGuid();
        string newUserName = "newusername";
        UserEntity user = UserFactory.CreateWithId(userId);
        user.UpdateUserName("oldusername");

        _authRepositoryMock.SetupGetUserWithRolesAndPermissionsById(user);

        _authRepositoryMock.SetupIsUserAccountActiveReturnsTrue();

        _authRepositoryMock.SetupIsSessionValid(sessionId);

        _authRepositoryMock
            .Setup(x => x.ExistsByUserNameAsync(newUserName, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _unitOfWorkMock.Setup(x => x.CommitAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        // Act
        await _service.UpdateProfileAsync(
            userId,
            sessionId,
            newUserName,
            null,
            null,
            null,
            null,
            null,
            CancellationToken.None
        );

        // Assert
        _authRepositoryMock.Verify(
            x => x.ExistsByUserNameAsync(newUserName, It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    [Fact]
    public async Task UpdateProfileAsync_WithSameUsername_ShouldNotCheckUniqueness()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var sessionId = Guid.NewGuid();
        string userName = "sameusername";
        UserEntity user = UserFactory.CreateWithId(userId);
        user.UpdateUserName(userName);

        _authRepositoryMock.SetupGetUserWithRolesAndPermissionsById(user);

        _authRepositoryMock.SetupIsUserAccountActiveReturnsTrue();

        _authRepositoryMock.SetupIsSessionValid(sessionId);

        _unitOfWorkMock.Setup(x => x.CommitAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        // Act
        await _service.UpdateProfileAsync(
            userId,
            sessionId,
            userName,
            null,
            null,
            null,
            null,
            null,
            CancellationToken.None
        );

        // Assert
        _authRepositoryMock.Verify(
            x => x.ExistsByUserNameAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never
        );
    }

    [Fact]
    public async Task UpdateProfileAsync_WithPhoneNumber_ShouldCheckUniqueness()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var sessionId = Guid.NewGuid();
        string countryDialCode = "+250";
        string partialPhoneNumber = "788123456";
        string fullPhoneNumber = $"{countryDialCode}{partialPhoneNumber}";

        UserEntity user = UserFactory.CreateWithId(userId);

        _authRepositoryMock.SetupGetUserWithRolesAndPermissionsById(user);

        _authRepositoryMock.SetupIsUserAccountActiveReturnsTrue();

        _authRepositoryMock.SetupIsSessionValid(sessionId);

        _authRepositoryMock
            .Setup(x => x.GetUserByPhoneNumberAsync(fullPhoneNumber, It.IsAny<CancellationToken>()))
            .ReturnsAsync((UserEntity?)null);

        _unitOfWorkMock.Setup(x => x.CommitAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        // Act
        await _service.UpdateProfileAsync(
            userId,
            sessionId,
            null,
            "Rwanda",
            "RW",
            countryDialCode,
            partialPhoneNumber,
            null,
            CancellationToken.None
        );

        // Assert
        _authRepositoryMock.Verify(
            x => x.GetUserByPhoneNumberAsync(fullPhoneNumber, It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    [Fact]
    public async Task UpdateProfileAsync_WithPhoneUsedByCurrentUser_ShouldNotThrow()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var sessionId = Guid.NewGuid();
        string countryDialCode = "+250";
        string partialPhoneNumber = "788123456";
        string fullPhoneNumber = $"{countryDialCode}{partialPhoneNumber}";

        UserEntity user = UserFactory.CreateWithId(userId);

        _authRepositoryMock.SetupGetUserWithRolesAndPermissionsById(user);

        _authRepositoryMock.SetupIsUserAccountActiveReturnsTrue();

        _authRepositoryMock.SetupIsSessionValid(sessionId);

        _authRepositoryMock
            .Setup(x => x.GetUserByPhoneNumberAsync(fullPhoneNumber, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _unitOfWorkMock.Setup(x => x.CommitAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        // Act
        AdminUpdateProfileAuthData result = await _service.UpdateProfileAsync(
            userId,
            sessionId,
            null,
            "Rwanda",
            "RW",
            countryDialCode,
            partialPhoneNumber,
            null,
            CancellationToken.None
        );

        // Assert
        result.User.FullPhoneNumber.Should().Be(fullPhoneNumber);
        result.User.CountryDialCode.Should().Be(countryDialCode);
        result.User.PartialPhoneNumber.Should().Be(partialPhoneNumber);
        _unitOfWorkMock.Verify(x => x.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateProfileAsync_ShouldCommitTransaction()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var sessionId = Guid.NewGuid();
        UserEntity user = UserFactory.CreateWithId(userId);

        _authRepositoryMock.SetupGetUserWithRolesAndPermissionsById(user);

        _authRepositoryMock.SetupIsUserAccountActiveReturnsTrue();

        _authRepositoryMock.SetupIsSessionValid(sessionId);

        _unitOfWorkMock.Setup(x => x.CommitAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        // Act
        await _service.UpdateProfileAsync(
            userId,
            sessionId,
            null,
            null,
            null,
            null,
            null,
            null,
            CancellationToken.None
        );

        // Assert
        _unitOfWorkMock.Verify(x => x.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateProfileAsync_WithCancellationToken_ShouldPassToRepository()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var sessionId = Guid.NewGuid();
        UserEntity user = UserFactory.CreateWithId(userId);
        CancellationToken cancellationToken = new();

        _authRepositoryMock
            .Setup(x => x.GetUserWithRolesAndPermissionsByIdOrThrow(userId, cancellationToken))
            .ReturnsAsync(user);

        _authRepositoryMock.SetupIsUserAccountActiveReturnsTrue();

        _authRepositoryMock.Setup(x => x.IsSessionValidAsync(sessionId, cancellationToken)).ReturnsAsync(true);

        _unitOfWorkMock.Setup(x => x.CommitAsync(cancellationToken)).ReturnsAsync(1);

        // Act
        await _service.UpdateProfileAsync(userId, sessionId, null, null, null, null, null, null, cancellationToken);

        // Assert
        _authRepositoryMock.Verify(
            x => x.GetUserWithRolesAndPermissionsByIdOrThrow(userId, cancellationToken),
            Times.Once
        );
        _authRepositoryMock.Verify(x => x.IsSessionValidAsync(sessionId, cancellationToken), Times.Once);
        _unitOfWorkMock.Verify(x => x.CommitAsync(cancellationToken), Times.Once);
    }

    #endregion
}
