using _116.Content.Application.Shared.DTOs;
using _116.Content.Application.Shared.Mappers;
using _116.Content.Domain.Entities;
using _116.Core.Application.Shared.Repositories;
using _116.Core.Domain.Entities;
using _116.Tests.Fixtures.Factories.Content;
using _116.Tests.Fixtures.Factories.Core;
using _116.Unit.Tests.Common;
using _116.Unit.Tests.Common.Mocks.Repositories;
using AwesomeAssertions;
using Moq;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Shared.Mappers;

/// <summary>
/// Tests for mapper extension methods (collection and single-item mappings)
/// that are not exercised by the handler tests.
/// </summary>
public class MapperExtensionTests : BaseContentHandlerTest
{
    #region CustomerMapper Extensions

    [Fact]
    public void ToCustomerDtos_WithMultipleEntities_ShouldReturnMappedList()
    {
        // Arrange
        IReadOnlyList<CustomerEntity> entities = CustomerFactory.CreateMany(3).AsReadOnly();

        // Act
        IReadOnlyList<CustomerDto> result = entities.ToCustomerDtos(Mapper);

        // Assert
        result.Should().HaveCount(3);
        result.Should().AllSatisfy(dto => dto.Should().NotBeNull());
    }

    [Fact]
    public void ToCustomerDtos_WithEmptyList_ShouldReturnEmptyList()
    {
        // Arrange
        IReadOnlyList<CustomerEntity> entities = new List<CustomerEntity>().AsReadOnly();

        // Act
        IReadOnlyList<CustomerDto> result = entities.ToCustomerDtos(Mapper);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public void ToCustomerDto_WithEntity_ShouldMapCorrectly()
    {
        // Arrange
        CustomerEntity entity = CustomerFactory.CreateDefault();

        // Act
        var result = entity.ToCustomerDto(Mapper);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(entity.Id);
        result.FullName.Should().Be(entity.FullName);
        result.Email.Should().Be(entity.Email);
    }

    #endregion

    #region PackageMapper Extensions

    [Fact]
    public void ToPackageDto_WithEntity_ShouldMapCorrectly()
    {
        // Arrange
        PackageEntity entity = PackageFactory.Create();

        // Act
        var result = entity.ToPackageDto(Mapper);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(entity.Id);
        result.Name.Should().Be(entity.Name);
    }

    [Fact]
    public void ToPackageDtos_WithMultipleEntities_ShouldReturnMappedList()
    {
        // Arrange
        IReadOnlyList<PackageEntity> entities = new List<PackageEntity>
        {
            PackageFactory.Create("Package A"),
            PackageFactory.Create("Package B"),
        }.AsReadOnly();

        // Act
        IReadOnlyList<PackageDto> result = entities.ToPackageDtos(Mapper);

        // Assert
        result.Should().HaveCount(2);
        result.Should().AllSatisfy(dto => dto.Should().NotBeNull());
    }

    [Fact]
    public void ToPackageDtos_WithEmptyList_ShouldReturnEmptyList()
    {
        // Arrange
        IReadOnlyList<PackageEntity> entities = new List<PackageEntity>().AsReadOnly();

        // Act
        IReadOnlyList<PackageDto> result = entities.ToPackageDtos(Mapper);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public void ToPackageSlotDto_WithOpenSlot_ShouldMapWithNullCategoryName()
    {
        // Arrange — open slot has no category (categoryId = null)
        PackageSlotEntity entity = PackageSlotFactory.CreateOpen(Guid.NewGuid());

        // Act
        var result = entity.ToPackageSlotDto(Mapper);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(entity.Id);
        result.CategoryName.Should().BeNull();
    }

    #endregion

    #region CategoryMapper Extensions

    [Fact]
    public async Task ToCategoryDtosAsync_WithMultipleEntities_ShouldReturnMappedList()
    {
        // Arrange
        var contentTypeId = Guid.NewGuid();
        IReadOnlyList<CategoryEntity> entities = new List<CategoryEntity>
        {
            CategoryFactory.Create(contentTypeId),
            CategoryFactory.Create(contentTypeId),
        }.AsReadOnly();
        Mock<IFileRepository> fileRepositoryMock = MockFileRepository.Create();

        // Act
        IReadOnlyList<CategoryDto> result = await entities.ToCategoryDtosAsync(
            Mapper,
            fileRepositoryMock.Object,
            CancellationToken.None
        );

        // Assert
        result.Should().HaveCount(2);
        result.Should().AllSatisfy(dto => dto.Should().NotBeNull());
    }

    [Fact]
    public async Task ToCategoryDtosAsync_WithEmptyList_ShouldReturnEmptyList()
    {
        // Arrange
        IReadOnlyList<CategoryEntity> entities = new List<CategoryEntity>().AsReadOnly();
        Mock<IFileRepository> fileRepositoryMock = MockFileRepository.Create();

        // Act
        IReadOnlyList<CategoryDto> result = await entities.ToCategoryDtosAsync(
            Mapper,
            fileRepositoryMock.Object,
            CancellationToken.None
        );

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task ToCategoryDtoAsync_WithPosterFileId_ShouldResolvePosterUrl()
    {
        // Arrange
        var contentTypeId = Guid.NewGuid();
        CategoryEntity entity = CategoryFactory.Create(contentTypeId);
        FileEntity posterFile = FileFactory.CreateWithStorageUrl("https://cloudinary.com/poster.jpg");
        entity.SetPosterFileId(posterFile.Id);

        Mock<IFileRepository> fileRepositoryMock = MockFileRepository.Create();
        fileRepositoryMock.SetupGetById(posterFile);

        // Act
        CategoryDto result = await entity.ToCategoryDtoAsync(Mapper, fileRepositoryMock.Object, CancellationToken.None);

        // Assert
        result.PosterUrl.Should().Be("https://cloudinary.com/poster.jpg");
    }

    [Fact]
    public async Task ToCategoryDtoAsync_WithNoPosterFileId_ShouldReturnNullPosterUrl()
    {
        // Arrange
        var contentTypeId = Guid.NewGuid();
        CategoryEntity entity = CategoryFactory.Create(contentTypeId);
        Mock<IFileRepository> fileRepositoryMock = MockFileRepository.Create();

        // Act
        CategoryDto result = await entity.ToCategoryDtoAsync(Mapper, fileRepositoryMock.Object, CancellationToken.None);

        // Assert
        result.PosterUrl.Should().BeNull();
    }

    [Fact]
    public async Task ToCategoryDtoAsync_ShouldMapIsExclusive()
    {
        // Arrange
        var contentTypeId = Guid.NewGuid();
        CategoryEntity entity = CategoryFactory.Create(contentTypeId);
        entity.SetExclusive();
        Mock<IFileRepository> fileRepositoryMock = MockFileRepository.Create();

        // Act
        CategoryDto result = await entity.ToCategoryDtoAsync(Mapper, fileRepositoryMock.Object, CancellationToken.None);

        // Assert
        result.IsExclusive.Should().BeTrue();
    }

    [Fact]
    public async Task ToCategoryDtosAsync_ShouldResolveAllPosterUrls()
    {
        // Arrange
        var contentTypeId = Guid.NewGuid();
        CategoryEntity entity1 = CategoryFactory.Create(contentTypeId);
        CategoryEntity entity2 = CategoryFactory.Create(contentTypeId);
        FileEntity posterFile1 = FileFactory.CreateWithStorageUrl("https://cloudinary.com/poster1.jpg");
        FileEntity posterFile2 = FileFactory.CreateWithStorageUrl("https://cloudinary.com/poster2.jpg");
        entity1.SetPosterFileId(posterFile1.Id);
        entity2.SetPosterFileId(posterFile2.Id);

        IReadOnlyList<CategoryEntity> entities = new List<CategoryEntity> { entity1, entity2 }.AsReadOnly();
        Mock<IFileRepository> fileRepositoryMock = MockFileRepository.Create();
        fileRepositoryMock.SetupGetById(posterFile1);
        fileRepositoryMock.SetupGetById(posterFile2);

        // Act
        IReadOnlyList<CategoryDto> result = await entities.ToCategoryDtosAsync(
            Mapper,
            fileRepositoryMock.Object,
            CancellationToken.None
        );

        // Assert
        result.Should().HaveCount(2);
        result[0].PosterUrl.Should().Be("https://cloudinary.com/poster1.jpg");
        result[1].PosterUrl.Should().Be("https://cloudinary.com/poster2.jpg");
    }

    #endregion

    #region LyricsMapper Extensions

    [Fact]
    public async Task ToLyricsSummaryDtosAsync_WithMultipleEntities_ShouldReturnMappedList()
    {
        // Arrange
        Guid categoryId = Guid.NewGuid();
        IReadOnlyList<LyricsEntity> entities = LyricsFactory.CreateMany(categoryId, 3).AsReadOnly();
        Mock<IFileRepository> fileRepositoryMock = MockFileRepository.Create();

        // Act
        IReadOnlyList<LyricsSummaryDto> result = await entities.ToLyricsSummaryDtosAsync(fileRepositoryMock.Object);

        // Assert
        result.Should().HaveCount(3);
        result.Should().AllSatisfy(dto => dto.Should().NotBeNull());
    }

    [Fact]
    public async Task ToLyricsSummaryDtosAsync_WithEmptyList_ShouldReturnEmptyList()
    {
        // Arrange
        IReadOnlyList<LyricsEntity> entities = new List<LyricsEntity>().AsReadOnly();
        Mock<IFileRepository> fileRepositoryMock = MockFileRepository.Create();

        // Act
        IReadOnlyList<LyricsSummaryDto> result = await entities.ToLyricsSummaryDtosAsync(fileRepositoryMock.Object);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task ToLyricsSummaryDtoAsync_WithSingleEntity_ShouldMapCoreFields()
    {
        // Arrange
        Guid categoryId = Guid.NewGuid();
        LyricsEntity entity = LyricsFactory.Create(categoryId);
        Mock<IFileRepository> fileRepositoryMock = MockFileRepository.Create();

        // Act
        LyricsSummaryDto dto = await entity.ToLyricsSummaryDtoAsync(fileRepositoryMock.Object);

        // Assert
        dto.Id.Should().Be(entity.Id);
        dto.CategoryId.Should().Be(entity.CategoryId);
        dto.SongTitle.Should().Be(entity.SongTitle);
        dto.ArtistName.Should().Be(entity.ArtistName);
        dto.Slug.Should().Be(entity.Slug);
        dto.Language.Should().Be(entity.Language);
        dto.Status.Should().Be(entity.Status);
    }

    #endregion

    #region ShortVideoMapper Extensions

    [Fact]
    public async Task ToShortVideoDtosAsync_WithMultipleEntities_ShouldReturnMappedList()
    {
        // Arrange
        IReadOnlyList<ShortVideoEntity> entities = ShortVideoFactory.CreateMany(3).AsReadOnly();
        Mock<IFileRepository> fileRepositoryMock = MockFileRepository.Create();
        FileEntity videoFile = FileFactory.CreateVideo();
        fileRepositoryMock.SetupGetById(videoFile);

        // Act
        IReadOnlyList<ShortVideoDto> result = await entities.ToShortVideoDtosAsync(
            Mapper,
            fileRepositoryMock.Object,
            CancellationToken.None
        );

        // Assert
        result.Should().HaveCount(3);
        result.Should().AllSatisfy(dto => dto.Should().NotBeNull());
    }

    [Fact]
    public async Task ToShortVideoDtosAsync_WithEmptyList_ShouldReturnEmptyList()
    {
        // Arrange
        IReadOnlyList<ShortVideoEntity> entities = new List<ShortVideoEntity>().AsReadOnly();
        Mock<IFileRepository> fileRepositoryMock = MockFileRepository.Create();

        // Act
        IReadOnlyList<ShortVideoDto> result = await entities.ToShortVideoDtosAsync(
            Mapper,
            fileRepositoryMock.Object,
            CancellationToken.None
        );

        // Assert
        result.Should().BeEmpty();
    }

    #endregion
}
