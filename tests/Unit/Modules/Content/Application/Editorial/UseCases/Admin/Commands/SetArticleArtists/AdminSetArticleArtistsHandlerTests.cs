using _116.Content.Application.Editorial.UseCases.Admin.Commands.SetArticleArtists;
using _116.Content.Application.Shared.Persistence;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Shared.Application.Exceptions;
using _116.Tests.Fixtures.Factories.Content;
using _116.Tests.Fixtures.Helpers;
using _116.Unit.Tests.Common.Mocks.Infrastructure;
using _116.Unit.Tests.Common.Mocks.Repositories;
using AwesomeAssertions;
using Moq;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Editorial.UseCases.Admin.Commands.SetArticleArtists;

/// <summary>
/// Unit tests for <see cref="AdminSetArticleArtistsHandler"/>.
/// </summary>
public class AdminSetArticleArtistsHandlerTests
{
    private readonly Mock<IArticleRepository> _articleRepositoryMock;
    private readonly Mock<IArtistRepository> _artistRepositoryMock;
    private readonly Mock<IContentUnitOfWork> _unitOfWorkMock;
    private readonly AdminSetArticleArtistsHandler _handler;

    public AdminSetArticleArtistsHandlerTests()
    {
        _articleRepositoryMock = MockArticleRepository.Create();
        _artistRepositoryMock = MockArtistRepository.Create();
        _unitOfWorkMock = MockContentUnitOfWork.Create();
        _handler = new AdminSetArticleArtistsHandler(
            _articleRepositoryMock.Object,
            _artistRepositoryMock.Object,
            _unitOfWorkMock.Object,
            TestErrorsFactory.CreateContentI18n()
        );
    }

    [Fact]
    public async Task Handle_WithExistingArtists_ShouldReplaceAndCommit()
    {
        // Arrange
        ArticleEntity article = ArticleFactory.Create(Guid.NewGuid());
        ArtistEntity artist = ArtistFactory.Create();
        _articleRepositoryMock.SetupGetByIdOrThrow(article);
        _artistRepositoryMock.SetupGetByIdAsync(artist.Id, artist);

        var command = new AdminSetArticleArtistsCommand(article.Id, [artist.Id]);

        // Act
        AdminSetArticleArtistsResult result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.ArtistIds.Should().Equal(artist.Id);
        article.Artists.Should().ContainSingle().Which.ArtistId.Should().Be(artist.Id);
        _unitOfWorkMock.VerifyCommitCalled();
    }

    [Fact]
    public async Task Handle_WithUnknownArtistId_ShouldThrowNotFoundBeforeWriting()
    {
        // Arrange
        ArticleEntity article = ArticleFactory.Create(Guid.NewGuid());
        _articleRepositoryMock.SetupGetByIdOrThrow(article);
        var missingArtistId = Guid.NewGuid();

        var command = new AdminSetArticleArtistsCommand(article.Id, [missingArtistId]);

        // Act
        Func<Task> act = () => _handler.Handle(command, CancellationToken.None);

        // Assert — validated before anything is written, so a bad id never half-applies.
        await act.Should().ThrowAsync<NotFoundException>();
        article.Artists.Should().BeEmpty();
        _unitOfWorkMock.VerifyCommitNotCalled();
    }

    [Fact]
    public async Task Handle_WithEmptyList_ShouldUntagEverything()
    {
        // Arrange
        ArticleEntity article = ArticleFactory.Create(Guid.NewGuid());
        ArticleArtistFactory.Link(article, Guid.NewGuid());
        _articleRepositoryMock.SetupGetByIdOrThrow(article);

        var command = new AdminSetArticleArtistsCommand(article.Id, []);

        // Act
        AdminSetArticleArtistsResult result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.ArtistIds.Should().BeEmpty();
        article.Artists.Should().BeEmpty();
        _unitOfWorkMock.VerifyCommitCalled();
    }
}
