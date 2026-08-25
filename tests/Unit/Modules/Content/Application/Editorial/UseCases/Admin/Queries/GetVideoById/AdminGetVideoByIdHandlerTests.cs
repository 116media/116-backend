using _116.Content.Application.Editorial.UseCases.Admin.Queries.GetVideoById;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Core.Application.Shared.Repositories;
using _116.Core.Domain.Entities;
using _116.Identity.Contracts.Application;
using _116.Shared.Application.Exceptions;
using _116.Tests.Fixtures.Constants;
using _116.Tests.Fixtures.Factories.Content;
using _116.Tests.Fixtures.Factories.Core;
using _116.Unit.Tests.Common;
using _116.Unit.Tests.Common.Mocks.Repositories;
using _116.Unit.Tests.Common.Mocks.Services;
using AwesomeAssertions;
using Moq;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Editorial.UseCases.Admin.Queries.GetVideoById;

/// <summary>
/// Unit tests for <see cref="AdminGetVideoByIdHandler"/>.
/// </summary>
public class AdminGetVideoByIdHandlerTests : BaseContentHandlerTest
{
    private readonly Mock<IVideoRepository> _videoRepositoryMock;
    private readonly Mock<IUserLookupService> _userLookupMock;
    private readonly Mock<IFileRepository> _fileRepositoryMock;
    private readonly AdminGetVideoByIdHandler _handler;

    private static readonly Guid CategoryId = Guid.NewGuid();

    public AdminGetVideoByIdHandlerTests()
    {
        _videoRepositoryMock = MockVideoRepository.Create();
        _userLookupMock = MockUserLookupService.Create();
        _fileRepositoryMock = MockFileRepository.Create();
        _handler = new AdminGetVideoByIdHandler(
            _videoRepositoryMock.Object,
            _userLookupMock.Object,
            _fileRepositoryMock.Object,
            Mapper
        );
    }

    #region Success Cases

    [Fact]
    public async Task Handle_WhenVideoExists_ShouldReturnVideoDetail()
    {
        // Arrange
        CategoryEntity category = CategoryFactory.Create(CategoryId);
        VideoEntity video = VideoFactory.CreateWithCategory(CategoryId, category);
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
        VideoEntity video = VideoFactory.CreateWithCategory(CategoryId, category);
        var query = new AdminGetVideoByIdQuery(Id: video.Id);
        _videoRepositoryMock.SetupGetByIdOrThrow(video);

        var authorInfo = new AuthorInfo(
            TestConstants.User.ValidUserName,
            TestConstants.User.ValidEmail,
            null,
            "SuperAdmin"
        );
        _userLookupMock
            .Setup(x => x.GetAuthorInfoByIdAsync(video.AuthorId, It.IsAny<CancellationToken>()))
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
        VideoEntity video = VideoFactory.CreateWithCategory(CategoryId, category);
        var query = new AdminGetVideoByIdQuery(Id: video.Id);
        _videoRepositoryMock.SetupGetByIdOrThrow(video);

        Guid avatarFileId = Guid.NewGuid();
        var authorInfo = new AuthorInfo(
            TestConstants.User.ValidUserName,
            TestConstants.User.ValidEmail,
            avatarFileId,
            "Admin"
        );
        _userLookupMock
            .Setup(x => x.GetAuthorInfoByIdAsync(video.AuthorId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(authorInfo);

        FileEntity avatarFile = FileFactory.CreateWithId(avatarFileId);
        _fileRepositoryMock.SetupGetById(avatarFile);

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
        VideoEntity video = VideoFactory.CreateWithCategory(CategoryId, category);
        var query = new AdminGetVideoByIdQuery(Id: video.Id);
        _videoRepositoryMock.SetupGetByIdOrThrow(video);

        _userLookupMock
            .Setup(x => x.GetAuthorInfoByIdAsync(video.AuthorId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((AuthorInfo?)null);

        // Act
        AdminGetVideoByIdResult result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Video.Author.Should().BeNull();
        _fileRepositoryMock.Verify(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenAuthorHasNoAvatar_ShouldNotCallFileRepository()
    {
        // Arrange
        CategoryEntity category = CategoryFactory.Create(CategoryId);
        VideoEntity video = VideoFactory.CreateWithCategory(CategoryId, category);
        var query = new AdminGetVideoByIdQuery(Id: video.Id);
        _videoRepositoryMock.SetupGetByIdOrThrow(video);

        var authorInfo = new AuthorInfo(TestConstants.User.ValidUserName, TestConstants.User.ValidEmail, null, "Admin");
        _userLookupMock
            .Setup(x => x.GetAuthorInfoByIdAsync(video.AuthorId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(authorInfo);

        // Act
        AdminGetVideoByIdResult result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Video.Author.Should().NotBeNull();
        result.Video.Author!.AvatarUrl.Should().BeNull();
        _fileRepositoryMock.Verify(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
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
