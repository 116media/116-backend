using _116.Content.Application.Catalog.Factories;
using _116.Content.Application.Shared.DTOs;
using _116.Content.Domain.Entities;
using _116.Core.Contracts.Application.DTOs;
using _116.Core.Contracts.Application.Services;
using _116.Tests.Fixtures.Factories.Content;
using _116.Tests.Fixtures.Factories.Core;
using _116.Unit.Tests.Common;
using _116.Unit.Tests.Common.Mocks.Services;
using AwesomeAssertions;
using Moq;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Catalog.Factories;

/// <summary>
/// Unit tests for <see cref="CategoryDtoFactory"/>, which owns the resolve-then-map step the
/// category projections used to perform inside the mapper.
/// </summary>
public class CategoryDtoFactoryTests : BaseContentHandlerTest
{
    private readonly Mock<IFileStorageService> _fileStorageMock = MockFileStorageService.Create();

    /// <summary>
    /// Builds the factory under test over the shared mapper, the mocked storage contract and
    /// mocked lookup repositories.
    /// </summary>
    /// <returns>The factory.</returns>
    private ICategoryDtoFactory CreateFactory() => CreateCategoryDtoFactory(_fileStorageMock.Object);

    [Fact]
    public async Task CreateManyAsync_WithMultipleEntities_ShouldReturnMappedList()
    {
        // Arrange
        var contentTypeId = Guid.NewGuid();
        IReadOnlyList<CategoryEntity> entities = new List<CategoryEntity>
        {
            CategoryFactory.Create(contentTypeId),
            CategoryFactory.Create(contentTypeId),
        }.AsReadOnly();

        // Act
        IReadOnlyList<CategoryDto> result = await CreateFactory().CreateManyAsync(entities, CancellationToken.None);

        // Assert
        result.Should().HaveCount(2);
        result.Should().AllSatisfy(dto => dto.Should().NotBeNull());
    }

    [Fact]
    public async Task CreateManyAsync_WithEmptyList_ShouldReturnEmptyList()
    {
        // Arrange
        IReadOnlyList<CategoryEntity> entities = new List<CategoryEntity>().AsReadOnly();

        // Act
        IReadOnlyList<CategoryDto> result = await CreateFactory().CreateManyAsync(entities, CancellationToken.None);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task CreateAsync_WithPosterFileId_ShouldResolvePosterUrl()
    {
        // Arrange
        var contentTypeId = Guid.NewGuid();
        CategoryEntity entity = CategoryFactory.Create(contentTypeId);
        FileReferenceDto posterFile = FileReferenceDtoFactory.CreateWithStorageUrl("https://cloudinary.com/poster.jpg");
        entity.SetPosterFileId(posterFile.Id);

        _fileStorageMock.SetupResolve(posterFile);

        // Act
        CategoryDto result = await CreateFactory().CreateAsync(entity, CancellationToken.None);

        // Assert
        result.PosterUrl.Should().Be("https://cloudinary.com/poster.jpg");
    }

    [Fact]
    public async Task CreateAsync_WithNoPosterFileId_ShouldReturnNullPosterUrl()
    {
        // Arrange
        var contentTypeId = Guid.NewGuid();
        CategoryEntity entity = CategoryFactory.Create(contentTypeId);

        // Act
        CategoryDto result = await CreateFactory().CreateAsync(entity, CancellationToken.None);

        // Assert
        result.PosterUrl.Should().BeNull();
    }

    [Fact]
    public async Task CreateAsync_ShouldMapIsExclusive()
    {
        // Arrange
        var contentTypeId = Guid.NewGuid();
        CategoryEntity entity = CategoryFactory.Create(contentTypeId);
        entity.SetExclusive();

        // Act
        CategoryDto result = await CreateFactory().CreateAsync(entity, CancellationToken.None);

        // Assert
        result.IsExclusive.Should().BeTrue();
    }

    [Fact]
    public async Task CreateManyAsync_ShouldResolveEveryPosterInOneQuery()
    {
        // Arrange
        var contentTypeId = Guid.NewGuid();
        CategoryEntity entity1 = CategoryFactory.Create(contentTypeId);
        CategoryEntity entity2 = CategoryFactory.Create(contentTypeId);
        FileReferenceDto posterFile1 = FileReferenceDtoFactory.CreateWithStorageUrl(
            "https://cloudinary.com/poster1.jpg"
        );
        FileReferenceDto posterFile2 = FileReferenceDtoFactory.CreateWithStorageUrl(
            "https://cloudinary.com/poster2.jpg"
        );
        entity1.SetPosterFileId(posterFile1.Id);
        entity2.SetPosterFileId(posterFile2.Id);

        IReadOnlyList<CategoryEntity> entities = new List<CategoryEntity> { entity1, entity2 }.AsReadOnly();
        _fileStorageMock.SetupResolve(posterFile1);
        _fileStorageMock.SetupResolve(posterFile2);

        // Act
        IReadOnlyList<CategoryDto> result = await CreateFactory().CreateManyAsync(entities, CancellationToken.None);

        // Assert
        result.Should().HaveCount(2);
        result[0].PosterUrl.Should().Be("https://cloudinary.com/poster1.jpg");
        result[1].PosterUrl.Should().Be("https://cloudinary.com/poster2.jpg");
        _fileStorageMock.Verify(
            x => x.ResolveManyAsync(It.IsAny<IReadOnlyCollection<Guid>>(), It.IsAny<CancellationToken>()),
            Times.Once
        );
    }
}
