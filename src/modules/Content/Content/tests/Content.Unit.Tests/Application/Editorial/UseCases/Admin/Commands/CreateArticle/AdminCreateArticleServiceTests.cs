using _116.BuildingBlocks.Application.Exceptions;
using _116.Content.Application.Editorial.UseCases.Admin.Commands.CreateArticle;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Content.TestData.Factories;
using _116.Content.TestData.Mocks.Repositories;
using _116.Tests.TestData.Helpers;
using AwesomeAssertions;
using Moq;
using Xunit;

namespace _116.Content.Unit.Tests.Application.Editorial.UseCases.Admin.Commands.CreateArticle;

/// <summary>
/// Unit tests for <see cref="AdminCreateArticleService"/>: the category and slug gates and the staged article.
/// </summary>
public class AdminCreateArticleServiceTests
{
    private readonly Mock<ICategoryRepository> _categoryRepositoryMock = MockCategoryRepository.Create();
    private readonly Mock<IArticleRepository> _articleRepositoryMock = MockArticleRepository.Create();
    private readonly AdminCreateArticleService _service;

    public AdminCreateArticleServiceTests()
    {
        _service = new AdminCreateArticleService(
            _categoryRepositoryMock.Object,
            _articleRepositoryMock.Object,
            TestErrorsFactory.CreateContentI18n()
        );
    }

    private static AdminCreateArticleCommand Command(Guid categoryId, Guid? customerId = null)
    {
        return new AdminCreateArticleCommand(
            categoryId,
            "Title",
            "slug",
            Guid.NewGuid(),
            customerId,
            customerId is null ? null : Guid.NewGuid()
        );
    }

    [Fact]
    public async Task CreateAsync_WithAFreeSlug_ShouldStageAFreeArticle()
    {
        // Arrange
        CategoryEntity category = CategoryFactory.Create(Guid.NewGuid());
        _categoryRepositoryMock.SetupGetByIdOrThrow(category);
        _articleRepositoryMock.SetupGetBySlug("slug", null);

        // Act
        ArticleEntity article = await _service.CreateAsync(Command(category.Id), CancellationToken.None);

        // Assert
        article.Slug.Value.Should().Be("slug");
        article.CustomerId.Should().BeNull();
        _articleRepositoryMock.VerifyAddCalled();
    }

    [Fact]
    public async Task CreateAsync_WithACustomer_ShouldStageAPaidArticle()
    {
        // Arrange
        CategoryEntity category = CategoryFactory.Create(Guid.NewGuid());
        Guid customerId = Guid.NewGuid();
        _categoryRepositoryMock.SetupGetByIdOrThrow(category);
        _articleRepositoryMock.SetupGetBySlug("slug", null);

        // Act
        ArticleEntity article = await _service.CreateAsync(Command(category.Id, customerId), CancellationToken.None);

        // Assert
        article.CustomerId.Should().Be(customerId);
    }

    [Fact]
    public async Task CreateAsync_WhenTheSlugIsTaken_ShouldThrowConflictException()
    {
        // Arrange
        CategoryEntity category = CategoryFactory.Create(Guid.NewGuid());
        _categoryRepositoryMock.SetupGetByIdOrThrow(category);
        _articleRepositoryMock.SetupGetBySlug("slug", ArticleFactory.Create(category.Id));

        // Act
        Func<Task> act = async () => await _service.CreateAsync(Command(category.Id), CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ConflictException>();
    }
}
