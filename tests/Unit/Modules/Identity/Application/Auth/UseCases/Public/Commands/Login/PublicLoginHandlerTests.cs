using _116.Core.Application.Shared.Repositories;
using _116.Identity.Application.Auth.UseCases.Public.Commands.Login;
using _116.Identity.Application.Auth.UseCases.Public.Commands.Login.Contracts;
using _116.Identity.Application.Session.Factories.Contracts;
using _116.Identity.Domain.Entities;
using _116.Shared.Application.Exceptions;
using _116.Tests.Fixtures.Constants;
using _116.Tests.Fixtures.Factories;
using _116.Tests.Fixtures.Helpers;
using _116.Unit.Tests.Common;
using _116.Unit.Tests.Common.Mocks.Repositories;
using AwesomeAssertions;
using Moq;
using Xunit;

namespace _116.Unit.Tests.Modules.Identity.Application.Auth.UseCases.Public.Commands.Login;

/// <summary>
/// Unit tests for <see cref="PublicLoginHandler"/>.
/// </summary>
public class PublicLoginHandlerTests : BaseHandlerTest
{
    private readonly Mock<IPublicLoginAuthFactory> _authFactoryMock;
    private readonly Mock<ISessionFactory> _sessionFactoryMock;
    private readonly Mock<IFileRepository> _fileRepositoryMock;
    private readonly PublicLoginHandler _handler;

    public PublicLoginHandlerTests()
    {
        _authFactoryMock = new Mock<IPublicLoginAuthFactory>();
        _sessionFactoryMock = new Mock<ISessionFactory>();
        _fileRepositoryMock = MockFileRepository.Create();

        _handler = new PublicLoginHandler(
            _authFactoryMock.Object,
            _sessionFactoryMock.Object,
            _fileRepositoryMock.Object,
            Mapper
        );
    }

    #region Success Cases

    [Fact]
    public async Task Handle_WithValidCredentials_ShouldReturnAuthenticationResult()
    {
        // Arrange
        string credentials = TestConstants.Auth.PublicLoginEmail;
        string password = TestConstants.Auth.PublicLoginPassword;
        UserEntity user = UserFactory.CreateVerifiedActive();
        PublicLoginAuthData authData = AuthTestHelpers.CreatePublicLoginAuthData(user);
        List<RolePermissionEntity> permissions = authData.UserPermissions;
        SessionResult sessionResult = AuthTestHelpers.CreateDefaultSessionResult();

        PublicLoginCommand command = new(Credentials: credentials, Password: password);

        _authFactoryMock
            .Setup(x => x.AuthenticateAsync(credentials, password, It.IsAny<CancellationToken>()))
            .ReturnsAsync(authData);
        _sessionFactoryMock
            .Setup(x => x.CreateSessionAsync(user, permissions, It.IsAny<CancellationToken>()))
            .ReturnsAsync(sessionResult);
        _fileRepositoryMock.SetupGetAvatarFileReturnsNull(user.AvatarFileId);

        // Act
        PublicLoginResult result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.AuthenticationResult.Should().NotBeNull();
        result.AuthenticationResult.AccessToken.Should().Be("access-token");
        result.AuthenticationResult.RefreshToken.Should().Be("refresh-token");
    }

    [Fact]
    public async Task Handle_ShouldAuthenticateUser()
    {
        // Arrange
        string credentials = TestConstants.Auth.PublicLoginEmail;
        string password = TestConstants.Auth.PublicLoginPassword;
        UserEntity user = UserFactory.CreateVerifiedActive();
        PublicLoginAuthData authData = AuthTestHelpers.CreatePublicLoginAuthData(user);
        List<RolePermissionEntity> permissions = authData.UserPermissions;
        SessionResult sessionResult = AuthTestHelpers.CreateDefaultSessionResult();

        PublicLoginCommand command = new(Credentials: credentials, Password: password);

        _authFactoryMock
            .Setup(x => x.AuthenticateAsync(credentials, password, It.IsAny<CancellationToken>()))
            .ReturnsAsync(authData);
        _sessionFactoryMock
            .Setup(x => x.CreateSessionAsync(user, permissions, It.IsAny<CancellationToken>()))
            .ReturnsAsync(sessionResult);
        _fileRepositoryMock.SetupGetAvatarFileReturnsNull(user.AvatarFileId);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _authFactoryMock.Verify(
            x => x.AuthenticateAsync(credentials, password, It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    [Fact]
    public async Task Handle_ShouldCreateSession()
    {
        // Arrange
        string credentials = TestConstants.Auth.PublicLoginEmail;
        string password = TestConstants.Auth.PublicLoginPassword;
        UserEntity user = UserFactory.CreateVerifiedActive();
        PublicLoginAuthData authData = AuthTestHelpers.CreatePublicLoginAuthData(user);
        List<RolePermissionEntity> permissions = authData.UserPermissions;
        SessionResult sessionResult = AuthTestHelpers.CreateDefaultSessionResult();

        PublicLoginCommand command = new(Credentials: credentials, Password: password);

        _authFactoryMock
            .Setup(x => x.AuthenticateAsync(credentials, password, It.IsAny<CancellationToken>()))
            .ReturnsAsync(authData);
        _sessionFactoryMock
            .Setup(x => x.CreateSessionAsync(user, permissions, It.IsAny<CancellationToken>()))
            .ReturnsAsync(sessionResult);
        _fileRepositoryMock.SetupGetAvatarFileReturnsNull(user.AvatarFileId);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _sessionFactoryMock.Verify(
            x => x.CreateSessionAsync(user, permissions, It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    [Fact]
    public async Task Handle_ShouldFetchUserAvatar()
    {
        // Arrange
        string credentials = TestConstants.Auth.PublicLoginEmail;
        string password = TestConstants.Auth.PublicLoginPassword;
        UserEntity user = UserFactory.CreateVerifiedActive();
        PublicLoginAuthData authData = AuthTestHelpers.CreatePublicLoginAuthData(user);
        List<RolePermissionEntity> permissions = authData.UserPermissions;
        SessionResult sessionResult = AuthTestHelpers.CreateDefaultSessionResult();

        PublicLoginCommand command = new(Credentials: credentials, Password: password);

        _authFactoryMock
            .Setup(x => x.AuthenticateAsync(credentials, password, It.IsAny<CancellationToken>()))
            .ReturnsAsync(authData);
        _sessionFactoryMock
            .Setup(x => x.CreateSessionAsync(user, permissions, It.IsAny<CancellationToken>()))
            .ReturnsAsync(sessionResult);
        _fileRepositoryMock.SetupGetAvatarFileReturnsNull(user.AvatarFileId);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _fileRepositoryMock.Verify(
            x => x.GetAvatarFileAsync(user.AvatarFileId, It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    #endregion

    #region Failure Cases

    [Fact]
    public async Task Handle_WhenAuthenticationFails_ShouldThrowBadRequestException()
    {
        // Arrange
        string credentials = TestConstants.Auth.PublicLoginEmail;
        string password = TestConstants.Auth.PublicLoginInvalidPassword;
        PublicLoginCommand command = new(Credentials: credentials, Password: password);

        _authFactoryMock
            .Setup(x => x.AuthenticateAsync(credentials, password, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new BadRequestException("Invalid credentials."));

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<BadRequestException>();
    }

    [Fact]
    public async Task Handle_WhenUserNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        string credentials = TestConstants.Auth.NonExistentEmail;
        string password = TestConstants.Auth.PublicLoginPassword;
        PublicLoginCommand command = new(Credentials: credentials, Password: password);

        _authFactoryMock
            .Setup(x => x.AuthenticateAsync(credentials, password, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new NotFoundException("User not found."));

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_WhenAuthenticationFails_ShouldNotCreateSession()
    {
        // Arrange
        string credentials = TestConstants.Auth.PublicLoginEmail;
        string password = TestConstants.Auth.PublicLoginInvalidPassword;
        PublicLoginCommand command = new(Credentials: credentials, Password: password);

        _authFactoryMock
            .Setup(x => x.AuthenticateAsync(credentials, password, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new BadRequestException("Invalid credentials."));

        // Act
        try
        {
            await _handler.Handle(command, CancellationToken.None);
        }
        catch (BadRequestException)
        {
            // Expected
        }

        // Assert
        _sessionFactoryMock.Verify(
            x =>
                x.CreateSessionAsync(
                    It.IsAny<UserEntity>(),
                    It.IsAny<List<RolePermissionEntity>>(),
                    It.IsAny<CancellationToken>()
                ),
            Times.Never
        );
    }

    #endregion

    #region Cancellation Token Tests

    [Fact]
    public async Task Handle_WithCancellationToken_ShouldPassToAuthFactory()
    {
        // Arrange
        string credentials = TestConstants.Auth.PublicLoginEmail;
        string password = TestConstants.Auth.PublicLoginPassword;
        UserEntity user = UserFactory.CreateVerifiedActive();
        PublicLoginAuthData authData = AuthTestHelpers.CreatePublicLoginAuthData(user);
        List<RolePermissionEntity> permissions = authData.UserPermissions;
        SessionResult sessionResult = AuthTestHelpers.CreateDefaultSessionResult();
        using CancellationTokenSource cts = new();

        PublicLoginCommand command = new(Credentials: credentials, Password: password);

        _authFactoryMock
            .Setup(x => x.AuthenticateAsync(credentials, password, It.IsAny<CancellationToken>()))
            .ReturnsAsync(authData);
        _sessionFactoryMock
            .Setup(x => x.CreateSessionAsync(user, permissions, It.IsAny<CancellationToken>()))
            .ReturnsAsync(sessionResult);
        _fileRepositoryMock.SetupGetAvatarFileReturnsNull(user.AvatarFileId);

        // Act
        await _handler.Handle(command, cts.Token);

        // Assert
        _authFactoryMock.Verify(x => x.AuthenticateAsync(credentials, password, cts.Token), Times.Once);
    }

    #endregion
}
