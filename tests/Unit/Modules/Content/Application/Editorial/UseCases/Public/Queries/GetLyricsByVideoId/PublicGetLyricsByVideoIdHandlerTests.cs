using _116.Content.Application.Editorial.UseCases.Public.Queries.GetLyricsByVideoId;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Shared.Application.Exceptions;
using _116.Tests.Fixtures.Factories.Content;
using _116.Tests.Fixtures.Helpers;
using _116.Unit.Tests.Common;
using _116.Unit.Tests.Common.Mocks.Repositories;
using AwesomeAssertions;
using Moq;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Editorial.UseCases.Public.Queries.GetLyricsByVideoId;

/// <summary>
/// Unit tests for <see cref="PublicGetLyricsByVideoIdHandler"/>.
/// </summary>
public class PublicGetLyricsByVideoIdHandlerTests : BaseContentHandlerTest
{
    private readonly Mock<ILyricsRepository> _lyricsRepositoryMock;
    private readonly PublicGetLyricsByVideoIdHandler _handler;

    public PublicGetLyricsByVideoIdHandlerTests()
    {
        _lyricsRepositoryMock = MockLyricsRepository.Create();
        _handler = new PublicGetLyricsByVideoIdHandler(
            _lyricsRepositoryMock.Object,
            Mapper,
            TestErrorsFactory.CreateContentI18n()
        );
    }

    [Fact]
    public async Task Handle_WhenLyricsLinkedToVideo_ShouldReturnLyrics()
    {
        // Arrange
        Guid videoId = Guid.NewGuid();
        LyricsEntity lyrics = LyricsFactory.CreateForVideo(videoId);
        var query = new PublicGetLyricsByVideoIdQuery(VideoId: videoId.ToString());

        _lyricsRepositoryMock.SetupGetByVideoIdAsync(videoId, lyrics);

        // Act
        PublicGetLyricsByVideoIdResult result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Lyrics.Should().NotBeNull();
        result.Lyrics.VideoId.Should().Be(videoId);
    }

    [Fact]
    public async Task Handle_WhenNoLyricsLinkedToVideo_ShouldThrowNotFoundException()
    {
        // Arrange
        Guid videoId = Guid.NewGuid();
        var query = new PublicGetLyricsByVideoIdQuery(VideoId: videoId.ToString());

        // Act
        Func<Task> act = async () => await _handler.Handle(query, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }
}
