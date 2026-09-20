using _116.Content.Application.Editorial.Factories;
using _116.Content.Application.Editorial.UseCases.Public.Queries.GetVideoBySlug;
using _116.Content.Application.Shared.Errors;
using _116.Content.Application.Shared.Errors.Facade;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Core.Application.Shared.Repositories;
using _116.Core.Contracts.Application.Services;
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

namespace _116.Unit.Tests.Modules.Content.Application.Editorial.UseCases.Public.Queries.GetVideoBySlug;

/// <summary>
/// Unit tests for <see cref="PublicGetVideoBySlugHandler"/>.
/// </summary>
public class PublicGetVideoBySlugHandlerTests : BaseContentHandlerTest
{
    private readonly Mock<IVideoRepository> _videoRepositoryMock;
    private readonly Mock<IFileStorageService> _fileStorageMock;
    private readonly Mock<IArtistRepository> _artistRepositoryMock;
    private readonly PublicGetVideoBySlugHandler _handler;
    private readonly VideoErrors _videoErrors = TestErrorsFactory.CreateVideoErrors();
    private readonly ContentI18n _i18n = TestErrorsFactory.CreateContentI18n();

    private static readonly Guid CategoryId = Guid.NewGuid();

    public PublicGetVideoBySlugHandlerTests()
    {
        _videoRepositoryMock = MockVideoRepository.Create();
        _fileStorageMock = MockFileStorageService.Create();
        _artistRepositoryMock = MockArtistRepository.Create();
        _handler = new PublicGetVideoBySlugHandler(
            _videoRepositoryMock.Object,
            _artistRepositoryMock.Object,
            new VideoDtoFactory(
                Mapper,
                _fileStorageMock.Object,
                _videoRepositoryMock.Object,
                CreateContentLookupFactory()
            ),
            _i18n
        );
    }

    [Fact]
    public async Task Handle_WhenPublishedVideoExists_ShouldReturnVideoDetail()
    {
        // Arrange
        CategoryEntity category = CategoryFactory.Create(CategoryId);
        VideoEntity video = VideoFactory.CreateWithCategory(CategoryId, category);
        video.AttachYoutubeVideoUrl(TestConstants.Video.ValidYoutubeVideoUrl, TestConstants.Clock.Instant);
        video.MarkPendingReview();
        video.Approve();
        video.Publish(TestConstants.Clock.Instant);

        string slug = video.Slug;
        var query = new PublicGetVideoBySlugQuery(Slug: slug);

        _videoRepositoryMock.SetupGetBySlug(slug, video);

        // Act
        PublicGetVideoBySlugResult result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Video.Id.Should().Be(video.Id);
        result.Video.Slug.Should().Be(slug);
    }

    [Fact]
    public async Task Handle_WhenVideoHasNoLinkedArtist_ShouldReturnNullArtistSlug()
    {
        // Arrange
        CategoryEntity category = CategoryFactory.Create(CategoryId);
        VideoEntity video = VideoFactory.CreateWithCategory(CategoryId, category);
        video.AttachYoutubeVideoUrl(TestConstants.Video.ValidYoutubeVideoUrl, TestConstants.Clock.Instant);
        video.MarkPendingReview();
        video.Approve();
        video.Publish(TestConstants.Clock.Instant);

        _videoRepositoryMock.SetupGetBySlug(video.Slug, video);

        // Act
        PublicGetVideoBySlugResult result = await _handler.Handle(
            new PublicGetVideoBySlugQuery(Slug: video.Slug),
            CancellationToken.None
        );

        // Assert
        result.ArtistSlug.Should().BeNull();
    }

    [Fact]
    public async Task Handle_WhenVideoHasLinkedArtist_ShouldResolveArtistSlug()
    {
        // Arrange
        CategoryEntity category = CategoryFactory.Create(CategoryId);
        VideoEntity video = VideoFactory.CreateWithCategory(CategoryId, category);
        video.AttachYoutubeVideoUrl(TestConstants.Video.ValidYoutubeVideoUrl, TestConstants.Clock.Instant);
        video.MarkPendingReview();
        video.Approve();
        video.Publish(TestConstants.Clock.Instant);

        ArtistEntity artist = ArtistFactory.CreateWithSlug($"linked-{Guid.NewGuid():N}");
        video.LinkArtist(artist.Id);

        _videoRepositoryMock.SetupGetBySlug(video.Slug, video);
        _artistRepositoryMock.SetupGetByIdAsync(artist.Id, artist);

        // Act
        PublicGetVideoBySlugResult result = await _handler.Handle(
            new PublicGetVideoBySlugQuery(Slug: video.Slug),
            CancellationToken.None
        );

        // Assert
        result.ArtistSlug.Should().Be(artist.Slug);
    }

    [Fact]
    public async Task Handle_WhenVideoNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        string slug = TestConstants.Video.ValidSlug;
        var query = new PublicGetVideoBySlugQuery(Slug: slug);

        _videoRepositoryMock.SetupGetBySlug(slug, null);

        // Act
        Func<Task> act = async () => await _handler.Handle(query, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_WhenVideoExistsButNotPublished_ShouldThrowNotFoundException()
    {
        // Arrange
        VideoEntity draftVideo = VideoFactory.Create(CategoryId);
        string slug = draftVideo.Slug;
        var query = new PublicGetVideoBySlugQuery(Slug: slug);

        _videoRepositoryMock.SetupGetBySlug(slug, draftVideo);

        // Act
        Func<Task> act = async () => await _handler.Handle(query, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }
}
