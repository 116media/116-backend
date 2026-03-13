using _116.Content.Application.Editorial.UseCases.Admin.Commands.UpdateArticleSeo;
using _116.Content.Application.Shared.Persistence;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Shared.Application.Exceptions;
using _116.Tests.Fixtures.Factories.Content;
using _116.Unit.Tests.Common;
using _116.Unit.Tests.Common.Mocks.Infrastructure;
using _116.Unit.Tests.Common.Mocks.Repositories;
using AwesomeAssertions;
using Moq;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Editorial.UseCases.Admin.Commands.UpdateArticleSeo;

/// <summary>
/// Unit tests for <see cref="AdminUpdateArticleSeoHandler"/>.
/// </summary>
public class AdminUpdateArticleSeoHandlerTests : BaseContentHandlerTest
{
    private readonly Mock<IArticleRepository> _articleRepositoryMock;
    private readonly Mock<IContentUnitOfWork> _unitOfWorkMock;
    private readonly AdminUpdateArticleSeoHandler _handler;

    private static readonly Guid CategoryId = Guid.NewGuid();

    public AdminUpdateArticleSeoHandlerTests()
    {
        _articleRepositoryMock = MockArticleRepository.Create();
        _unitOfWorkMock = MockContentUnitOfWork.Create();
        _handler = new AdminUpdateArticleSeoHandler(_articleRepositoryMock.Object, _unitOfWorkMock.Object, Mapper);
    }

    #region Success Cases

    [Fact]
    public async Task Handle_WhenValidRequest_ShouldUpdateSeoAndReturnArticle()
    {
        // Arrange
        ArticleEntity article = ArticleFactory.Create(CategoryId);
        var command = new AdminUpdateArticleSeoCommand(
            Id: article.Id.ToString(),
            MetaTitle: "Custom SEO Title",
            MetaDescription: "Custom SEO description for testing."
        );

        _articleRepositoryMock.SetupGetByIdOrThrow(article);
        _articleRepositoryMock
            .Setup(x => x.GetByIdOrThrowAsync(article.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(article);

        // Act
        AdminUpdateArticleSeoResult result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Article.Should().NotBeNull();
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
        var command = new AdminUpdateArticleSeoCommand(
            Id: nonExistentId.ToString(),
            MetaTitle: null,
            MetaDescription: null
        );
        _articleRepositoryMock.SetupGetByIdOrThrowNotFound(nonExistentId);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    #endregion
}
