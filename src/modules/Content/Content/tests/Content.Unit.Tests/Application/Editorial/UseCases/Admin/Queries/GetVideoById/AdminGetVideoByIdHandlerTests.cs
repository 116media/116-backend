using _116.BuildingBlocks.Application.Exceptions;
using _116.Content.Application.Editorial.Services;
using _116.Content.Application.Editorial.UseCases.Admin.Queries.GetVideoById;
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

namespace _116.Content.Unit.Tests.Application.Editorial.UseCases.Admin.Queries.GetVideoById;

/// <summary>
/// Unit tests for <see cref="AdminGetVideoByIdHandler"/>.
/// </summary>
public class AdminGetVideoByIdHandlerTests : BaseContentHandlerTest
{
    private readonly Mock<IVideoRepository> _videoRepositoryMock;
    private readonly Mock<IUserLookupService> _userLookupMock;
    private readonly Mock<IFileStorageService> _fileStorageMock;
    private readonly AdminGetVideoByIdHandler _handler;

    private static readonly Guid CategoryId = Guid.NewGuid();

    public AdminGetVideoByIdHandlerTests()
    {
        _videoRepositoryMock = MockVideoRepository.Create();
        _userLookupMock = MockUserLookupService.Create();
        _fileStorageMock = MockFileStorageService.Create();
        _handler = new AdminGetVideoByIdHandler(
            _videoRepositoryMock.Object,
            _userLookupMock.Object,
            new VideoDtoService(
                Mapper,
                _fileStorageMock.Object,
                _videoRepositoryMock.Object,
                CreateContentLookupService()
            ),
            _fileStorageMock.Object
        );
    }

    #region Success Cases

    [Fact]
    public async Task Handle_WhenVideoExists_ShouldReturnVideoDetail()
    {
        // Arrange
        CategoryEntity category = CategoryFactory.Create(CategoryId);
        VideoEntity video = VideoFactory.CreateWithCategory(category);
        var query = new AdminGetVideoByIdQuery(Id: video.Id);
        _videoRepositoryMock.SetupGetByIdOrThrow(video);

        // Act
        AdminGetVideoByIdResult result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Video.Id.Should().Be(video.Id);
    }

    [Fact]
    public async Task Handle_WhenAuthorExists_ShouldResolveAuthorProfile()
    {
        // Arrange
        CategoryEntity category = CategoryFactory.Create(CategoryId);
        VideoEntity video = VideoFactory.CreateWithCategory(category);
        var query = new AdminGetVideoByIdQuery(Id: video.Id);
        _videoRepositoryMock.SetupGetByIdOrThrow(video);

        var authorInfo = new UserProfileDto(
            TestConstants.User.ValidUserName,
            TestConstants.User.ValidEmail,
            null,
            "SuperAdmin",
            LocaleConstants.DefaultLocale
        );
        _userLookupMock
            .Setup(x => x.GetUserProfileByIdAsync(video.AuthorId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(authorInfo);

        // Act
        AdminGetVideoByIdResult result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Video.Author.Should().NotBeNull();
        result.Video.Author!.UserName.Should().Be(TestConstants.User.ValidUserName);
        result.Video.Author.Email.Should().Be(TestConstants.User.ValidEmail);
        result.Video.Author.Role.Should().Be("SuperAdmin");
        result.Video.Author.AvatarUrl.Should().BeNull();
    }

    [Fact]
    public async Task Handle_WhenAuthorHasAvatar_ShouldResolveAvatarUrl()
    {
        // Arrange
        CategoryEntity category = CategoryFactory.Create(CategoryId);
        VideoEntity video = VideoFactory.CreateWithCategory(category);
        var query = new AdminGetVideoByIdQuery(Id: video.Id);
        _videoRepositoryMock.SetupGetByIdOrThrow(video);

        Guid avatarFileId = Guid.NewGuid();
        var authorInfo = new UserProfileDto(
            TestConstants.User.ValidUserName,
            TestConstants.User.ValidEmail,
            avatarFileId,
            "Admin",
            LocaleConstants.DefaultLocale
        );
        _userLookupMock
            .Setup(x => x.GetUserProfileByIdAsync(video.AuthorId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(authorInfo);

        FileReferenceDto avatarFile = FileReferenceDtoFactory.CreateWithId(avatarFileId);
        _fileStorageMock.SetupResolve(avatarFile);

        // Act
        AdminGetVideoByIdResult result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Video.Author.Should().NotBeNull();
        result.Video.Author!.AvatarUrl.Should().Be(avatarFile.StorageUrl);
    }

    [Fact]
    public async Task Handle_WhenAuthorNotFound_ShouldReturnNullAuthor()
    {
        // Arrange
        CategoryEntity category = CategoryFactory.Create(CategoryId);
        VideoEntity video = VideoFactory.CreateWithCategory(category);
        var query = new AdminGetVideoByIdQuery(Id: video.Id);
        _videoRepositoryMock.SetupGetByIdOrThrow(video);

        _userLookupMock
            .Setup(x => x.GetUserProfileByIdAsync(video.AuthorId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((UserProfileDto?)null);

        // Act
        AdminGetVideoByIdResult result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Video.Author.Should().BeNull();
        _fileStorageMock.Verify(x => x.ResolveAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenAuthorHasNoAvatar_ShouldNotCallFileRepository()
    {
        // Arrange
        CategoryEntity category = CategoryFactory.Create(CategoryId);
        VideoEntity video = VideoFactory.CreateWithCategory(category);
        var query = new AdminGetVideoByIdQuery(Id: video.Id);
        _videoRepositoryMock.SetupGetByIdOrThrow(video);

        var authorInfo = new UserProfileDto(
            TestConstants.User.ValidUserName,
            TestConstants.User.ValidEmail,
            null,
            "Admin",
            LocaleConstants.DefaultLocale
        );
        _userLookupMock
            .Setup(x => x.GetUserProfileByIdAsync(video.AuthorId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(authorInfo);

        // Act
        AdminGetVideoByIdResult result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Video.Author.Should().NotBeNull();
        result.Video.Author!.AvatarUrl.Should().BeNull();
        _fileStorageMock.Verify(x => x.ResolveAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    #endregion

    #region Failure Cases

    [Fact]
    public async Task Handle_WhenVideoNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        Guid nonExistentId = Guid.NewGuid();
        var query = new AdminGetVideoByIdQuery(Id: nonExistentId);
        _videoRepositoryMock.SetupGetByIdOrThrowNotFound(nonExistentId);

        // Act
        Func<Task> act = async () => await _handler.Handle(query, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    #endregion
}
