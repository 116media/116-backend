using _116.BuildingBlocks.Application.Exceptions;
using _116.Content.Application.Editorial.UseCases.Admin.Commands.UpdateArticle;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Content.TestData.Factories;
using _116.Content.TestData.Mocks.Repositories;
using _116.Tests.TestData.Helpers;
using AwesomeAssertions;
using Moq;
using Xunit;

namespace _116.Content.Unit.Tests.Application.Editorial.UseCases.Admin.Commands.UpdateArticle;

/// <summary>
/// Unit tests for <see cref="AdminUpdateArticleService"/>: the category and slug gates and the verbs applied.
/// </summary>
public class AdminUpdateArticleServiceTests
{
    private readonly Mock<ICategoryRepository> _categoryRepositoryMock = MockCategoryRepository.Create();
    private readonly Mock<IArticleRepository> _articleRepositoryMock = MockArticleRepository.Create();
    private readonly AdminUpdateArticleService _service;

    public AdminUpdateArticleServiceTests()
    {
        _service = new AdminUpdateArticleService(
            _categoryRepositoryMock.Object,
            _articleRepositoryMock.Object,
            TestErrorsFactory.CreateContentI18n()
        );
    }

    private static AdminUpdateArticleCommand Command(ArticleEntity article, string slug)
    {
        return new AdminUpdateArticleCommand(
            article.Id.ToString(),
            article.CategoryId,
            "New title",
            slug,
            "Headline",
            "<p>Body</p>",
            null,
            null,
            false,
            null,
            null
        );
    }

    [Fact]
    public async Task UpdateAsync_ShouldApplyTheVerbs()
    {
        // Arrange
        CategoryEntity category = CategoryFactory.Create(Guid.NewGuid());
        ArticleEntity article = ArticleFactory.CreateWithSlug(category.Id, "old-slug");
        _articleRepositoryMock.SetupGetByIdOrThrow(article);
        _categoryRepositoryMock.SetupGetByIdOrThrow(category);
        _articleRepositoryMock.SetupGetBySlug("new-slug", null);

        // Act
        ArticleEntity result = await _service.UpdateAsync(Command(article, "new-slug"), CancellationToken.None);

        // Assert
        result.Should().BeSameAs(article);
        article.Title.Should().Be("New title");
        article.Slug.Value.Should().Be("new-slug");
    }

    [Fact]
    public async Task UpdateAsync_WhenTheNewSlugBelongsToAnotherArticle_ShouldThrowConflictException()
    {
        // Arrange
        CategoryEntity category = CategoryFactory.Create(Guid.NewGuid());
        ArticleEntity article = ArticleFactory.CreateWithSlug(category.Id, "old-slug");
        _articleRepositoryMock.SetupGetByIdOrThrow(article);
        _categoryRepositoryMock.SetupGetByIdOrThrow(category);
        _articleRepositoryMock.SetupGetBySlug("new-slug", ArticleFactory.CreateWithSlug(category.Id, "new-slug"));

        // Act
        Func<Task> act = async () => await _service.UpdateAsync(Command(article, "new-slug"), CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ConflictException>();
    }
}
