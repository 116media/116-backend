using _116.Content.Application.Editorial.UseCases.Admin.Queries.GetLyricsSubmissions;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;
using _116.Shared.Application.Pagination;
using _116.Tests.Fixtures.Factories.Content;
using _116.Unit.Tests.Common.Mocks.Repositories;
using AwesomeAssertions;
using Moq;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Editorial.UseCases.Admin.Queries.GetLyricsSubmissions;

/// <summary>
/// Unit tests for <see cref="AdminGetLyricsSubmissionsHandler"/>.
/// </summary>
public class AdminGetLyricsSubmissionsHandlerTests
{
    private readonly Mock<ILyricsSubmissionRepository> _submissionRepositoryMock;
    private readonly AdminGetLyricsSubmissionsHandler _handler;

    public AdminGetLyricsSubmissionsHandlerTests()
    {
        _submissionRepositoryMock = MockLyricsSubmissionRepository.Create();
        _handler = new AdminGetLyricsSubmissionsHandler(_submissionRepositoryMock.Object);
    }

    #region Success Cases

    [Fact]
    public async Task Handle_WithExistingSubmissions_ShouldReturnPaginatedDtos()
    {
        // Arrange
        LyricsSubmissionEntity submission = LyricsSubmissionFactory.Create("Eloko Oyo", "Fally Ipupa");
        _submissionRepositoryMock.SetupGetAllAsync([submission], 1);
        var query = new AdminGetLyricsSubmissionsQuery(new PaginatedRequest(PageIndex: 0, PageSize: 10), Status: null);

        // Act
        AdminGetLyricsSubmissionsResult result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Submissions.Items.Should().ContainSingle();
        LyricsSubmissionDto dto = result.Submissions.Items.First();
        dto.Id.Should().Be(submission.Id);
        dto.SongTitle.Should().Be("Eloko Oyo");
        dto.ArtistName.Should().Be("Fally Ipupa");
        dto.Status.Should().Be(EnumSubmissionStatus.Pending.ToString());
        result.Submissions.Count.Should().Be(1);
    }

    [Fact]
    public async Task Handle_WithNoSubmissions_ShouldReturnEmptyPaginatedResult()
    {
        // Arrange
        _submissionRepositoryMock.SetupGetAllAsync([], 0);
        var query = new AdminGetLyricsSubmissionsQuery(new PaginatedRequest(PageIndex: 0, PageSize: 10), Status: null);

        // Act
        AdminGetLyricsSubmissionsResult result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Submissions.Items.Should().BeEmpty();
        result.Submissions.Count.Should().Be(0);
    }

    #endregion
}
