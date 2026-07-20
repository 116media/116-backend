using _116.Content.Application.Interactions.UseCases.Public.Queries.GetOwnSharedVideos;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;
using _116.Core.Application.Shared.Repositories;
using _116.Shared.Application.Pagination;
using _116.Tests.Fixtures.Factories.Content;
using _116.Unit.Tests.Common;
using _116.Unit.Tests.Common.Mocks.Repositories;
using AwesomeAssertions;
using Moq;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Interactions.UseCases.Public.Queries.GetOwnSharedVideos;

/// <summary>
/// Unit tests for <see cref="PublicGetOwnSharedVideosHandler" />.
/// </summary>
public class PublicGetOwnSharedVideosHandlerTests : BaseContentHandlerTest
{
    private readonly Mock<IVideoRepository> _videoRepository = MockVideoRepository.Create();
    private readonly Mock<IFileRepository> _fileRepository = MockFileRepository.Create();

    [Fact]
    public async Task Handle_ReturnsOwnShareCountLatestChannelAndPaginationMetadata()
    {
        Guid userId = Guid.NewGuid();
        VideoEntity video = VideoFactory.CreatePublished(Guid.NewGuid());
        DateTimeOffset interactedAt = DateTimeOffset.UtcNow.AddMinutes(-3);
        IReadOnlyList<SharedVideoActivity> activities = [new(video, 3, interactedAt, EnumShareChannel.WhatsApp)];
        _videoRepository
            .Setup(repository => repository.GetSharedVideosByUserAsync(userId, 1, 12, It.IsAny<CancellationToken>()))
            .ReturnsAsync((activities, 1));
        var handler = new PublicGetOwnSharedVideosHandler(_videoRepository.Object, _fileRepository.Object, Mapper);

        PublicGetOwnSharedVideosResult result = await handler.Handle(
            new PublicGetOwnSharedVideosQuery(userId, new PaginatedRequest(0, 12)),
            CancellationToken.None
        );

        var item = result.Videos.Items.Should().ContainSingle().Subject;
        item.Video.Id.Should().Be(video.Id);
        item.InteractionCount.Should().Be(3);
        item.LastShareChannel.Should().Be(EnumShareChannel.WhatsApp);
        item.LastInteractedAt.Should().Be(interactedAt);
        item.RatedStars.Should().BeNull();
    }

    [Fact]
    public async Task Handle_EmptyRepositoryResult_ReturnsEmptyPage()
    {
        Guid userId = Guid.NewGuid();
        _videoRepository
            .Setup(repository => repository.GetSharedVideosByUserAsync(userId, 1, 10, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Array.Empty<SharedVideoActivity>(), 0));
        var handler = new PublicGetOwnSharedVideosHandler(_videoRepository.Object, _fileRepository.Object, Mapper);

        PublicGetOwnSharedVideosResult result = await handler.Handle(
            new PublicGetOwnSharedVideosQuery(userId, new PaginatedRequest(0, 10)),
            CancellationToken.None
        );

        result.Videos.Items.Should().BeEmpty();
        result.Videos.Count.Should().Be(0);
    }
}
