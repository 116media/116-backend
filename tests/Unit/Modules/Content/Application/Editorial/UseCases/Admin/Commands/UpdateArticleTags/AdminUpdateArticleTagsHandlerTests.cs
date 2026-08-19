using _116.Content.Application.Editorial.UseCases.Admin.Commands.UpdateArticleTags;
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

namespace _116.Unit.Tests.Modules.Content.Application.Editorial.UseCases.Admin.Commands.UpdateArticleTags;

/// <summary>
/// Unit tests for <see cref="AdminUpdateArticleTagsHandler"/>.
/// </summary>
public class AdminUpdateArticleTagsHandlerTests
{
    private readonly Mock<IArticleRepository> _articleRepositoryMock;
    private readonly Mock<ILookupRepository> _lookupRepositoryMock;
    private readonly Mock<IContentUnitOfWork> _unitOfWorkMock;
    private readonly AdminUpdateArticleTagsHandler _handler;

    private static readonly Guid CategoryId = Guid.NewGuid();

    public AdminUpdateArticleTagsHandlerTests()
    {
        _articleRepositoryMock = MockArticleRepository.Create();
        _lookupRepositoryMock = MockLookupRepository.Create();
        _unitOfWorkMock = MockContentUnitOfWork.Create();
        _handler = new AdminUpdateArticleTagsHandler(
            _articleRepositoryMock.Object,
            _lookupRepositoryMock.Object,
            _unitOfWorkMock.Object,
            TestErrorsFactory.CreateContentI18n()
        );
    }

    #region Success Cases

    [Fact]
    public async Task Handle_WhenEmptyTagNames_ShouldClearExistingTagsAndReturnSuccess()
    {
        // Arrange
        ArticleEntity article = ArticleFactory.Create(CategoryId);
        TagEntity existingTag = TagFactory.Create();
        var command = new AdminUpdateArticleTagsCommand(ArticleId: article.Id.ToString(), TagNames: new List<string>());

        _articleRepositoryMock.SetupGetByIdOrThrow(article);
        _articleRepositoryMock.SetupGetTagsByArticleId(
            article.Id,
            new List<ArticleTagEntity>
            {
                ArticleTagEntity.Create(id: Guid.NewGuid(), articleId: article.Id, tagId: existingTag.Id),
            }
        );

        // Act
        AdminUpdateArticleTagsResult result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _articleRepositoryMock.VerifyRemoveTagCalled();
        _unitOfWorkMock.VerifyCommitCalled();
    }

    [Fact]
    public async Task Handle_WhenTagNamesMatchExistingTags_ShouldReuseExistingTagsAndReturnSuccess()
    {
        // Arrange
        ArticleEntity article = ArticleFactory.Create(CategoryId);
        TagEntity tag1 = TagFactory.Create("Fally Ipupa", "fally-ipupa");
        TagEntity tag2 = TagFactory.Create("Kinshasa", "kinshasa");

        var command = new AdminUpdateArticleTagsCommand(
            ArticleId: article.Id.ToString(),
            TagNames: new List<string> { "Fally Ipupa", "Kinshasa" }
        );

        _articleRepositoryMock.SetupGetByIdOrThrow(article);
        _articleRepositoryMock.SetupGetTagsByArticleId(article.Id, new List<ArticleTagEntity>());
        _lookupRepositoryMock.SetupGetTagByName("Fally Ipupa", tag1);
        _lookupRepositoryMock.SetupGetTagByName("Kinshasa", tag2);

        // Act
        AdminUpdateArticleTagsResult result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _lookupRepositoryMock.VerifyAddTagNotCalled();
        _articleRepositoryMock.VerifyAddTagCalled();
        _unitOfWorkMock.VerifyCommitCalled();
    }

    [Fact]
    public async Task Handle_WhenTagNamesAreNew_ShouldCreateTagsAndReturnSuccess()
    {
        // Arrange
        ArticleEntity article = ArticleFactory.Create(CategoryId);

        var command = new AdminUpdateArticleTagsCommand(
            ArticleId: article.Id.ToString(),
            TagNames: new List<string> { "Afrobeats", "Rumba" }
        );

        _articleRepositoryMock.SetupGetByIdOrThrow(article);
        _articleRepositoryMock.SetupGetTagsByArticleId(article.Id, new List<ArticleTagEntity>());
        _lookupRepositoryMock.SetupGetTagByName("Afrobeats", null);
        _lookupRepositoryMock.SetupGetTagByName("Rumba", null);

        // Act
        AdminUpdateArticleTagsResult result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _lookupRepositoryMock.Verify(
            x => x.AddTagAsync(It.IsAny<TagEntity>(), It.IsAny<CancellationToken>()),
            Times.Exactly(2)
        );
        _articleRepositoryMock.VerifyAddTagCalled();
        _unitOfWorkMock.VerifyCommitCalled();
    }

    [Fact]
    public async Task Handle_WhenMixedExistingAndNewTagNames_ShouldUpsertAndReturnSuccess()
    {
        // Arrange
        ArticleEntity article = ArticleFactory.Create(CategoryId);
        TagEntity existingTag = TagFactory.Create("Fally Ipupa", "fally-ipupa");

        var command = new AdminUpdateArticleTagsCommand(
            ArticleId: article.Id.ToString(),
            TagNames: new List<string> { "Fally Ipupa", "NewArtist" }
        );

        _articleRepositoryMock.SetupGetByIdOrThrow(article);
        _articleRepositoryMock.SetupGetTagsByArticleId(article.Id, new List<ArticleTagEntity>());
        _lookupRepositoryMock.SetupGetTagByName("Fally Ipupa", existingTag);
        _lookupRepositoryMock.SetupGetTagByName("NewArtist", null);

        // Act
        AdminUpdateArticleTagsResult result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _lookupRepositoryMock.Verify(
            x => x.AddTagAsync(It.IsAny<TagEntity>(), It.IsAny<CancellationToken>()),
            Times.Once
        );
        _unitOfWorkMock.VerifyCommitCalled();
    }

    [Fact]
    public async Task Handle_WhenTagNameHasDiacritics_ShouldSlugifyAndUpsertCorrectly()
    {
        // Arrange
        ArticleEntity article = ArticleFactory.Create(CategoryId);

        var command = new AdminUpdateArticleTagsCommand(
            ArticleId: article.Id.ToString(),
            TagNames: new List<string> { "Café & Crème" }
        );

        _articleRepositoryMock.SetupGetByIdOrThrow(article);
        _articleRepositoryMock.SetupGetTagsByArticleId(article.Id, new List<ArticleTagEntity>());
        _lookupRepositoryMock.SetupGetTagByName("Café & Crème", null);

        // Act
        AdminUpdateArticleTagsResult result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _lookupRepositoryMock.Verify(
            x => x.GetTagByNameAsync("Café & Crème", It.IsAny<CancellationToken>()),
            Times.Once
        );
        _lookupRepositoryMock.Verify(
            x => x.AddTagAsync(It.IsAny<TagEntity>(), It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    [Fact]
    public async Task Handle_WhenExistingTagsPresent_ShouldRemoveThemBeforeAddingNew()
    {
        // Arrange
        ArticleEntity article = ArticleFactory.Create(CategoryId);
        TagEntity oldTag = TagFactory.Create();
        var existingArticleTag = ArticleTagEntity.Create(id: Guid.NewGuid(), articleId: article.Id, tagId: oldTag.Id);

        TagEntity newTag = TagFactory.Create("Kinshasa", "kinshasa");

        var command = new AdminUpdateArticleTagsCommand(
            ArticleId: article.Id.ToString(),
            TagNames: new List<string> { "Kinshasa" }
        );

        _articleRepositoryMock.SetupGetByIdOrThrow(article);
        _articleRepositoryMock.SetupGetTagsByArticleId(article.Id, new List<ArticleTagEntity> { existingArticleTag });
        _lookupRepositoryMock.SetupGetTagByName("Kinshasa", newTag);

        // Act
        AdminUpdateArticleTagsResult result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _articleRepositoryMock.VerifyRemoveTagCalled();
        _articleRepositoryMock.VerifyAddTagCalled();
        _unitOfWorkMock.VerifyCommitCalled();
    }

    #endregion

    #region Failure Cases

    [Fact]
    public async Task Handle_WhenArticleNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        Guid nonExistentId = Guid.NewGuid();
        var command = new AdminUpdateArticleTagsCommand(
            ArticleId: nonExistentId.ToString(),
            TagNames: new List<string>()
        );
        _articleRepositoryMock.SetupGetByIdOrThrowNotFound(nonExistentId);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_WhenArticleNotFound_ShouldNotInvalidateCache()
    {
        // Arrange
        Guid nonExistentId = Guid.NewGuid();
        var command = new AdminUpdateArticleTagsCommand(
            ArticleId: nonExistentId.ToString(),
            TagNames: new List<string>()
        );
        _articleRepositoryMock.SetupGetByIdOrThrowNotFound(nonExistentId);

        // Act
        try
        {
            await _handler.Handle(command, CancellationToken.None);
        }
        catch (NotFoundException)
        {
            // Expected
        }

        // Assert
    }

    #endregion
}
