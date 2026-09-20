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
    private readonly Mock<ITagRepository> _tagRepositoryMock;
    private readonly Mock<IContentUnitOfWork> _unitOfWorkMock;
    private readonly AdminUpdateArticleTagsHandler _handler;

    private static readonly Guid CategoryId = Guid.NewGuid();

    public AdminUpdateArticleTagsHandlerTests()
    {
        _articleRepositoryMock = MockArticleRepository.Create();
        _tagRepositoryMock = MockTagRepository.Create();
        _unitOfWorkMock = MockContentUnitOfWork.Create();
        _handler = new AdminUpdateArticleTagsHandler(
            _articleRepositoryMock.Object,
            _tagRepositoryMock.Object,
            _unitOfWorkMock.Object
        );
    }

    #region Success Cases

    [Fact]
    public async Task Handle_WhenEmptyTagNames_ShouldClearExistingTagsAndReturnSuccess()
    {
        // Arrange
        ArticleEntity article = ArticleFactory.Create(CategoryId);
        TagEntity existingTag = TagFactory.Create();
        article.ReplaceTags([existingTag.Id]);
        var command = new AdminUpdateArticleTagsCommand(ArticleId: article.Id.ToString(), TagNames: new List<string>());

        _articleRepositoryMock.SetupGetByIdOrThrow(article);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        article.Tags.Should().BeEmpty();
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
        _tagRepositoryMock.SetupGetTagByName("Fally Ipupa", tag1);
        _tagRepositoryMock.SetupGetTagByName("Kinshasa", tag2);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        article.Tags.Select(t => t.TagId).Should().BeEquivalentTo([tag1.Id, tag2.Id]);
        article.Tags.Should().OnlyContain(t => t.ArticleId == article.Id);
        _tagRepositoryMock.VerifyAddTagNotCalled();
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
        _tagRepositoryMock.SetupGetTagByName("Afrobeats", null);
        _tagRepositoryMock.SetupGetTagByName("Rumba", null);

        var created = new List<TagEntity>();
        _tagRepositoryMock
            .Setup(x => x.AddAsync(Capture.In(created), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        created.Select(t => t.Name).Should().Equal("Afrobeats", "Rumba");
        _tagRepositoryMock.Verify(
            x => x.AddAsync(It.Is<TagEntity>(t => t.Name == "Afrobeats"), It.IsAny<CancellationToken>()),
            Times.Once
        );
        _tagRepositoryMock.Verify(
            x => x.AddAsync(It.Is<TagEntity>(t => t.Name == "Rumba"), It.IsAny<CancellationToken>()),
            Times.Once
        );
        _tagRepositoryMock.Verify(
            x => x.AddAsync(It.IsAny<TagEntity>(), It.IsAny<CancellationToken>()),
            Times.Exactly(2)
        );
        article.Tags.Select(t => t.TagId).Should().BeEquivalentTo(created.Select(t => t.Id));
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
        _tagRepositoryMock.SetupGetTagByName("Fally Ipupa", existingTag);
        _tagRepositoryMock.SetupGetTagByName("NewArtist", null);

        var created = new List<TagEntity>();
        _tagRepositoryMock
            .Setup(x => x.AddAsync(Capture.In(created), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        created.Select(t => t.Name).Should().Equal("NewArtist");
        _tagRepositoryMock.Verify(
            x => x.AddAsync(It.Is<TagEntity>(t => t.Name == "NewArtist"), It.IsAny<CancellationToken>()),
            Times.Once
        );
        _tagRepositoryMock.Verify(x => x.AddAsync(It.IsAny<TagEntity>(), It.IsAny<CancellationToken>()), Times.Once);
        article.Tags.Select(t => t.TagId).Should().BeEquivalentTo([existingTag.Id, created[0].Id]);
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
        _tagRepositoryMock.SetupGetTagByName("Café & Crème", null);

        var created = new List<TagEntity>();
        _tagRepositoryMock
            .Setup(x => x.AddAsync(Capture.In(created), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        created.Should().ContainSingle();
        created[0].Name.Should().Be("Café & Crème");
        created[0].Slug.Value.Should().StartWith("cafe-creme-");
        _tagRepositoryMock.Verify(
            x =>
                x.GetByNamesAsync(
                    It.Is<IReadOnlyCollection<string>>(names => names.Contains("Café & Crème")),
                    It.IsAny<CancellationToken>()
                ),
            Times.Once
        );
    }

    [Fact]
    public async Task Handle_WhenExistingTagsPresent_ShouldRemoveThemBeforeAddingNew()
    {
        // Arrange
        ArticleEntity article = ArticleFactory.Create(CategoryId);
        TagEntity oldTag = TagFactory.Create();
        article.ReplaceTags([oldTag.Id]);

        TagEntity newTag = TagFactory.Create("Kinshasa", "kinshasa");

        var command = new AdminUpdateArticleTagsCommand(
            ArticleId: article.Id.ToString(),
            TagNames: new List<string> { "Kinshasa" }
        );

        _articleRepositoryMock.SetupGetByIdOrThrow(article);
        _tagRepositoryMock.SetupGetTagByName("Kinshasa", newTag);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        article.Tags.Select(t => t.TagId).Should().BeEquivalentTo([newTag.Id]);
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
    public async Task Handle_WhenArticleNotFound_ShouldNotModifyTagsOrCommit()
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
        _unitOfWorkMock.VerifyCommitNotCalled();
    }

    #endregion
}
