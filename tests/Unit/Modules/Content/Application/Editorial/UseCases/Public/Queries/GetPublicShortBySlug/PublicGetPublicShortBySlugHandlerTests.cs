using _116.Content.Application.Editorial.UseCases.Public.Queries.GetPublicShortBySlug;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Core.Application.Shared.Repositories;
using _116.Core.Domain.Entities;
using _116.Identity.Contracts.Application;
using _116.Shared.Application.Exceptions;
using _116.Tests.Fixtures.Constants;
using _116.Tests.Fixtures.Factories.Content;
using _116.Tests.Fixtures.Factories.Core;
using _116.Tests.Fixtures.Helpers;
using _116.Unit.Tests.Common;
using _116.Unit.Tests.Common.Mocks.Repositories;
using _116.Unit.Tests.Common.Mocks.Services;
using AwesomeAssertions;
using Moq;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Editorial.UseCases.Public.Queries.GetPublicShortBySlug;

/// <summary>
/// Unit tests for <see cref="PublicGetPublicShortBySlugHandler"/>.
/// </summary>
public class PublicGetPublicShortBySlugHandlerTests : BaseContentHandlerTest
{
    private readonly Mock<IShortVideoRepository> _shortVideoRepositoryMock;
    private readonly Mock<IUserLookupService> _userLookupMock;
    private readonly Mock<IFileRepository> _fileRepositoryMock;
    private readonly PublicGetPublicShortBySlugHandler _handler;

    public PublicGetPublicShortBySlugHandlerTests()
    {
        _shortVideoRepositoryMock = MockShortVideoRepository.Create();
        _userLookupMock = MockUserLookupService.Create();
        _fileRepositoryMock = MockFileRepository.Create();

        FileEntity videoFile = FileFactory.CreateVideo();
        _fileRepositoryMock.SetupGetById(videoFile);

        _handler = new PublicGetPublicShortBySlugHandler(
            _shortVideoRepositoryMock.Object,
            _userLookupMock.Object,
            _fileRepositoryMock.Object,
            Mapper,
            TestErrorsFactory.CreateContentI18n()
        );
    }

    [Fact]
    public async Task Handle_WhenActiveShortVideoExists_ShouldReturnShortVideo()
    {
        // Arrange
        ShortVideoEntity shortVideo = ShortVideoFactory.Create(); // active by default
        string slug = shortVideo.Slug;
        var query = new PublicGetPublicShortBySlugQuery(Slug: slug);

        _shortVideoRepositoryMock.SetupGetBySlug(slug, shortVideo);

        // Act
        PublicGetPublicShortBySlugResult result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.ShortVideo.Should().NotBeNull();
    }

    [Fact]
    public async Task Handle_WhenShortVideoNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        string slug = TestConstants.ShortVideo.ValidSlug;
        var query = new PublicGetPublicShortBySlugQuery(Slug: slug);

        _shortVideoRepositoryMock.SetupGetBySlug(slug, null);

        // Act
        Func<Task> act = async () => await _handler.Handle(query, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_WhenShortVideoExistsButInactive_ShouldThrowNotFoundException()
    {
        // Arrange
        ShortVideoEntity inactiveShortVideo = ShortVideoFactory.CreateInactive();
        string slug = inactiveShortVideo.Slug;
        var query = new PublicGetPublicShortBySlugQuery(Slug: slug);

        _shortVideoRepositoryMock.SetupGetBySlug(slug, inactiveShortVideo);

        // Act
        Func<Task> act = async () => await _handler.Handle(query, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_WhenUserLikedAndBookmarked_ShouldStampFlags()
    {
        // Arrange
        ShortVideoEntity shortVideo = ShortVideoFactory.Create();
        var userId = Guid.NewGuid();
        var query = new PublicGetPublicShortBySlugQuery(Slug: shortVideo.Slug, CurrentUserId: userId);

        _shortVideoRepositoryMock.SetupGetBySlug(shortVideo.Slug, shortVideo);
        _shortVideoRepositoryMock.SetupHasLikedAsync(true);
        _shortVideoRepositoryMock.SetupHasBookmarkedAsync(true);

        // Act
        PublicGetPublicShortBySlugResult result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.ShortVideo.IsLiked.Should().BeTrue();
        result.ShortVideo.IsBookmarked.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_WhenAnonymous_ShouldNotResolveFlagsAndReturnFalse()
    {
        // Arrange
        ShortVideoEntity shortVideo = ShortVideoFactory.Create();
        var query = new PublicGetPublicShortBySlugQuery(Slug: shortVideo.Slug);

        _shortVideoRepositoryMock.SetupGetBySlug(shortVideo.Slug, shortVideo);

        // Act
        PublicGetPublicShortBySlugResult result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.ShortVideo.IsLiked.Should().BeFalse();
        result.ShortVideo.IsBookmarked.Should().BeFalse();
        _shortVideoRepositoryMock.Verify(
            x => x.HasLikedAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
            Times.Never
        );
        _shortVideoRepositoryMock.Verify(
            x => x.HasBookmarkedAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
            Times.Never
        );
    }
}
