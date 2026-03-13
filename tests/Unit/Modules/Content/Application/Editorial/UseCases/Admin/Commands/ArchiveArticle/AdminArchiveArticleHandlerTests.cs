using _116.Content.Application.Editorial.UseCases.Admin.Commands.ArchiveArticle;
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

namespace _116.Unit.Tests.Modules.Content.Application.Editorial.UseCases.Admin.Commands.ArchiveArticle;

/// <summary>
/// Unit tests for <see cref="AdminArchiveArticleHandler"/>.
/// </summary>
public class AdminArchiveArticleHandlerTests
{
    private readonly Mock<IArticleRepository> _articleRepositoryMock;
    private readonly Mock<IContentUnitOfWork> _unitOfWorkMock;
    private readonly AdminArchiveArticleHandler _handler;

    private static readonly Guid CategoryId = Guid.NewGuid();

    public AdminArchiveArticleHandlerTests()
    {
        _articleRepositoryMock = MockArticleRepository.Create();
        _unitOfWorkMock = MockContentUnitOfWork.Create();
        _handler = new AdminArchiveArticleHandler(_articleRepositoryMock.Object, _unitOfWorkMock.Object);
    }

    #region Success Cases

    [Fact]
    public async Task Handle_WhenArticleIsPublished_ShouldArchiveAndReturnSuccess()
    {
        // Arrange
        ArticleEntity article = ArticleFactory.CreatePublished(CategoryId);
        var command = new AdminArchiveArticleCommand(Id: article.Id.ToString());
        _articleRepositoryMock.SetupGetByIdOrThrow(article);

        // Act
        AdminArchiveArticleResult result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _articleRepositoryMock.VerifyUpdateCalled();
        _unitOfWorkMock.VerifyCommitCalled();
    }

    #endregion

    #region Failure Cases

    [Fact]
    public async Task Handle_WhenArticleNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        Guid nonExistentId = Guid.NewGuid();
        var command = new AdminArchiveArticleCommand(Id: nonExistentId.ToString());
        _articleRepositoryMock.SetupGetByIdOrThrowNotFound(nonExistentId);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_WhenArticleAlreadyArchived_ShouldThrowConflictException()
    {
        // Arrange
        ArticleEntity article = ArticleFactory.CreateArchived(CategoryId);
        var command = new AdminArchiveArticleCommand(Id: article.Id.ToString());
        _articleRepositoryMock.SetupGetByIdOrThrow(article);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ConflictException>();
    }

    #endregion
}
