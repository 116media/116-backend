using _116.Content.Application.Editorial.UseCases.Public.Queries.GetArtists;
using _116.Content.Application.Shared.DTOs;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Core.Application.Shared.Repositories;
using _116.Core.Contracts.Application.DTOs;
using _116.Core.Contracts.Application.Services;
using _116.Core.Domain.Entities;
using _116.Shared.Application.Pagination;
using _116.Tests.Fixtures.Constants;
using _116.Tests.Fixtures.Factories.Content;
using _116.Tests.Fixtures.Factories.Core;
using _116.Tests.Fixtures.Helpers;
using _116.Unit.Tests.Common.Mocks.Repositories;
using _116.Unit.Tests.Common.Mocks.Services;
using AwesomeAssertions;
using Moq;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Editorial.UseCases.Public.Queries.GetArtists;

/// <summary>
/// Unit tests for <see cref="PublicGetArtistsHandler"/> covering the directory
/// page shape, the avatar URL resolution, and the verified flag.
/// </summary>
public class PublicGetArtistsHandlerTests
{
    private readonly Mock<IArtistRepository> _artistRepositoryMock;
    private readonly Mock<IFileStorageService> _fileStorageMock;
    private readonly PublicGetArtistsHandler _handler;

    public PublicGetArtistsHandlerTests()
    {
        _artistRepositoryMock = MockArtistRepository.Create();
        _fileStorageMock = MockFileStorageService.Create();
        _handler = new PublicGetArtistsHandler(_artistRepositoryMock.Object, _fileStorageMock.Object);
    }

    /// <summary>
    /// Points the directory query at the supplied rows and letters.
    /// </summary>
    /// <param name="totalCount">The directory-wide match count.</param>
    /// <param name="letters">The available first letters.</param>
    /// <param name="rows">The rows on the requested page.</param>
    private void SetupDirectory(int totalCount, IReadOnlyList<string> letters, params ArtistDirectoryRow[] rows)
    {
        _artistRepositoryMock.SetupGetPublicDirectory([.. rows], totalCount);
        _artistRepositoryMock
            .Setup(r => r.GetAvailableLettersAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(letters);
    }

    [Fact]
    public async Task Handle_ShouldShapeThePageAndCarryTheAvailableLetters()
    {
        // Arrange
        ArtistEntity artist = ArtistFactory.Create("Fally Ipupa", "fally-ipupa");
        SetupDirectory(31, ["F", "K"], new ArtistDirectoryRow(artist, ContentCount: 4));

        var query = new PublicGetArtistsQuery(new PaginatedRequest(0, 10), Letter: null, Search: null);

        // Act
        PublicGetArtistsResult result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Artists.Count.Should().Be(31);
        result.AvailableLetters.Should().BeEquivalentTo(["F", "K"]);

        ArtistSummaryDto card = result.Artists.Items.Should().ContainSingle().Subject;
        card.Name.Should().Be("Fally Ipupa");
        card.Slug.Should().Be("fally-ipupa");
        card.ContentCount.Should().Be(4);
        card.AvatarUrl.Should().BeNull();
        card.IsVerified.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_WithAnAvatarOnFile_ShouldResolveTheStorageUrl()
    {
        // Arrange
        FileReferenceDto avatar = FileReferenceDtoFactory.CreateJpeg();
        ArtistEntity artist = ArtistFactory.Create();
        artist.SetAvatarFileId(avatar.Id);
        _fileStorageMock.SetupResolve(avatar);
        SetupDirectory(1, [], new ArtistDirectoryRow(artist, ContentCount: 0));

        var query = new PublicGetArtistsQuery(new PaginatedRequest(0, 10), Letter: null, Search: null);

        // Act
        PublicGetArtistsResult result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Artists.Items.Single().AvatarUrl.Should().Be(avatar.StorageUrl);
    }

    [Fact]
    public async Task Handle_WithAClaimedProfile_ShouldReportTheArtistVerified()
    {
        // Arrange
        ArtistEntity artist = ArtistFactory.Create();
        artist.ClaimOwnership(Guid.NewGuid(), TestConstants.Clock.Instant);
        SetupDirectory(1, [], new ArtistDirectoryRow(artist, ContentCount: 0));

        var query = new PublicGetArtistsQuery(new PaginatedRequest(0, 10), Letter: null, Search: null);

        // Act
        PublicGetArtistsResult result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Artists.Items.Single().IsVerified.Should().BeTrue();
    }
}
