using _116.Content.Application.Editorial.UseCases.Admin.Queries.GetAllLyrics;
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

namespace _116.Unit.Tests.Modules.Content.Application.Editorial.UseCases.Admin.Queries.GetAllLyrics;

/// <summary>
/// Unit tests for <see cref="AdminGetAllLyricsHandler"/>.
/// </summary>
public class AdminGetAllLyricsHandlerTests : BaseContentHandlerTest
{
    private readonly Mock<ILyricsRepository> _lyricsRepositoryMock;
    private readonly AdminGetAllLyricsHandler _handler;

    private static readonly Guid CategoryId = Guid.NewGuid();

    public AdminGetAllLyricsHandlerTests()
    {
        _lyricsRepositoryMock = MockLyricsRepository.Create();
        Mock<IFileRepository> fileRepositoryMock = MockFileRepository.Create();
        _handler = new AdminGetAllLyricsHandler(_lyricsRepositoryMock.Object, fileRepositoryMock.Object);
    }

    [Fact]
    public async Task Handle_WhenLyricsExist_ShouldReturnPaginatedResult()
    {
        // Arrange
        List<LyricsEntity> lyricsList = LyricsFactory.CreateMany(CategoryId, 3);
        var query = new AdminGetAllLyricsQuery(
            PaginatedRequest: new PaginatedRequest(0, 10),
            Search: null,
            Status: null,
            CategoryId: null
        );

        _lyricsRepositoryMock.SetupGetAllAsync(lyricsList, lyricsList.Count);

        // Act
        AdminGetAllLyricsResult result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Lyrics.Items.Should().HaveCount(lyricsList.Count);
        result.Lyrics.Count.Should().Be((long)lyricsList.Count);
    }

    [Fact]
    public async Task Handle_WhenNoLyricsExist_ShouldReturnEmptyPaginatedResult()
    {
        // Arrange
        var query = new AdminGetAllLyricsQuery(
            PaginatedRequest: new PaginatedRequest(0, 10),
            Search: null,
            Status: null,
            CategoryId: null
        );

        _lyricsRepositoryMock.SetupGetAllAsync(new List<LyricsEntity>(), 0);

        // Act
        AdminGetAllLyricsResult result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Lyrics.Items.Should().BeEmpty();
        result.Lyrics.Count.Should().Be(0);
    }

    [Fact]
    public async Task Handle_WithStatusFilter_ShouldPassStatusToRepository()
    {
        // Arrange
        var query = new AdminGetAllLyricsQuery(
            PaginatedRequest: new PaginatedRequest(0, 10),
            Search: null,
            Status: EnumContentStatus.Published,
            CategoryId: null
        );

        _lyricsRepositoryMock.SetupGetAllAsync(new List<LyricsEntity>(), 0);

        // Act
        await _handler.Handle(query, CancellationToken.None);

        // Assert
        _lyricsRepositoryMock.Verify(
            x =>
                x.GetAllAsync(
                    1,
                    10,
                    null,
                    EnumContentStatus.Published,
                    null,
                    null,
                    null,
                    It.IsAny<CancellationToken>()
                ),
            Times.Once
        );
    }
}
