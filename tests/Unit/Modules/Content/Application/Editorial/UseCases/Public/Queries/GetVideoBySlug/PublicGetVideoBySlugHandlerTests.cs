using _116.Content.Application.Editorial.UseCases.Public.Queries.GetVideoBySlug;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Shared.Application.Exceptions;
using _116.Tests.Fixtures.Constants;
using _116.Tests.Fixtures.Factories.Content;
using _116.Unit.Tests.Common;
using _116.Unit.Tests.Common.Mocks.Repositories;
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
    private readonly PublicGetVideoBySlugHandler _handler;

    private static readonly Guid CategoryId = Guid.NewGuid();

    public PublicGetVideoBySlugHandlerTests()
    {
        _videoRepositoryMock = MockVideoRepository.Create();
        _handler = new PublicGetVideoBySlugHandler(_videoRepositoryMock.Object, Mapper);
    }

    [Fact]
    public async Task Handle_WhenPublishedVideoExists_ShouldReturnVideoDetail()
    {
        // Arrange
        CategoryEntity category = CategoryFactory.Create(CategoryId);
        VideoEntity video = VideoFactory.CreateWithCategory(CategoryId, category);
        // Manually transition to Published for the slug lookup
        video.AttachYoutubeId(TestConstants.Content.Editorial.Video.ValidYoutubeVideoId);
        video.MarkPendingReview();
        video.Approve();
        video.Publish();

        string slug = video.Slug;
        var query = new PublicGetVideoBySlugQuery(Slug: slug);

        _videoRepositoryMock.SetupGetBySlug(slug, video);

        // Act
        PublicGetVideoBySlugResult result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Video.Should().NotBeNull();
    }

    [Fact]
    public async Task Handle_WhenVideoNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        string slug = TestConstants.Content.Editorial.Video.ValidSlug;
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
        VideoEntity draftVideo = VideoFactory.Create(CategoryId); // Draft status
        string slug = draftVideo.Slug;
        var query = new PublicGetVideoBySlugQuery(Slug: slug);

        _videoRepositoryMock.SetupGetBySlug(slug, draftVideo);

        // Act
        Func<Task> act = async () => await _handler.Handle(query, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }
}
