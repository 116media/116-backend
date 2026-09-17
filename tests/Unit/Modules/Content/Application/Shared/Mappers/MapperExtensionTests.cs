using _116.Content.Application.Shared.DTOs;
using _116.Content.Application.Shared.Mappers;
using _116.Content.Domain.Entities;
using _116.Core.Application.Shared.Repositories;
using _116.Core.Contracts.Application.DTOs;
using _116.Core.Contracts.Application.Services;
using _116.Core.Domain.Entities;
using _116.Tests.Fixtures.Factories.Content;
using _116.Tests.Fixtures.Factories.Core;
using _116.Unit.Tests.Common;
using _116.Unit.Tests.Common.Mocks.Repositories;
using _116.Unit.Tests.Common.Mocks.Services;
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
        // Arrange
        PackageSlotEntity entity = PackageSlotFactory.CreateOpen(Guid.NewGuid());

        // Act
        var result = entity.ToPackageSlotDto(Mapper);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(entity.Id);
        result.CategoryName.Should().BeNull();
    }

    #endregion

    #region LyricsMapper Extensions

    [Fact]
    public async Task ToLyricsSummaryDtosAsync_WithMultipleEntities_ShouldReturnMappedList()
    {
        // Arrange
        Guid categoryId = Guid.NewGuid();
        IReadOnlyList<LyricsEntity> entities = LyricsFactory.CreateMany(categoryId, 3).AsReadOnly();
        Mock<IFileStorageService> fileStorageMock = MockFileStorageService.Create();

        // Act
        IReadOnlyList<LyricsSummaryDto> result = await entities.ToLyricsSummaryDtosAsync(fileStorageMock.Object);

        // Assert
        result.Should().HaveCount(3);
        result.Should().AllSatisfy(dto => dto.Should().NotBeNull());
    }

    [Fact]
    public async Task ToLyricsSummaryDtosAsync_WithEmptyList_ShouldReturnEmptyList()
    {
        // Arrange
        IReadOnlyList<LyricsEntity> entities = new List<LyricsEntity>().AsReadOnly();
        Mock<IFileStorageService> fileStorageMock = MockFileStorageService.Create();

        // Act
        IReadOnlyList<LyricsSummaryDto> result = await entities.ToLyricsSummaryDtosAsync(fileStorageMock.Object);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task ToLyricsSummaryDtoAsync_WithSingleEntity_ShouldMapCoreFields()
    {
        // Arrange
        Guid categoryId = Guid.NewGuid();
        LyricsEntity entity = LyricsFactory.Create(categoryId);
        Mock<IFileStorageService> fileStorageMock = MockFileStorageService.Create();

        // Act
        LyricsSummaryDto dto = await entity.ToLyricsSummaryDtoAsync(fileStorageMock.Object);

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
        Mock<IFileStorageService> fileStorageMock = MockFileStorageService.Create();
        FileReferenceDto videoFile = FileReferenceDtoFactory.CreateVideo();
        fileStorageMock.SetupResolve(videoFile);
        fileStorageMock.SetupResolveMany(new Dictionary<Guid, FileReferenceDto>());

        // Act
        IReadOnlyList<ShortVideoDto> result = await entities.ToShortVideoDtosAsync(
            Mapper,
            fileStorageMock.Object,
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
        Mock<IFileStorageService> fileStorageMock = MockFileStorageService.Create();

        // Act
        IReadOnlyList<ShortVideoDto> result = await entities.ToShortVideoDtosAsync(
            Mapper,
            fileStorageMock.Object,
            CancellationToken.None
        );

        // Assert
        result.Should().BeEmpty();
    }

    #endregion
}
