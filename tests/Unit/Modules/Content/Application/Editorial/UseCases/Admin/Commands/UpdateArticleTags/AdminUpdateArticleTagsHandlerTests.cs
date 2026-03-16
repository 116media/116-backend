using _116.Content.Application.Editorial.UseCases.Admin.Commands.UpdateArticleTags;
using _116.Content.Application.Shared.Persistence;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Shared.Application.Exceptions;
using _116.Tests.Fixtures.Factories.Content;
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
            _unitOfWorkMock.Object
        );
    }

    #region Success Cases

    [Fact]
    public async Task Handle_WhenEmptyTagList_ShouldClearExistingTagsAndReturnSuccess()
    {
        // Arrange
        ArticleEntity article = ArticleFactory.Create(CategoryId);
        TagEntity existingTag = TagFactory.Create();
        var command = new AdminUpdateArticleTagsCommand(ArticleId: article.Id.ToString(), TagIds: new List<Guid>());

        _articleRepositoryMock.SetupGetByIdOrThrow(article);
        _articleRepositoryMock.SetupGetTagsByArticleId(
            article.Id,
            new List<ArticleTagEntity>
            {
                new() { ArticleId = article.Id, TagId = existingTag.Id },
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
    public async Task Handle_WhenValidTagIds_ShouldReplaceTagsAndReturnSuccess()
    {
        // Arrange
        ArticleEntity article = ArticleFactory.Create(CategoryId);
        TagEntity tag1 = TagFactory.Create();
        TagEntity tag2 = TagFactory.Create();

        var command = new AdminUpdateArticleTagsCommand(
            ArticleId: article.Id.ToString(),
            TagIds: new List<Guid> { tag1.Id, tag2.Id }
        );

        _articleRepositoryMock.SetupGetByIdOrThrow(article);
        _articleRepositoryMock.SetupGetTagsByArticleId(article.Id, new List<ArticleTagEntity>());
        _lookupRepositoryMock.SetupGetTagByIdOrThrow(tag1);
        _lookupRepositoryMock.SetupGetTagByIdOrThrow(tag2);

        // Act
        AdminUpdateArticleTagsResult result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
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
        var command = new AdminUpdateArticleTagsCommand(ArticleId: nonExistentId.ToString(), TagIds: new List<Guid>());
        _articleRepositoryMock.SetupGetByIdOrThrowNotFound(nonExistentId);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_WhenTagNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        ArticleEntity article = ArticleFactory.Create(CategoryId);
        Guid nonExistentTagId = Guid.NewGuid();
        var command = new AdminUpdateArticleTagsCommand(
            ArticleId: article.Id.ToString(),
            TagIds: new List<Guid> { nonExistentTagId }
        );

        _articleRepositoryMock.SetupGetByIdOrThrow(article);
        _articleRepositoryMock.SetupGetTagsByArticleId(article.Id, new List<ArticleTagEntity>());
        _lookupRepositoryMock.SetupGetTagByIdOrThrowNotFound(nonExistentTagId);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    #endregion
}
