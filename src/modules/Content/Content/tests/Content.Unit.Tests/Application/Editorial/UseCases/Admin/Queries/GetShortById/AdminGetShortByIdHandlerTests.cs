using _116.BuildingBlocks.Application.Exceptions;
using _116.Content.Application.Editorial.UseCases.Admin.Queries.GetShortById;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Content.TestData;
using _116.Content.TestData.Factories;
using _116.Content.TestData.Mocks.Repositories;
using _116.Content.TestData.Mocks.Services;
using _116.Identity.Contracts.Application.DTOs;
using _116.Identity.Contracts.Application.Services;
using _116.Identity.TestData.Mocks.Repositories;
using _116.Identity.TestData.Mocks.Services;
using _116.Shared.Domain.Constants;
using _116.Storage.Contracts.Application.DTOs;
using _116.Storage.Contracts.Application.Services;
using _116.Storage.TestData.Factories;
using _116.Tests.TestData.Constants;
using AwesomeAssertions;
using Moq;
using Xunit;

namespace _116.Content.Unit.Tests.Application.Editorial.UseCases.Admin.Queries.GetShortById;

/// <summary>
/// Unit tests for <see cref="AdminGetShortByIdHandler"/>.
/// </summary>
public class AdminGetShortByIdHandlerTests : BaseContentHandlerTest
{
    private readonly Mock<IShortVideoRepository> _shortVideoRepositoryMock;
    private readonly Mock<IUserLookupService> _userLookupMock;
    private readonly Mock<IFileStorageService> _fileStorageMock;
    private readonly AdminGetShortByIdHandler _handler;

    public AdminGetShortByIdHandlerTests()
    {
        _shortVideoRepositoryMock = MockShortVideoRepository.Create();
        _userLookupMock = MockUserLookupService.Create();
        _fileStorageMock = MockFileStorageService.Create();
        _handler = new AdminGetShortByIdHandler(
            _shortVideoRepositoryMock.Object,
            CreateShortVideoDtoService(_fileStorageMock.Object, _userLookupMock.Object)
        );
    }

    #region Success Cases

    [Fact]
    public async Task Handle_WhenShortVideoExists_ShouldReturnShortVideoDetail()
    {
        // Arrange
        ShortVideoEntity shortVideo = ShortVideoFactory.Create();
        var query = new AdminGetShortByIdQuery(Id: shortVideo.Id);
        _shortVideoRepositoryMock.SetupGetByIdOrThrow(shortVideo);

        // Act
        AdminGetShortByIdResult result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.ShortVideo.Id.Should().Be(shortVideo.Id);
    }

    [Fact]
    public async Task Handle_WhenAuthorExists_ShouldResolveAuthorProfile()
    {
        // Arrange
        ShortVideoEntity shortVideo = ShortVideoFactory.Create();
        var query = new AdminGetShortByIdQuery(Id: shortVideo.Id);
        _shortVideoRepositoryMock.SetupGetByIdOrThrow(shortVideo);

        var authorInfo = new UserProfileDto(
            TestConstants.User.ValidUserName,
            TestConstants.User.ValidEmail,
            null,
            "SuperAdmin",
            LocaleConstants.DefaultLocale
        );
        _userLookupMock
            .Setup(x => x.GetUserProfileByIdAsync(shortVideo.AuthorId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(authorInfo);

        // Act
        AdminGetShortByIdResult result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.ShortVideo.Author.Should().NotBeNull();
        result.ShortVideo.Author!.UserName.Should().Be(TestConstants.User.ValidUserName);
        result.ShortVideo.Author.Email.Should().Be(TestConstants.User.ValidEmail);
        result.ShortVideo.Author.Role.Should().Be("SuperAdmin");
        result.ShortVideo.Author.AvatarUrl.Should().BeNull();
    }

    [Fact]
    public async Task Handle_WhenAuthorHasAvatar_ShouldResolveAvatarUrl()
    {
        // Arrange
        ShortVideoEntity shortVideo = ShortVideoFactory.Create();
        var query = new AdminGetShortByIdQuery(Id: shortVideo.Id);
        _shortVideoRepositoryMock.SetupGetByIdOrThrow(shortVideo);

        Guid avatarFileId = Guid.NewGuid();
        var authorInfo = new UserProfileDto(
            TestConstants.User.ValidUserName,
            TestConstants.User.ValidEmail,
            avatarFileId,
            "Admin",
            LocaleConstants.DefaultLocale
        );
        _userLookupMock
            .Setup(x => x.GetUserProfileByIdAsync(shortVideo.AuthorId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(authorInfo);

        FileReferenceDto avatarFile = FileReferenceDtoFactory.CreateWithId(avatarFileId);
        _fileStorageMock.SetupResolve(avatarFile);

        // Act
        AdminGetShortByIdResult result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.ShortVideo.Author.Should().NotBeNull();
        result.ShortVideo.Author!.AvatarUrl.Should().Be(avatarFile.StorageUrl);
        result.ShortVideo.Author.Role.Should().Be("Admin");
    }

    [Fact]
    public async Task Handle_WhenAuthorNotFound_ShouldReturnNullAuthor()
    {
        // Arrange
        ShortVideoEntity shortVideo = ShortVideoFactory.Create();
        var query = new AdminGetShortByIdQuery(Id: shortVideo.Id);
        _shortVideoRepositoryMock.SetupGetByIdOrThrow(shortVideo);

        _userLookupMock
            .Setup(x => x.GetUserProfileByIdAsync(shortVideo.AuthorId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((UserProfileDto?)null);

        // Act
        AdminGetShortByIdResult result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.ShortVideo.Author.Should().BeNull();
        _fileStorageMock.Verify(
            x => x.ResolveAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
            Times.Once,
            "GetByIdAsync should be called once for the video file URL resolution"
        );
    }

    [Fact]
    public async Task Handle_WhenAuthorHasNoAvatar_ShouldNotCallFileRepository()
    {
        // Arrange
        ShortVideoEntity shortVideo = ShortVideoFactory.Create();
        var query = new AdminGetShortByIdQuery(Id: shortVideo.Id);
        _shortVideoRepositoryMock.SetupGetByIdOrThrow(shortVideo);

        var authorInfo = new UserProfileDto(
            TestConstants.User.ValidUserName,
            TestConstants.User.ValidEmail,
            null,
            "Admin",
            LocaleConstants.DefaultLocale
        );
        _userLookupMock
            .Setup(x => x.GetUserProfileByIdAsync(shortVideo.AuthorId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(authorInfo);

        // Act
        AdminGetShortByIdResult result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.ShortVideo.Author.Should().NotBeNull();
        result.ShortVideo.Author!.AvatarUrl.Should().BeNull();
        _fileStorageMock.Verify(
            x => x.ResolveAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
            Times.Once,
            "GetByIdAsync should be called once for the video file URL, not for the avatar"
        );
    }

    #endregion

    #region Failure Cases

    [Fact]
    public async Task Handle_WhenShortVideoNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        Guid nonExistentId = Guid.NewGuid();
        var query = new AdminGetShortByIdQuery(Id: nonExistentId);
        _shortVideoRepositoryMock.SetupGetByIdOrThrowNotFound(nonExistentId);

        // Act
        Func<Task> act = async () => await _handler.Handle(query, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    #endregion
}
