using _116.Core.Application.Shared.Repositories;
using _116.Core.Domain.Entities;
using _116.Identity.Application.Auth.UseCases.Public.Commands.SocialLogin;
using _116.Identity.Application.Auth.UseCases.Public.Commands.SocialLogin.Contracts;
using _116.Identity.Application.Shared.Persistence;
using _116.Identity.Application.Shared.Repositories;
using _116.Identity.Domain.Entities;
using _116.Identity.Domain.Enums;
using _116.Identity.Domain.ValueObjects;
using _116.Tests.Fixtures.Factories;
using AwesomeAssertions;
using Moq;
using Xunit;

namespace _116.Unit.Tests.Modules.Identity.Application.Auth.UseCases.Public.Commands.SocialLogin;

/// <summary>
/// Unit tests for <see cref="PublicSocialLoginAuthFactory"/>.
/// </summary>
public class PublicSocialLoginAuthFactoryTests
{
    private readonly Mock<IAuthRepository> _authRepositoryMock;
    private readonly Mock<IFileRepository> _fileRepositoryMock;
    private readonly Mock<IIdentityUnitOfWork> _unitOfWorkMock;
    private readonly PublicSocialLoginAuthFactory _factory;

    public PublicSocialLoginAuthFactoryTests()
    {
        _authRepositoryMock = new Mock<IAuthRepository>();
        _fileRepositoryMock = new Mock<IFileRepository>();
        _unitOfWorkMock = new Mock<IIdentityUnitOfWork>();
        _factory = new PublicSocialLoginAuthFactory(
            _authRepositoryMock.Object,
            _fileRepositoryMock.Object,
            _unitOfWorkMock.Object
        );
    }

    #region AuthenticateOrCreateAsync Tests

    [Fact]
    public async Task AuthenticateOrCreateAsync_WithValidData_ShouldReturnAuthData()
    {
        // Arrange
        string email = "user@example.com";
        string userName = "socialuser";
        string provider = "Google";
        string? avatarUrl = "https://avatar.url/image.jpg";
        UserEntity user = UserFactory.Create(email);
        FileEntity avatarFile = FileFactory.Create();

        _authRepositoryMock
            .Setup(x =>
                x.GetOrCreateExternalUserAsync(
                    It.IsAny<string>(),
                    userName,
                    It.IsAny<AuthProvider>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(user);

        _fileRepositoryMock
            .Setup(x =>
                x.UpdateAvatarUrlFromSourceAsync(
                    user.AvatarFileId,
                    avatarUrl,
                    user.Id.ToString(),
                    It.IsAny<bool>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(avatarFile);

        _unitOfWorkMock.Setup(x => x.CommitAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        // Act
        PublicSocialLoginAuthData result = await _factory.AuthenticateOrCreateAsync(
            email,
            userName,
            provider,
            avatarUrl,
            CancellationToken.None
        );

        // Assert
        result.Should().NotBeNull();
        result.User.Should().Be(user);
    }

    [Fact]
    public async Task AuthenticateOrCreateAsync_WithoutAvatarUrl_ShouldNotUpdateAvatar()
    {
        // Arrange
        string email = "user@example.com";
        string userName = "socialuser";
        string provider = "Google";
        string? avatarUrl = null;
        UserEntity user = UserFactory.Create(email);

        _authRepositoryMock
            .Setup(x =>
                x.GetOrCreateExternalUserAsync(
                    It.IsAny<string>(),
                    userName,
                    It.IsAny<AuthProvider>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(user);

        _fileRepositoryMock
            .Setup(x =>
                x.UpdateAvatarUrlFromSourceAsync(
                    user.AvatarFileId,
                    avatarUrl,
                    user.Id.ToString(),
                    It.IsAny<bool>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync((FileEntity?)null);

        _unitOfWorkMock.Setup(x => x.CommitAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        // Act
        PublicSocialLoginAuthData result = await _factory.AuthenticateOrCreateAsync(
            email,
            userName,
            provider,
            avatarUrl,
            CancellationToken.None
        );

        // Assert
        result.Should().NotBeNull();
        _fileRepositoryMock.Verify(
            x =>
                x.UpdateAvatarUrlFromSourceAsync(
                    user.AvatarFileId,
                    avatarUrl,
                    user.Id.ToString(),
                    It.IsAny<bool>(),
                    It.IsAny<CancellationToken>()
                ),
            Times.Once
        );
    }

    [Fact]
    public async Task AuthenticateOrCreateAsync_ShouldGetOrCreateExternalUser()
    {
        // Arrange
        string email = "user@example.com";
        string userName = "socialuser";
        string provider = "Google";
        string? avatarUrl = null;
        UserEntity user = UserFactory.Create(email);

        _authRepositoryMock
            .Setup(x =>
                x.GetOrCreateExternalUserAsync(
                    It.Is<string>(e => e == email),
                    userName,
                    It.Is<AuthProvider>(p => ((string)p) == provider),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(user);

        _fileRepositoryMock
            .Setup(x =>
                x.UpdateAvatarUrlFromSourceAsync(
                    It.IsAny<Guid?>(),
                    It.IsAny<string?>(),
                    It.IsAny<string>(),
                    It.IsAny<bool>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync((FileEntity?)null);

        _unitOfWorkMock.Setup(x => x.CommitAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        // Act
        await _factory.AuthenticateOrCreateAsync(email, userName, provider, avatarUrl, CancellationToken.None);

        // Assert
        _authRepositoryMock.Verify(
            x =>
                x.GetOrCreateExternalUserAsync(
                    It.Is<string>(e => e == email),
                    userName,
                    It.Is<AuthProvider>(p => ((string)p) == provider),
                    It.IsAny<CancellationToken>()
                ),
            Times.Once
        );
    }

    [Fact]
    public async Task AuthenticateOrCreateAsync_ShouldUpdateAvatarUrlFromSource()
    {
        // Arrange
        string email = "user@example.com";
        string userName = "socialuser";
        string provider = "Google";
        string? avatarUrl = "https://avatar.url/image.jpg";
        UserEntity user = UserFactory.Create(email);

        _authRepositoryMock
            .Setup(x =>
                x.GetOrCreateExternalUserAsync(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<AuthProvider>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(user);

        _fileRepositoryMock
            .Setup(x =>
                x.UpdateAvatarUrlFromSourceAsync(
                    user.AvatarFileId,
                    avatarUrl,
                    user.Id.ToString(),
                    It.IsAny<bool>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync((FileEntity?)null);

        _unitOfWorkMock.Setup(x => x.CommitAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        // Act
        await _factory.AuthenticateOrCreateAsync(email, userName, provider, avatarUrl, CancellationToken.None);

        // Assert
        _fileRepositoryMock.Verify(
            x =>
                x.UpdateAvatarUrlFromSourceAsync(
                    user.AvatarFileId,
                    avatarUrl,
                    user.Id.ToString(),
                    It.IsAny<bool>(),
                    It.IsAny<CancellationToken>()
                ),
            Times.Once
        );
    }

    [Fact]
    public async Task AuthenticateOrCreateAsync_WithManualAvatarSource_ShouldPassCorrectFlag()
    {
        // Arrange
        string email = "user@example.com";
        string userName = "socialuser";
        string provider = "Google";
        string? avatarUrl = "https://avatar.url/image.jpg";
        UserEntity user = UserFactory.Create(email);
        user.UpdateAvatar(Guid.NewGuid(), EnumAvatarSource.Manual);

        _authRepositoryMock
            .Setup(x =>
                x.GetOrCreateExternalUserAsync(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<AuthProvider>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(user);

        _fileRepositoryMock
            .Setup(x =>
                x.UpdateAvatarUrlFromSourceAsync(
                    user.AvatarFileId,
                    avatarUrl,
                    user.Id.ToString(),
                    true,
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync((FileEntity?)null);

        _unitOfWorkMock.Setup(x => x.CommitAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        // Act
        await _factory.AuthenticateOrCreateAsync(email, userName, provider, avatarUrl, CancellationToken.None);

        // Assert
        _fileRepositoryMock.Verify(
            x =>
                x.UpdateAvatarUrlFromSourceAsync(
                    user.AvatarFileId,
                    avatarUrl,
                    user.Id.ToString(),
                    true,
                    It.IsAny<CancellationToken>()
                ),
            Times.Once
        );
    }

    [Fact]
    public async Task AuthenticateOrCreateAsync_WithAvatarFileReturned_ShouldUpdateUserAvatar()
    {
        // Arrange
        string email = "user@example.com";
        string userName = "socialuser";
        string provider = "Google";
        string? avatarUrl = "https://avatar.url/image.jpg";
        var avatarFileId = Guid.NewGuid();
        UserEntity user = UserFactory.Create(email);
        FileEntity avatarFile = FileFactory.CreateWithId(avatarFileId);

        _authRepositoryMock
            .Setup(x =>
                x.GetOrCreateExternalUserAsync(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<AuthProvider>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(user);

        _fileRepositoryMock
            .Setup(x =>
                x.UpdateAvatarUrlFromSourceAsync(
                    user.AvatarFileId,
                    avatarUrl,
                    user.Id.ToString(),
                    It.IsAny<bool>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(avatarFile);

        _unitOfWorkMock.Setup(x => x.CommitAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        // Act
        await _factory.AuthenticateOrCreateAsync(email, userName, provider, avatarUrl, CancellationToken.None);

        // Assert
        user.AvatarFileId.Should().Be(avatarFileId);
        user.AvatarSource.Should().Be(EnumAvatarSource.Provider);
    }

    [Fact]
    public async Task AuthenticateOrCreateAsync_ShouldCommitTransaction()
    {
        // Arrange
        string email = "user@example.com";
        string userName = "socialuser";
        string provider = "Google";
        string? avatarUrl = null;
        UserEntity user = UserFactory.Create(email);

        _authRepositoryMock
            .Setup(x =>
                x.GetOrCreateExternalUserAsync(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<AuthProvider>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(user);

        _fileRepositoryMock
            .Setup(x =>
                x.UpdateAvatarUrlFromSourceAsync(
                    It.IsAny<Guid?>(),
                    It.IsAny<string?>(),
                    It.IsAny<string>(),
                    It.IsAny<bool>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync((FileEntity?)null);

        _unitOfWorkMock.Setup(x => x.CommitAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        // Act
        await _factory.AuthenticateOrCreateAsync(email, userName, provider, avatarUrl, CancellationToken.None);

        // Assert
        _unitOfWorkMock.Verify(x => x.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task AuthenticateOrCreateAsync_WithCancellationToken_ShouldPassToRepositories()
    {
        // Arrange
        string email = "user@example.com";
        string userName = "socialuser";
        string provider = "Google";
        string? avatarUrl = "https://avatar.url/image.jpg";
        UserEntity user = UserFactory.Create(email);
        CancellationToken cancellationToken = new();

        _authRepositoryMock
            .Setup(x =>
                x.GetOrCreateExternalUserAsync(
                    It.IsAny<string>(),
                    userName,
                    It.IsAny<AuthProvider>(),
                    cancellationToken
                )
            )
            .ReturnsAsync(user);

        _fileRepositoryMock
            .Setup(x =>
                x.UpdateAvatarUrlFromSourceAsync(
                    user.AvatarFileId,
                    avatarUrl,
                    user.Id.ToString(),
                    It.IsAny<bool>(),
                    cancellationToken
                )
            )
            .ReturnsAsync((FileEntity?)null);

        _unitOfWorkMock.Setup(x => x.CommitAsync(cancellationToken)).ReturnsAsync(1);

        // Act
        await _factory.AuthenticateOrCreateAsync(email, userName, provider, avatarUrl, cancellationToken);

        // Assert
        _authRepositoryMock.Verify(
            x =>
                x.GetOrCreateExternalUserAsync(
                    It.IsAny<string>(),
                    userName,
                    It.IsAny<AuthProvider>(),
                    cancellationToken
                ),
            Times.Once
        );
        _fileRepositoryMock.Verify(
            x =>
                x.UpdateAvatarUrlFromSourceAsync(
                    user.AvatarFileId,
                    avatarUrl,
                    user.Id.ToString(),
                    It.IsAny<bool>(),
                    cancellationToken
                ),
            Times.Once
        );
        _unitOfWorkMock.Verify(x => x.CommitAsync(cancellationToken), Times.Once);
    }

    #endregion
}
