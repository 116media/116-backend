using _116.BuildingBlocks.Application.Exceptions;
using _116.BuildingBlocks.Application.Services;
using _116.Content.Application.Editorial.UseCases.Admin.Commands.ForceUnpromoteArticle;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Content.TestData.Factories;
using _116.Content.TestData.Mocks.Repositories;
using _116.Tests.TestData.Helpers;
using AwesomeAssertions;
using Moq;
using Xunit;

namespace _116.Content.Unit.Tests.Application.Editorial.UseCases.Admin.Commands.ForceUnpromoteArticle;

/// <summary>
/// Unit tests for <see cref="AdminForceUnpromoteArticleService"/>: the slug gate and the clocked unpromotion.
/// </summary>
public class AdminForceUnpromoteArticleServiceTests
{
    private readonly string _adminId = Guid.NewGuid().ToString();
    private readonly Mock<IArticleRepository> _articleRepositoryMock = MockArticleRepository.Create();
    private readonly AdminForceUnpromoteArticleService _service;

    public AdminForceUnpromoteArticleServiceTests()
    {
        ICurrentActor currentActor = Mock.Of<ICurrentActor>(actor => actor.UserId == _adminId);
        _service = new AdminForceUnpromoteArticleService(
            _articleRepositoryMock.Object,
            currentActor,
            TestErrorsFactory.CreateContentI18n(),
            TimeProvider.System
        );
    }

    [Fact]
    public async Task UnpromoteAsync_WithAPromotedArticle_ShouldUnpromoteItOnBehalfOfTheAdmin()
    {
        // Arrange
        ArticleEntity article = ArticleFactory.CreatePromoted(Guid.NewGuid());
        _articleRepositoryMock.SetupGetBySlug(article.Slug, article);

        // Act
        ArticleEntity result = await _service.UnpromoteAsync(article.Slug, "Policy breach", CancellationToken.None);

        // Assert
        result.Should().BeSameAs(article);
        article.UnpromotedAt.Should().NotBeNull();
        article.UnpromotedBy.Should().Be(_adminId);
    }

    [Fact]
    public async Task UnpromoteAsync_WhenTheSlugIsUnknown_ShouldThrowNotFoundException()
    {
        // Arrange
        _articleRepositoryMock.SetupGetBySlug("missing", null);

        // Act
        Func<Task> act = async () => await _service.UnpromoteAsync("missing", "Policy breach", CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }
}
