using _116.Content.Application.Interactions.UseCases.Public.Commands.AddCommentReply;
using _116.Content.Application.Shared.Cache;
using _116.Content.Application.Shared.Persistence;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Core.Application.Shared.Repositories;
using _116.Identity.Contracts.Application;
using _116.Shared.Application.Exceptions;
using _116.Tests.Fixtures.Factories.Content;
using _116.Tests.Fixtures.Helpers;
using _116.Unit.Tests.Common;
using _116.Unit.Tests.Common.Mocks.Infrastructure;
using _116.Unit.Tests.Common.Mocks.Repositories;
using AwesomeAssertions;
using Moq;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Interactions.UseCases.Public.Commands.AddCommentReply;

/// <summary>
/// Unit tests for <see cref="PublicAddCommentReplyHandler" />.
/// </summary>
public class PublicAddCommentReplyHandlerTests : BaseContentHandlerTest
{
    private static readonly Guid CategoryId = Guid.NewGuid();

    private readonly Mock<IArticleRepository> _articleRepositoryMock;
    private readonly Mock<IContentUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IPopularArticlesCacheInvalidator> _cacheInvalidatorMock;
    private readonly Mock<IUserLookupService> _userLookupMock;
    private readonly Mock<IFileRepository> _fileRepositoryMock;
    private readonly PublicAddCommentReplyHandler _handler;

    public PublicAddCommentReplyHandlerTests()
    {
        _articleRepositoryMock = MockArticleRepository.Create();
        _unitOfWorkMock = MockContentUnitOfWork.Create();
        _cacheInvalidatorMock = MockPopularArticlesCacheInvalidator.Create();
        _userLookupMock = new Mock<IUserLookupService>();
        _fileRepositoryMock = new Mock<IFileRepository>();
        _handler = new PublicAddCommentReplyHandler(
            _articleRepositoryMock.Object,
            _unitOfWorkMock.Object,
            _cacheInvalidatorMock.Object,
            _userLookupMock.Object,
            _fileRepositoryMock.Object,
            Mapper,
            TestErrorsFactory.CreateContentI18n()
        );
    }

    [Fact]
    public async Task Handle_WhenParentIsTopLevel_ShouldCreateReplyIncrementCountAndReturnAuthor()
    {
        // Arrange
        ArticleEntity article = ArticleFactory.CreatePublished(CategoryId);
        ArticleCommentEntity parent = ArticleCommentFactory.Create(article.Id, Guid.NewGuid());
        Guid replierId = Guid.NewGuid();

        _articleRepositoryMock.SetupGetByIdOrThrow(article);
        _articleRepositoryMock.SetupGetCommentByIdAsync(parent);
        _userLookupMock
            .Setup(x => x.GetAuthorInfoByIdAsync(replierId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new AuthorInfo("bob", "bob@example.com", null, "Visitor"));

        var command = new PublicAddCommentReplyCommand(article.Id, parent.Id, replierId, "A valid reply body.");

        // Act
        PublicAddCommentReplyResult result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Reply.ParentCommentId.Should().Be(parent.Id);
        result.Reply.Author.Should().NotBeNull();
        result.Reply.Author!.UserName.Should().Be("bob");
        result.Reply.Author.Email.Should().BeNull();
        _articleRepositoryMock.VerifyAddCommentCalled();
        _unitOfWorkMock.VerifyCommitCalled();
    }

    [Fact]
    public async Task Handle_WhenParentIsItselfAReply_ShouldThrowBadRequest()
    {
        ArticleEntity article = ArticleFactory.CreatePublished(CategoryId);
        ArticleCommentEntity grandparent = ArticleCommentFactory.Create(article.Id, Guid.NewGuid());
        ArticleCommentEntity parentReply = ArticleCommentEntity.CreateReply(
            id: Guid.NewGuid(),
            userId: Guid.NewGuid(),
            articleId: article.Id,
            parentCommentId: grandparent.Id,
            body: "I am already a reply."
        );

        _articleRepositoryMock.SetupGetByIdOrThrow(article);
        _articleRepositoryMock.SetupGetCommentByIdAsync(parentReply);

        var command = new PublicAddCommentReplyCommand(article.Id, parentReply.Id, Guid.NewGuid(), "nested reply");

        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<BadRequestException>();
        _articleRepositoryMock.Verify(
            x => x.AddCommentAsync(It.IsAny<ArticleCommentEntity>(), It.IsAny<CancellationToken>()),
            Times.Never
        );
    }

    [Fact]
    public async Task Handle_WhenParentNotFound_ShouldThrowNotFound()
    {
        ArticleEntity article = ArticleFactory.CreatePublished(CategoryId);
        _articleRepositoryMock.SetupGetByIdOrThrow(article);
        _articleRepositoryMock.SetupGetCommentByIdAsync(null);

        var command = new PublicAddCommentReplyCommand(article.Id, Guid.NewGuid(), Guid.NewGuid(), "reply");

        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_WhenParentBelongsToDifferentArticle_ShouldThrowNotFound()
    {
        ArticleEntity article = ArticleFactory.CreatePublished(CategoryId);
        ArticleCommentEntity parentOnOtherArticle = ArticleCommentFactory.Create(Guid.NewGuid(), Guid.NewGuid());

        _articleRepositoryMock.SetupGetByIdOrThrow(article);
        _articleRepositoryMock.SetupGetCommentByIdAsync(parentOnOtherArticle);

        var command = new PublicAddCommentReplyCommand(article.Id, parentOnOtherArticle.Id, Guid.NewGuid(), "reply");

        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }
}
