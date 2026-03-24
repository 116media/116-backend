using _116.Content.Application.Shared.DTOs;
using _116.Content.Application.Shared.Mappers;
using _116.Content.Domain.Entities;
using _116.Shared.Application.DTOs;
using _116.Tests.Fixtures.Factories.Content;
using _116.Unit.Tests.Common;
using AwesomeAssertions;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Shared.Mappers;

/// <summary>
/// Unit tests for <see cref="VideoMapper"/> extension methods and mapping registration.
/// </summary>
public class VideoMapperTests : BaseContentHandlerTest
{
    private static readonly Guid CategoryId = Guid.NewGuid();
    private static readonly Guid ContentTypeId = Guid.NewGuid();

    /// <summary>
    /// Creates a video entity with the Category navigation property populated via reflection,
    /// so the mapper can access Category.Name without a database.
    /// </summary>
    private static VideoEntity CreateVideoWithCategory()
    {
        VideoEntity video = VideoFactory.Create(CategoryId);
        CategoryEntity category = CategoryFactory.Create(ContentTypeId);
        video.GetType().GetProperty("Category")!.SetValue(video, category);
        return video;
    }

    #region Register

    [Fact]
    public void Register_ShouldNotThrowException()
    {
        Action act = () => MappingRegistration.CreateConfiguration();

        act.Should().NotThrow();
    }

    #endregion

    #region ToVideoDetailDto — AuditableDto inheritance

    [Fact]
    public void ToVideoDetailDto_ShouldInheritAuditableDto()
    {
        typeof(VideoDetailDto).Should().BeAssignableTo<AuditableDto>();
    }

    [Fact]
    public void ToVideoDetailDto_ShouldMapCreatedAtFromEntity()
    {
        // Arrange
        VideoEntity video = CreateVideoWithCategory();
        var expectedCreatedAt = new DateTime(2024, 3, 20, 9, 0, 0, DateTimeKind.Utc);
        video.CreatedAt = expectedCreatedAt;

        // Act
        var dto = video.ToVideoDetailDto(Mapper);

        // Assert
        dto.CreatedAt.Should().Be(expectedCreatedAt);
    }

    [Fact]
    public void ToVideoDetailDto_ShouldMapCreatedByFromEntity()
    {
        // Arrange
        VideoEntity video = CreateVideoWithCategory();
        video.CreatedBy = "admin-user-id";

        // Act
        var dto = video.ToVideoDetailDto(Mapper);

        // Assert
        dto.CreatedBy.Should().Be("admin-user-id");
    }

    [Fact]
    public void ToVideoDetailDto_ShouldMapUpdatedAtFromEntity()
    {
        // Arrange
        VideoEntity video = CreateVideoWithCategory();
        var expectedUpdatedAt = new DateTime(2025, 6, 15, 12, 30, 0, DateTimeKind.Utc);
        video.UpdatedAt = expectedUpdatedAt;

        // Act
        var dto = video.ToVideoDetailDto(Mapper);

        // Assert
        dto.UpdatedAt.Should().Be(expectedUpdatedAt);
    }

    [Fact]
    public void ToVideoDetailDto_ShouldMapUpdatedByFromEntity()
    {
        // Arrange
        VideoEntity video = CreateVideoWithCategory();
        video.UpdatedBy = "super-admin-id";

        // Act
        var dto = video.ToVideoDetailDto(Mapper);

        // Assert
        dto.UpdatedBy.Should().Be("super-admin-id");
    }

    #endregion

    #region ToVideoDetailDto — core field mapping

    [Fact]
    public void ToVideoDetailDto_ShouldMapCoreFields()
    {
        // Arrange
        VideoEntity video = CreateVideoWithCategory();

        // Act
        var dto = video.ToVideoDetailDto(Mapper);

        // Assert
        dto.Id.Should().Be(video.Id);
        dto.AuthorId.Should().Be(video.AuthorId.ToString());
        dto.Status.Should().Be(video.Status.ToString());
        dto.Title.Should().Be(video.Title);
        dto.Slug.Should().Be(video.Slug);
    }

    #endregion
}
