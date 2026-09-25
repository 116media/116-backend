using _116.Content.Application.Interactions.EventHandlers;
using _116.Content.Application.Shared.Messages;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Content.Domain.Events;
using _116.Identity.Contracts.Application.DTOs;
using _116.Identity.Contracts.Application.Services;
using _116.Mailer.Contracts.Application.Messages;
using _116.Mailer.Contracts.Application.Services;
using _116.Mailer.Contracts.Domain.Enums;
using _116.Tests.Fixtures.Factories.Content;
using _116.Unit.Tests.Common.Mocks.Repositories;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Interactions.EventHandlers;

/// <summary>
/// Unit tests for <see cref="CommentReplyAddedNotificationsHandler"/>.
/// </summary>
public class CommentReplyAddedNotificationsHandlerTests
{
    private readonly Mock<IArticleRepository> _articleRepositoryMock;
    private readonly Mock<IArticleCommentRepository> _articleCommentRepositoryMock;
    private readonly Mock<IUserLookupService> _userLookupServiceMock = new();
    private readonly Mock<IMessageDispatcher> _dispatcherMock = new();
    private readonly Mock<INotificationService> _notifierMock = new();
    private readonly CommentReplyAddedNotificationsHandler _handler;

    private readonly ArticleEntity _article;
    private readonly ArticleCommentEntity _parent;
    private readonly ArticleCommentEntity _reply;
    private readonly Guid _parentAuthorId = Guid.NewGuid();
    private readonly Guid _replierId = Guid.NewGuid();

    public CommentReplyAddedNotificationsHandlerTests()
    {
        _articleRepositoryMock = MockArticleRepository.Create();
        _articleCommentRepositoryMock = MockArticleCommentRepository.Create();

        _article = ArticleFactory.CreatePublished(Guid.NewGuid());
        _parent = ArticleCommentFactory.Create(_article.Id, _parentAuthorId);
        _reply = ArticleCommentEntity.CreateReply(
            id: Guid.NewGuid(),
            userId: _replierId,
            articleId: _article.Id,
            parentCommentId: _parent.Id,
            body: "Totally agree with you!"
        );

        _articleCommentRepositoryMock
            .Setup(x => x.GetCommentByIdAsync(_parent.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(_parent);
        _articleCommentRepositoryMock
            .Setup(x => x.GetCommentByIdAsync(_reply.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(_reply);
        _articleRepositoryMock.SetupGetByIdAsync(_article.Id, _article);

        _handler = new CommentReplyAddedNotificationsHandler(
            _articleRepositoryMock.Object,
            _articleCommentRepositoryMock.Object,
            _userLookupServiceMock.Object,
            _dispatcherMock.Object,
            _notifierMock.Object,
            NullLogger<CommentReplyAddedNotificationsHandler>.Instance
        );
    }

    [Fact]
    public async Task Handle_ShouldEnqueueTheCommentReplyEmailToTheParentAuthor()
    {
        // Arrange
        SetupParentAuthor("author@test.com");
        SetupReplierName("Aline");

        // Act
        await _handler.Handle(
            new CommentReplyAddedEvent(_reply.Id, _parent.Id, _article.Id, _replierId),
            CancellationToken.None
        );

        // Assert
        _dispatcherMock.Verify(
            x =>
                x.DispatchAsync(
                    It.Is<Message>(m =>
                        m.TemplateName == ContentMessageTemplates.CommentReply
                        && m.Recipients[0].Address == "author@test.com"
                        && m.Tokens["userName"] == "Fally"
                        && m.Tokens["replierName"] == "Aline"
                        && m.Tokens["articleTitle"] == _article.Title
                        && m.Tokens["replyExcerpt"] == "Totally agree with you!"
                    ),
                    It.IsAny<CancellationToken>()
                ),
            Times.Once
        );
    }

    [Fact]
    public async Task Handle_ShouldWriteTheCommentReplyNotificationLinkedToTheArticle()
    {
        // Arrange
        SetupParentAuthor("author@test.com");
        SetupReplierName("Aline");

        // Act
        await _handler.Handle(
            new CommentReplyAddedEvent(_reply.Id, _parent.Id, _article.Id, _replierId),
            CancellationToken.None
        );

        // Assert
        _notifierMock.Verify(
            x =>
                x.NotifyAsync(
                    _parentAuthorId,
                    EnumNotificationType.CommentReply,
                    It.Is<IReadOnlyDictionary<string, string>>(t =>
                        t["replierName"] == "Aline"
                        && t["articleTitle"] == _article.Title
                        && t["linkPath"] == $"/articles/{_article.Slug}"
                    ),
                    It.IsAny<CancellationToken>()
                ),
            Times.Once
        );
    }

    [Fact]
    public async Task Handle_WhenTheReplierCannotBeResolved_ShouldFallBackToAnAnonymousName()
    {
        // Arrange
        SetupParentAuthor("author@test.com");

        // Act
        await _handler.Handle(
            new CommentReplyAddedEvent(_reply.Id, _parent.Id, _article.Id, _replierId),
            CancellationToken.None
        );

        // Assert
        _notifierMock.Verify(
            x =>
                x.NotifyAsync(
                    _parentAuthorId,
                    EnumNotificationType.CommentReply,
                    It.Is<IReadOnlyDictionary<string, string>>(t => t["replierName"] == "Someone"),
                    It.IsAny<CancellationToken>()
                ),
            Times.Once
        );
    }

    [Fact]
    public async Task Handle_WhenTheReplyIsASelfReply_ShouldSkipBothChannels()
    {
        // Arrange
        SetupParentAuthor("author@test.com");

        // Act
        await _handler.Handle(
            new CommentReplyAddedEvent(_reply.Id, _parent.Id, _article.Id, _parentAuthorId),
            CancellationToken.None
        );

        // Assert
        _dispatcherMock.VerifyNoOtherCalls();
        _notifierMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Handle_WhenTheParentAuthorHasNoEmail_ShouldSkipTheEmailButStillNotify()
    {
        // Arrange
        SetupParentAuthor(email: null);
        SetupReplierName("Aline");

        // Act
        await _handler.Handle(
            new CommentReplyAddedEvent(_reply.Id, _parent.Id, _article.Id, _replierId),
            CancellationToken.None
        );

        // Assert
        _dispatcherMock.VerifyNoOtherCalls();
        _notifierMock.Verify(
            x =>
                x.NotifyAsync(
                    _parentAuthorId,
                    EnumNotificationType.CommentReply,
                    It.IsAny<IReadOnlyDictionary<string, string>>(),
                    It.IsAny<CancellationToken>()
                ),
            Times.Once
        );
    }

    [Fact]
    public async Task Handle_WhenTheParentAuthorCannotBeResolved_ShouldSkipBothChannels()
    {
        // Act
        await _handler.Handle(
            new CommentReplyAddedEvent(_reply.Id, _parent.Id, _article.Id, _replierId),
            CancellationToken.None
        );

        // Assert
        _dispatcherMock.VerifyNoOtherCalls();
        _notifierMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Handle_WhenTheParentCommentIsGone_ShouldSkipBothChannels()
    {
        // Act
        await _handler.Handle(
            new CommentReplyAddedEvent(_reply.Id, Guid.NewGuid(), _article.Id, _replierId),
            CancellationToken.None
        );

        // Assert
        _dispatcherMock.VerifyNoOtherCalls();
        _notifierMock.VerifyNoOtherCalls();
    }

    private void SetupParentAuthor(string? email)
    {
        _userLookupServiceMock
            .Setup(x => x.GetAuthorInfoByIdAsync(_parentAuthorId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new AuthorDto("Fally", email, null, "Visitor"));
    }

    private void SetupReplierName(string name)
    {
        _userLookupServiceMock
            .Setup(x => x.GetUserNameByIdAsync(_replierId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(name);
    }
}
