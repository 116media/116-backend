using _116.Content.Application.Interactions.UseCases.Public.Queries.GetOwnRatedVideos;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Core.Application.Shared.Repositories;
using _116.Shared.Application.Pagination;
using _116.Tests.Fixtures.Factories.Content;
using _116.Unit.Tests.Common;
using _116.Unit.Tests.Common.Mocks.Repositories;
using AwesomeAssertions;
using Moq;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Interactions.UseCases.Public.Queries.GetOwnRatedVideos;

/// <summary>
/// Unit tests for <see cref="PublicGetOwnRatedVideosHandler" />.
/// </summary>
public class PublicGetOwnRatedVideosHandlerTests : BaseContentHandlerTest
{
    private readonly Mock<IVideoRepository> _videoRepository = MockVideoRepository.Create();
    private readonly Mock<IFileRepository> _fileRepository = MockFileRepository.Create();

    [Fact]
    public async Task Handle_ReturnsOwnStarsAndPaginationMetadata()
    {
        Guid userId = Guid.NewGuid();
        VideoEntity video = VideoFactory.CreatePublished(Guid.NewGuid());
        DateTimeOffset interactedAt = DateTimeOffset.UtcNow.AddMinutes(-5);
        IReadOnlyList<RatedVideoActivity> activities = [new(video, 4, interactedAt)];
        _videoRepository
            .Setup(repository => repository.GetRatedVideosByUserAsync(userId, 2, 7, It.IsAny<CancellationToken>()))
            .ReturnsAsync((activities, 15));
        var handler = new PublicGetOwnRatedVideosHandler(_videoRepository.Object, _fileRepository.Object, Mapper);

        PublicGetOwnRatedVideosResult result = await handler.Handle(
            new PublicGetOwnRatedVideosQuery(userId, new PaginatedRequest(1, 7)),
            CancellationToken.None
        );

        result.Videos.PageIndex.Should().Be(1);
        result.Videos.PageSize.Should().Be(7);
        result.Videos.Count.Should().Be(15);
        var item = result.Videos.Items.Should().ContainSingle().Subject;
        item.Video.Id.Should().Be(video.Id);
        item.RatedStars.Should().Be(4);
        item.InteractionCount.Should().Be(1);
        item.LastInteractedAt.Should().Be(interactedAt);
        item.LastShareChannel.Should().BeNull();
    }

    [Fact]
    public async Task Handle_EmptyRepositoryResult_ReturnsEmptyPage()
    {
        Guid userId = Guid.NewGuid();
        _videoRepository
            .Setup(repository => repository.GetRatedVideosByUserAsync(userId, 1, 10, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Array.Empty<RatedVideoActivity>(), 0));
        var handler = new PublicGetOwnRatedVideosHandler(_videoRepository.Object, _fileRepository.Object, Mapper);

        PublicGetOwnRatedVideosResult result = await handler.Handle(
            new PublicGetOwnRatedVideosQuery(userId, new PaginatedRequest(0, 10)),
            CancellationToken.None
        );

        result.Videos.Items.Should().BeEmpty();
        result.Videos.Count.Should().Be(0);
    }
}
