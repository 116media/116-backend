using _116.Core.Application.Shared.Services;
using _116.Core.Contracts.Application.DTOs;
using _116.Core.Contracts.Application.Services;
using _116.Core.Domain.Entities;
using _116.Identity.Application.Adapters.SocialAuth;
using _116.Identity.Application.Auth.UseCases.Public.Commands.SocialLogin;
using _116.Identity.Application.Auth.UseCases.Public.Commands.SocialLogin.Contracts;
using _116.Identity.Application.Shared.Persistence;
using _116.Identity.Application.Shared.Repositories;
using _116.Identity.Application.User.Services;
using _116.Identity.Domain.Entities;
using _116.Identity.Domain.Enums;
using _116.Identity.Domain.ValueObjects;
using _116.Tests.Fixtures.Factories.Core;
using _116.Tests.Fixtures.Factories.Identity;
using _116.Unit.Tests.Common.Mocks.Infrastructure;
using _116.Unit.Tests.Common.Mocks.Services;
using AwesomeAssertions;
using Moq;
using Xunit;

namespace _116.Unit.Tests.Modules.Identity.Application.Auth.UseCases.Public.Commands.SocialLogin;

/// <summary>
/// Unit tests for <see cref="PublicSocialLoginAuthFactory"/>.
/// </summary>
public class PublicSocialLoginAuthFactoryTests
{
    private const EnumAuthProvider Provider = EnumAuthProvider.Google;

    private readonly Mock<IAvatarService> _avatarServiceMock;
    private readonly Mock<IAuthRepository> _authRepositoryMock;
    private readonly Mock<IIdentityUnitOfWork> _unitOfWorkMock;
    private readonly PublicSocialLoginAuthFactory _factory;

    public PublicSocialLoginAuthFactoryTests()
    {
        _authRepositoryMock = new Mock<IAuthRepository>();
        _avatarServiceMock = MockAvatarService.Create();
        _unitOfWorkMock = MockIdentityUnitOfWork.Create();
        _factory = new PublicSocialLoginAuthFactory(
            _authRepositoryMock.Object,
            _avatarServiceMock.Object,
            _unitOfWorkMock.Object
        );
    }

    private static SocialTokenPayload Payload(string email, string userName, string? pictureUrl) =>
        new(
            ProviderSubjectId: $"sub-{Guid.NewGuid():N}",
            Email: email,
            EmailVerified: true,
            Name: userName,
            PictureUrl: pictureUrl
        );

    #region AuthenticateOrCreateAsync Tests

    [Fact]
    public async Task AuthenticateOrCreateAsync_WithValidData_ShouldReturnAuthData()
    {
        // Arrange
        SocialTokenPayload payload = Payload("user@example.com", "socialuser", "https://avatar.url/image.jpg");
        UserEntity user = UserFactory.Create(payload.Email);
        FileReferenceDto avatarFile = FileReferenceDtoFactory.Create();

        _authRepositoryMock
            .Setup(x =>
                x.GetOrCreateExternalUserAsync(
                    It.IsAny<string>(),
                    payload.Name,
                    It.IsAny<AuthProvider>(),
                    It.IsAny<string>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(user);

        _avatarServiceMock
            .Setup(x => x.UploadFromUrlAsync(user.AvatarFileId, payload.PictureUrl!, It.IsAny<CancellationToken>()))
            .ReturnsAsync(StoredFileFactory.From(avatarFile));

        // Act
        PublicSocialLoginAuthData result = await _factory.AuthenticateOrCreateAsync(
            payload,
            Provider,
            CancellationToken.None
        );

        // Assert
        result.User.Should().Be(user);
    }

    [Fact]
    public async Task AuthenticateOrCreateAsync_WithoutPicture_ShouldNotUpdateAvatar()
    {
        // Arrange
        SocialTokenPayload payload = Payload("user@example.com", "socialuser", pictureUrl: null);
        UserEntity user = UserFactory.Create(payload.Email);

        _authRepositoryMock
            .Setup(x =>
                x.GetOrCreateExternalUserAsync(
                    It.IsAny<string>(),
                    payload.Name,
                    It.IsAny<AuthProvider>(),
                    It.IsAny<string>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(user);

        // Act
        await _factory.AuthenticateOrCreateAsync(payload, Provider, CancellationToken.None);

        // Assert
    }

    [Fact]
    public async Task AuthenticateOrCreateAsync_ShouldGetOrCreateExternalUser_WithSubjectId()
    {
        // Arrange
        SocialTokenPayload payload = Payload("user@example.com", "socialuser", pictureUrl: null);
        UserEntity user = UserFactory.Create(payload.Email);

        _authRepositoryMock
            .Setup(x =>
                x.GetOrCreateExternalUserAsync(
                    It.Is<string>(e => e == payload.Email),
                    payload.Name,
                    It.Is<AuthProvider>(p => ((EnumAuthProvider)p) == Provider),
                    payload.ProviderSubjectId,
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(user);

        _avatarServiceMock
            .Setup(x => x.UploadFromUrlAsync(It.IsAny<Guid?>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((StoredFile?)null);

        // Act
        await _factory.AuthenticateOrCreateAsync(payload, Provider, CancellationToken.None);

        // Assert
        _authRepositoryMock.Verify(
            x =>
                x.GetOrCreateExternalUserAsync(
                    It.Is<string>(e => e == payload.Email),
                    payload.Name,
                    It.Is<AuthProvider>(p => ((EnumAuthProvider)p) == Provider),
                    payload.ProviderSubjectId,
                    It.IsAny<CancellationToken>()
                ),
            Times.Once
        );
    }

    [Fact]
    public async Task AuthenticateOrCreateAsync_ShouldUpdateAvatarFromPicture()
    {
        // Arrange
        SocialTokenPayload payload = Payload("user@example.com", "socialuser", "https://avatar.url/image.jpg");
        UserEntity user = UserFactory.Create(payload.Email);

        _authRepositoryMock
            .Setup(x =>
                x.GetOrCreateExternalUserAsync(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<AuthProvider>(),
                    It.IsAny<string>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(user);

        _avatarServiceMock
            .Setup(x => x.UploadFromUrlAsync(user.AvatarFileId, payload.PictureUrl!, It.IsAny<CancellationToken>()))
            .ReturnsAsync((StoredFile?)null);

        // Act
        await _factory.AuthenticateOrCreateAsync(payload, Provider, CancellationToken.None);

        // Assert
    }

    [Fact]
    public async Task AuthenticateOrCreateAsync_WithAManuallyUploadedAvatar_ShouldNotTouchTheProviderPicture()
    {
        // Arrange
        SocialTokenPayload payload = Payload("user@example.com", "socialuser", "https://avatar.url/image.jpg");
        UserEntity user = UserFactory.Create(payload.Email);
        user.UpdateAvatar(Guid.NewGuid(), EnumAvatarSource.Manual);

        _authRepositoryMock
            .Setup(x =>
                x.GetOrCreateExternalUserAsync(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<AuthProvider>(),
                    It.IsAny<string>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(user);

        // Act
        await _factory.AuthenticateOrCreateAsync(payload, Provider, CancellationToken.None);

        // Assert

        user.AvatarSource.Should().Be(EnumAvatarSource.Manual);
    }

    [Fact]
    public async Task AuthenticateOrCreateAsync_WithAvatarFileReturned_ShouldUpdateUserAvatar()
    {
        // Arrange
        SocialTokenPayload payload = Payload("user@example.com", "socialuser", "https://avatar.url/image.jpg");
        var avatarFileId = Guid.NewGuid();
        UserEntity user = UserFactory.Create(payload.Email);
        FileReferenceDto avatarFile = FileReferenceDtoFactory.CreateWithId(avatarFileId);

        _authRepositoryMock
            .Setup(x =>
                x.GetOrCreateExternalUserAsync(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<AuthProvider>(),
                    It.IsAny<string>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(user);

        _avatarServiceMock
            .Setup(x => x.UploadFromUrlAsync(user.AvatarFileId, payload.PictureUrl!, It.IsAny<CancellationToken>()))
            .ReturnsAsync(StoredFileFactory.From(avatarFile));

        // Act
        await _factory.AuthenticateOrCreateAsync(payload, Provider, CancellationToken.None);

        // Assert
        user.AvatarFileId.Should().Be(avatarFileId);
        user.AvatarSource.Should().Be(EnumAvatarSource.Provider);
    }

    [Fact]
    public async Task AuthenticateOrCreateAsync_ShouldCommitTransaction()
    {
        // Arrange
        SocialTokenPayload payload = Payload("user@example.com", "socialuser", pictureUrl: null);
        UserEntity user = UserFactory.Create(payload.Email);

        _authRepositoryMock
            .Setup(x =>
                x.GetOrCreateExternalUserAsync(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<AuthProvider>(),
                    It.IsAny<string>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(user);

        _avatarServiceMock
            .Setup(x => x.UploadFromUrlAsync(It.IsAny<Guid?>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((StoredFile?)null);

        // Act
        await _factory.AuthenticateOrCreateAsync(payload, Provider, CancellationToken.None);

        // Assert
        _unitOfWorkMock.VerifyExecutedInTransaction();
    }

    [Fact]
    public async Task AuthenticateOrCreateAsync_WithCancellationToken_ShouldPassToRepositories()
    {
        // Arrange
        SocialTokenPayload payload = Payload("user@example.com", "socialuser", "https://avatar.url/image.jpg");
        UserEntity user = UserFactory.Create(payload.Email);
        CancellationToken cancellationToken = new();

        _authRepositoryMock
            .Setup(x =>
                x.GetOrCreateExternalUserAsync(
                    It.IsAny<string>(),
                    payload.Name,
                    It.IsAny<AuthProvider>(),
                    It.IsAny<string>(),
                    cancellationToken
                )
            )
            .ReturnsAsync(user);

        _avatarServiceMock
            .Setup(x => x.UploadFromUrlAsync(user.AvatarFileId, payload.PictureUrl!, cancellationToken))
            .ReturnsAsync((StoredFile?)null);

        // Act
        await _factory.AuthenticateOrCreateAsync(payload, Provider, cancellationToken);

        // Assert
        _authRepositoryMock.Verify(
            x =>
                x.GetOrCreateExternalUserAsync(
                    It.IsAny<string>(),
                    payload.Name,
                    It.IsAny<AuthProvider>(),
                    It.IsAny<string>(),
                    cancellationToken
                ),
            Times.Once
        );
        _unitOfWorkMock.VerifyExecutedInTransaction();
    }

    #endregion

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task AuthenticateOrCreateAsync_WithABlankPictureUrl_ShouldNotTouchTheAvatar(string pictureUrl)
    {
        // Arrange
        SocialTokenPayload payload = Payload("user@example.com", "socialuser", pictureUrl);
        UserEntity user = UserFactory.Create(payload.Email);

        _authRepositoryMock
            .Setup(x =>
                x.GetOrCreateExternalUserAsync(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<AuthProvider>(),
                    It.IsAny<string>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(user);

        // Act
        await _factory.AuthenticateOrCreateAsync(payload, Provider, CancellationToken.None);

        // Assert
    }
}
