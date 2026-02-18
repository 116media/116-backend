using _116.Core.Application.Shared.Repositories;
using _116.Core.Domain.Entities;
using _116.Identity.Application.Auth.UseCases.Public.Commands.SocialLogin;
using _116.Identity.Application.Auth.UseCases.Public.Commands.SocialLogin.Contracts;
using _116.Identity.Application.Session.Factories;
using _116.Identity.Domain.Entities;
using _116.Tests.Fixtures.Constants;
using _116.Tests.Fixtures.Factories;
using _116.Tests.Fixtures.Helpers;
using _116.Unit.Tests.Common;
using _116.Unit.Tests.Common.Mocks.Repositories;
using AwesomeAssertions;
using Moq;
using Xunit;

namespace _116.Unit.Tests.Modules.Identity.Application.Auth.UseCases.Public.Commands.SocialLogin;

/// <summary>
/// Unit tests for <see cref="PublicSocialLoginHandler"/>.
/// </summary>
public class PublicSocialLoginHandlerTests : BaseHandlerTest
{
    private readonly Mock<IPublicSocialLoginAuthFactory> _authFactoryMock;
    private readonly Mock<ISessionFactory> _sessionFactoryMock;
    private readonly Mock<IFileRepository> _fileRepositoryMock;
    private readonly PublicSocialLoginHandler _handler;

    public PublicSocialLoginHandlerTests()
    {
        _authFactoryMock = new Mock<IPublicSocialLoginAuthFactory>();
        _sessionFactoryMock = new Mock<ISessionFactory>();
        _fileRepositoryMock = MockFileRepository.Create();

        _handler = new PublicSocialLoginHandler(
            _authFactoryMock.Object,
            _sessionFactoryMock.Object,
            _fileRepositoryMock.Object,
            Mapper
        );
    }

    #region Success Cases

    [Fact]
    public async Task Handle_WithValidSocialLoginData_ShouldReturnAuthenticationResult()
    {
        // Arrange
        UserEntity user = UserFactory.CreateVerifiedActive();
        PublicSocialLoginAuthData authData = AuthTestHelpers.CreatePublicSocialLoginAuthData(user);
        List<RolePermissionEntity> permissions = authData.UserPermissions;
        SessionResult sessionResult = AuthTestHelpers.CreateDefaultSessionResult();

        PublicSocialLoginCommand command = new(
            Email: TestConstants.Auth.SocialLoginEmail,
            UserName: TestConstants.Auth.SocialLoginUserName,
            AvatarUrl: TestConstants.Auth.SocialLoginAvatarUrl,
            Provider: TestConstants.Auth.ProviderGoogle
        );

        _authFactoryMock
            .Setup(x =>
                x.AuthenticateOrCreateAsync(
                    TestConstants.Auth.SocialLoginEmail,
                    TestConstants.Auth.SocialLoginUserName,
                    TestConstants.Auth.ProviderGoogle,
                    TestConstants.Auth.SocialLoginAvatarUrl,
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(authData);
        _sessionFactoryMock
            .Setup(x => x.CreateSessionAsync(user, permissions, It.IsAny<CancellationToken>()))
            .ReturnsAsync(sessionResult);
        _fileRepositoryMock.SetupGetAvatarFileReturnsNull(user.AvatarFileId);

        // Act
        PublicSocialLoginResult result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.AuthenticationResult.Should().NotBeNull();
        result.AuthenticationResult.AccessToken.Should().Be("access-token");
        result.AuthenticationResult.RefreshToken.Should().Be("refresh-token");
    }

    [Fact]
    public async Task Handle_WithValidSocialLoginData_ShouldReturnUserInformation()
    {
        // Arrange
        string? avatarUrl = null;

        PublicSocialLoginCommand command = new(
            Email: TestConstants.Auth.SocialLoginEmail,
            UserName: TestConstants.Auth.SocialLoginUserName,
            AvatarUrl: avatarUrl,
            Provider: TestConstants.Auth.ProviderGoogle
        );

        UserEntity user = UserFactory.CreateVerifiedActive();
        PublicSocialLoginAuthData authData = AuthTestHelpers.CreatePublicSocialLoginAuthData(user);
        List<RolePermissionEntity> permissions = authData.UserPermissions;
        SessionResult sessionResult = AuthTestHelpers.CreateDefaultSessionResult();

        _authFactoryMock
            .Setup(x =>
                x.AuthenticateOrCreateAsync(
                    TestConstants.Auth.SocialLoginEmail,
                    TestConstants.Auth.SocialLoginUserName,
                    TestConstants.Auth.ProviderGoogle,
                    avatarUrl,
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(authData);
        _sessionFactoryMock
            .Setup(x => x.CreateSessionAsync(user, permissions, It.IsAny<CancellationToken>()))
            .ReturnsAsync(sessionResult);
        _fileRepositoryMock.SetupGetAvatarFileReturnsNull(user.AvatarFileId);

        // Act
        PublicSocialLoginResult result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.AuthenticationResult.User.Should().NotBeNull();
        result.AuthenticationResult.User.Id.Should().Be(user.Id);
    }

    [Fact]
    public async Task Handle_WithAvatarFile_ShouldIncludeAvatarInResult()
    {
        // Arrange
        FileEntity avatarFile = FileFactory.Create();

        PublicSocialLoginCommand command = new(
            Email: TestConstants.Auth.SocialLoginEmail,
            UserName: TestConstants.Auth.SocialLoginUserName,
            AvatarUrl: TestConstants.Auth.SocialLoginAvatarUrl,
            Provider: TestConstants.Auth.ProviderGoogle
        );

        UserEntity user = UserFactory.CreateVerifiedActive();
        PublicSocialLoginAuthData authData = AuthTestHelpers.CreatePublicSocialLoginAuthData(user);
        List<RolePermissionEntity> permissions = authData.UserPermissions;
        SessionResult sessionResult = AuthTestHelpers.CreateDefaultSessionResult();

        _authFactoryMock
            .Setup(x =>
                x.AuthenticateOrCreateAsync(
                    TestConstants.Auth.SocialLoginEmail,
                    TestConstants.Auth.SocialLoginUserName,
                    TestConstants.Auth.ProviderGoogle,
                    TestConstants.Auth.SocialLoginAvatarUrl,
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(authData);
        _sessionFactoryMock
            .Setup(x => x.CreateSessionAsync(user, permissions, It.IsAny<CancellationToken>()))
            .ReturnsAsync(sessionResult);
        _fileRepositoryMock.SetupGetAvatarFile(user.AvatarFileId, avatarFile);

        // Act
        PublicSocialLoginResult result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.AuthenticationResult.User.Avatar.Should().NotBeNull();
        result.AuthenticationResult.User.Avatar!.Id.Should().Be(avatarFile.Id);
    }

    #endregion

    #region Dependency Verification Tests

    [Fact]
    public async Task Handle_ShouldCallAuthenticateOrCreateAsync()
    {
        // Arrange
        PublicSocialLoginCommand command = new(
            Email: TestConstants.Auth.SocialLoginEmail,
            UserName: TestConstants.Auth.SocialLoginUserName,
            AvatarUrl: TestConstants.Auth.SocialLoginAvatarUrl,
            Provider: TestConstants.Auth.ProviderGoogle
        );

        UserEntity user = UserFactory.CreateVerifiedActive();
        PublicSocialLoginAuthData authData = AuthTestHelpers.CreatePublicSocialLoginAuthData(user);
        List<RolePermissionEntity> permissions = authData.UserPermissions;
        SessionResult sessionResult = AuthTestHelpers.CreateDefaultSessionResult();

        _authFactoryMock
            .Setup(x =>
                x.AuthenticateOrCreateAsync(
                    TestConstants.Auth.SocialLoginEmail,
                    TestConstants.Auth.SocialLoginUserName,
                    TestConstants.Auth.ProviderGoogle,
                    TestConstants.Auth.SocialLoginAvatarUrl,
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(authData);
        _sessionFactoryMock
            .Setup(x => x.CreateSessionAsync(user, permissions, It.IsAny<CancellationToken>()))
            .ReturnsAsync(sessionResult);
        _fileRepositoryMock.SetupGetAvatarFileReturnsNull(user.AvatarFileId);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _authFactoryMock.Verify(
            x =>
                x.AuthenticateOrCreateAsync(
                    TestConstants.Auth.SocialLoginEmail,
                    TestConstants.Auth.SocialLoginUserName,
                    TestConstants.Auth.ProviderGoogle,
                    TestConstants.Auth.SocialLoginAvatarUrl,
                    It.IsAny<CancellationToken>()
                ),
            Times.Once
        );
    }

    [Fact]
    public async Task Handle_ShouldCreateSession()
    {
        // Arrange
        string? avatarUrl = null;

        PublicSocialLoginCommand command = new(
            Email: TestConstants.Auth.SocialLoginEmail,
            UserName: TestConstants.Auth.SocialLoginUserName,
            AvatarUrl: avatarUrl,
            Provider: TestConstants.Auth.ProviderGoogle
        );

        UserEntity user = UserFactory.CreateVerifiedActive();
        PublicSocialLoginAuthData authData = AuthTestHelpers.CreatePublicSocialLoginAuthData(user);
        List<RolePermissionEntity> permissions = authData.UserPermissions;
        SessionResult sessionResult = AuthTestHelpers.CreateDefaultSessionResult();

        _authFactoryMock
            .Setup(x =>
                x.AuthenticateOrCreateAsync(
                    TestConstants.Auth.SocialLoginEmail,
                    TestConstants.Auth.SocialLoginUserName,
                    TestConstants.Auth.ProviderGoogle,
                    avatarUrl,
                    It.IsAny<CancellationToken>()
                )
            )
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
        PublicSocialLoginCommand command = new(
            Email: TestConstants.Auth.SocialLoginEmail,
            UserName: TestConstants.Auth.SocialLoginUserName,
            AvatarUrl: TestConstants.Auth.SocialLoginAvatarUrl,
            Provider: TestConstants.Auth.ProviderGoogle
        );

        UserEntity user = UserFactory.CreateVerifiedActive();
        PublicSocialLoginAuthData authData = AuthTestHelpers.CreatePublicSocialLoginAuthData(user);
        List<RolePermissionEntity> permissions = authData.UserPermissions;
        SessionResult sessionResult = AuthTestHelpers.CreateDefaultSessionResult();

        _authFactoryMock
            .Setup(x =>
                x.AuthenticateOrCreateAsync(
                    TestConstants.Auth.SocialLoginEmail,
                    TestConstants.Auth.SocialLoginUserName,
                    TestConstants.Auth.ProviderGoogle,
                    TestConstants.Auth.SocialLoginAvatarUrl,
                    It.IsAny<CancellationToken>()
                )
            )
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

    #region Edge Cases

    [Fact]
    public async Task Handle_WithNullAvatarUrl_ShouldStillAuthenticate()
    {
        // Arrange
        string? avatarUrl = null;

        PublicSocialLoginCommand command = new(
            Email: TestConstants.Auth.SocialLoginEmail,
            UserName: TestConstants.Auth.SocialLoginUserName,
            AvatarUrl: avatarUrl,
            Provider: TestConstants.Auth.ProviderGoogle
        );

        UserEntity user = UserFactory.CreateVerifiedActive();
        PublicSocialLoginAuthData authData = AuthTestHelpers.CreatePublicSocialLoginAuthData(user);
        List<RolePermissionEntity> permissions = authData.UserPermissions;
        SessionResult sessionResult = AuthTestHelpers.CreateDefaultSessionResult();

        _authFactoryMock
            .Setup(x =>
                x.AuthenticateOrCreateAsync(
                    TestConstants.Auth.SocialLoginEmail,
                    TestConstants.Auth.SocialLoginUserName,
                    TestConstants.Auth.ProviderGoogle,
                    avatarUrl,
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(authData);
        _sessionFactoryMock
            .Setup(x => x.CreateSessionAsync(user, permissions, It.IsAny<CancellationToken>()))
            .ReturnsAsync(sessionResult);
        _fileRepositoryMock.SetupGetAvatarFileReturnsNull(user.AvatarFileId);

        // Act
        PublicSocialLoginResult result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.AuthenticationResult.Should().NotBeNull();
    }

    [Fact]
    public async Task Handle_WithCancellationToken_ShouldPassToAuthFactory()
    {
        // Arrange
        PublicSocialLoginCommand command = new(
            Email: TestConstants.Auth.SocialLoginEmail,
            UserName: TestConstants.Auth.SocialLoginUserName,
            AvatarUrl: TestConstants.Auth.SocialLoginAvatarUrl,
            Provider: TestConstants.Auth.ProviderGitHub
        );

        UserEntity user = UserFactory.CreateVerifiedActive();
        PublicSocialLoginAuthData authData = AuthTestHelpers.CreatePublicSocialLoginAuthData(user);
        List<RolePermissionEntity> permissions = authData.UserPermissions;
        SessionResult sessionResult = AuthTestHelpers.CreateDefaultSessionResult();

        using CancellationTokenSource cts = new();

        _authFactoryMock
            .Setup(x =>
                x.AuthenticateOrCreateAsync(
                    TestConstants.Auth.SocialLoginEmail,
                    TestConstants.Auth.SocialLoginUserName,
                    TestConstants.Auth.ProviderGitHub,
                    TestConstants.Auth.SocialLoginAvatarUrl,
                    cts.Token
                )
            )
            .ReturnsAsync(authData);
        _sessionFactoryMock.Setup(x => x.CreateSessionAsync(user, permissions, cts.Token)).ReturnsAsync(sessionResult);
        _fileRepositoryMock.SetupGetAvatarFileReturnsNull(user.AvatarFileId);

        // Act
        await _handler.Handle(command, cts.Token);

        // Assert
        _authFactoryMock.Verify(
            x =>
                x.AuthenticateOrCreateAsync(
                    TestConstants.Auth.SocialLoginEmail,
                    TestConstants.Auth.SocialLoginUserName,
                    TestConstants.Auth.ProviderGitHub,
                    TestConstants.Auth.SocialLoginAvatarUrl,
                    cts.Token
                ),
            Times.Once
        );
    }

    [Fact]
    public async Task Handle_WithDifferentProviders_ShouldWork()
    {
        // Arrange
        string[] providers =
        [
            TestConstants.Auth.ProviderGoogle,
            TestConstants.Auth.ProviderGitHub,
            TestConstants.Auth.ProviderFacebook,
            TestConstants.Auth.ProviderMicrosoft,
        ];

        foreach (string provider in providers)
        {
            string email = $"user@{provider.ToLower()}.com";
            string userName = $"{provider.ToLower()}user";
            string? avatarUrl = $"https://{provider.ToLower()}.com/avatar.jpg";

            UserEntity user = UserFactory.CreateVerifiedActive();
            PublicSocialLoginAuthData authData = AuthTestHelpers.CreatePublicSocialLoginAuthData(user);
            List<RolePermissionEntity> permissions = authData.UserPermissions;
            SessionResult sessionResult = AuthTestHelpers.CreateDefaultSessionResult();

            PublicSocialLoginCommand command = new(
                Email: email,
                UserName: userName,
                AvatarUrl: avatarUrl,
                Provider: provider
            );

            _authFactoryMock
                .Setup(x =>
                    x.AuthenticateOrCreateAsync(email, userName, provider, avatarUrl, It.IsAny<CancellationToken>())
                )
                .ReturnsAsync(authData);
            _sessionFactoryMock
                .Setup(x => x.CreateSessionAsync(user, permissions, It.IsAny<CancellationToken>()))
                .ReturnsAsync(sessionResult);
            _fileRepositoryMock.SetupGetAvatarFileReturnsNull(user.AvatarFileId);

            // Act
            PublicSocialLoginResult result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.AuthenticationResult.Should().NotBeNull();
        }
    }

    #endregion
}
