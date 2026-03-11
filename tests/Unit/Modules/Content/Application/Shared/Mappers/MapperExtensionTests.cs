using _116.Content.Application.Shared.DTOs;
using _116.Content.Application.Shared.Mappers;
using _116.Content.Domain.Entities;
using _116.Tests.Fixtures.Factories.Content;
using _116.Unit.Tests.Common;
using AwesomeAssertions;
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
    public void ToCategoryDtos_WithMultipleEntities_ShouldReturnMappedList()
    {
        // Arrange
        var contentTypeId = Guid.NewGuid();
        IReadOnlyList<CategoryEntity> entities = new List<CategoryEntity>
        {
            CategoryFactory.Create(contentTypeId),
            CategoryFactory.Create(contentTypeId),
        }.AsReadOnly();

        // Act
        IReadOnlyList<CategoryDto> result = entities.ToCategoryDtos(Mapper);

        // Assert
        result.Should().HaveCount(2);
        result.Should().AllSatisfy(dto => dto.Should().NotBeNull());
    }

    [Fact]
    public void ToCategoryDtos_WithEmptyList_ShouldReturnEmptyList()
    {
        // Arrange
        IReadOnlyList<CategoryEntity> entities = new List<CategoryEntity>().AsReadOnly();

        // Act
        IReadOnlyList<CategoryDto> result = entities.ToCategoryDtos(Mapper);

        // Assert
        result.Should().BeEmpty();
    }

    #endregion
}
