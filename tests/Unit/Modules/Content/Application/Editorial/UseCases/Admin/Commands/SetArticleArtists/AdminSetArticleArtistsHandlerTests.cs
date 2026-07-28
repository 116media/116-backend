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
        _articleRepositoryMock
            .Setup(x => x.GetArtistsByArticleIdAsync(article.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync([ArticleArtistEntity.Create(Guid.NewGuid(), article.Id, artist.Id)]);

        var command = new AdminSetArticleArtistsCommand(article.Id, [artist.Id]);

        // Act
        AdminSetArticleArtistsResult result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.ArtistIds.Should().Equal(artist.Id);
        _articleRepositoryMock.Verify(
            x => x.ReplaceArticleArtistsAsync(article.Id, command.ArtistIds, It.IsAny<CancellationToken>()),
            Times.Once
        );
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
        _articleRepositoryMock.Verify(
            x =>
                x.ReplaceArticleArtistsAsync(
                    It.IsAny<Guid>(),
                    It.IsAny<IReadOnlyList<Guid>>(),
                    It.IsAny<CancellationToken>()
                ),
            Times.Never
        );
        _unitOfWorkMock.VerifyCommitNotCalled();
    }

    [Fact]
    public async Task Handle_WithEmptyList_ShouldUntagEverything()
    {
        // Arrange — empty is valid: an article about nobody in particular must be untaggable.
        ArticleEntity article = ArticleFactory.Create(Guid.NewGuid());
        _articleRepositoryMock.SetupGetByIdOrThrow(article);
        _articleRepositoryMock
            .Setup(x => x.GetArtistsByArticleIdAsync(article.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        var command = new AdminSetArticleArtistsCommand(article.Id, []);

        // Act
        AdminSetArticleArtistsResult result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.ArtistIds.Should().BeEmpty();
        _articleRepositoryMock.Verify(
            x => x.ReplaceArticleArtistsAsync(article.Id, command.ArtistIds, It.IsAny<CancellationToken>()),
            Times.Once
        );
        _unitOfWorkMock.VerifyCommitCalled();
    }
}
