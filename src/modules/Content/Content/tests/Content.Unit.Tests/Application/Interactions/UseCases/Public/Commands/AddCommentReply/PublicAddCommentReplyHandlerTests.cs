using _116.BuildingBlocks.Application.Exceptions;
using _116.Content.Application.Interactions.Services;
using _116.Content.Application.Interactions.UseCases.Public.Commands.AddCommentReply;
using _116.Content.Application.Shared.Persistence;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Content.TestData;
using _116.Content.TestData.Factories;
using _116.Content.TestData.Mocks.Infrastructure;
using _116.Content.TestData.Mocks.Repositories;
using _116.Identity.Contracts.Application.DTOs;
using _116.Identity.Contracts.Application.Services;
using _116.Identity.TestData.Mocks.Infrastructure;
using _116.Shared.Domain.Constants;
using _116.Storage.Contracts.Application.Services;
using _116.Storage.TestData.Mocks.Infrastructure;
using _116.Tests.TestData.Helpers;
using AwesomeAssertions;
using Moq;
using Xunit;

namespace _116.Content.Unit.Tests.Application.Interactions.UseCases.Public.Commands.AddCommentReply;

/// <summary>
/// Unit tests for <see cref="PublicAddCommentReplyHandler" />.
/// </summary>
public class PublicAddCommentReplyHandlerTests : BaseContentHandlerTest
{
    private static readonly Guid CategoryId = Guid.NewGuid();

    private readonly Mock<IArticleCommentRepository> _articleCommentRepositoryMock;
    private readonly Mock<IContentUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IUserLookupService> _userLookupMock;
    private readonly Mock<IFileStorageService> _fileStorageMock;
    private readonly PublicAddCommentReplyHandler _handler;

    public PublicAddCommentReplyHandlerTests()
    {
        _articleCommentRepositoryMock = MockArticleCommentRepository.Create();
        _unitOfWorkMock = MockContentUnitOfWork.Create();
        _userLookupMock = new Mock<IUserLookupService>();
        _fileStorageMock = new Mock<IFileStorageService>();
        _handler = new PublicAddCommentReplyHandler(
            _articleCommentRepositoryMock.Object,
            _unitOfWorkMock.Object,
            new ArticleCommentDtoService(_userLookupMock.Object, _fileStorageMock.Object),
            TestErrorsFactory.CreateContentI18n()
        );
    }

    [Fact]
    public async Task Handle_WhenParentIsTopLevel_ShouldCreateReplyAndReturnAuthor()
    {
        // Arrange
        ArticleEntity article = ArticleFactory.CreatePublished(CategoryId);
        ArticleCommentEntity parent = ArticleCommentFactory.Create(article.Id, Guid.NewGuid());
        Guid replierId = Guid.NewGuid();

        _articleCommentRepositoryMock.SetupExistsOrThrow(article.Id);
        _articleCommentRepositoryMock.SetupGetCommentByIdAsync(parent);
        _userLookupMock
            .Setup(x => x.GetUserProfileByIdAsync(replierId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new UserProfileDto("bob", "bob@example.com", null, "Visitor", LocaleConstants.DefaultLocale));

        var command = new PublicAddCommentReplyCommand(article.Id, parent.Id, replierId, "A valid reply body.");

        // Act
        PublicAddCommentReplyResult result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Reply.ParentCommentId.Should().Be(parent.Id);
        result.Reply.Author.Should().NotBeNull();
        result.Reply.Author!.UserName.Should().Be("bob");
        _articleCommentRepositoryMock.VerifyAddCommentCalled();
        _unitOfWorkMock.VerifyCommitCalled();
    }

    [Fact]
    public async Task Handle_WhenParentIsItselfAReply_ShouldThrowBadRequest()
    {
        ArticleEntity article = ArticleFactory.CreatePublished(CategoryId);
        ArticleCommentEntity grandparent = ArticleCommentFactory.Create(article.Id, Guid.NewGuid());
        ArticleCommentEntity parentReply = ArticleCommentFactory.CreateReply(
            article.Id,
            Guid.NewGuid(),
            grandparent.Id,
            "I am already a reply."
        );

        _articleCommentRepositoryMock.SetupExistsOrThrow(article.Id);
        _articleCommentRepositoryMock.SetupGetCommentByIdAsync(parentReply);

        var command = new PublicAddCommentReplyCommand(article.Id, parentReply.Id, Guid.NewGuid(), "nested reply");

        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<BadRequestException>();
        _articleCommentRepositoryMock.Verify(
            x => x.AddCommentAsync(It.IsAny<ArticleCommentEntity>(), It.IsAny<CancellationToken>()),
            Times.Never
        );
    }

    [Fact]
    public async Task Handle_WhenParentNotFound_ShouldThrowNotFound()
    {
        ArticleEntity article = ArticleFactory.CreatePublished(CategoryId);
        _articleCommentRepositoryMock.SetupExistsOrThrow(article.Id);
        Guid missingParentId = Guid.NewGuid();
        _articleCommentRepositoryMock.SetupGetCommentByIdNotFound(missingParentId);

        var command = new PublicAddCommentReplyCommand(article.Id, missingParentId, Guid.NewGuid(), "reply");

        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_WhenParentBelongsToDifferentArticle_ShouldThrowNotFound()
    {
        ArticleEntity article = ArticleFactory.CreatePublished(CategoryId);
        ArticleCommentEntity parentOnOtherArticle = ArticleCommentFactory.Create(Guid.NewGuid(), Guid.NewGuid());

        _articleCommentRepositoryMock.SetupExistsOrThrow(article.Id);
        _articleCommentRepositoryMock.SetupGetCommentByIdAsync(parentOnOtherArticle);

        var command = new PublicAddCommentReplyCommand(article.Id, parentOnOtherArticle.Id, Guid.NewGuid(), "reply");

        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }
}
