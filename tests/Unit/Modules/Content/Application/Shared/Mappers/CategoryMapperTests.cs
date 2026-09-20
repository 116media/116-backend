using _116.Content.Application.Shared.DTOs;
using _116.Content.Application.Shared.Mappers;
using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;
using _116.Core.Contracts.Application.DTOs;
using _116.Core.Domain.Entities;
using _116.Tests.Fixtures.Factories.Content;
using _116.Tests.Fixtures.Factories.Core;
using _116.Unit.Tests.Common;
using AwesomeAssertions;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Shared.Mappers;

/// <summary>
/// Unit tests for the batch, no-IO <see cref="CategoryMapper.ToCategoryDto(CategoryEntity, MapsterMapper.IMapper, IReadOnlyDictionary{Guid, FileEntity})"/> overload.
/// </summary>
public class CategoryMapperTests : BaseContentHandlerTest
{
    private static readonly ContentTypeEntity VideoType = ContentTypeFactory.Create(nameof(EnumCoreContentType.Video));

    private static CategoryEntity CategoryWithContentType() => CategoryFactory.Create(VideoType);

    /// <summary>
    /// The lookups the DTO factory hands the mapper, resolving the shared video content type.
    /// </summary>
    private static CategoryLookups Lookups(IReadOnlyDictionary<Guid, FileReferenceDto>? posters = null) =>
        new(
            Posters: posters is null ? new Dictionary<Guid, FileReferenceDto>() : posters,
            ContentTypes: new Dictionary<Guid, ContentTypeEntity> { [VideoType.Id] = VideoType },
            PricingTiers: new Dictionary<Guid, PricingTierEntity>()
        );

    [Fact]
    public void ToCategoryDto_WhenPosterInFileMap_ShouldResolvePosterUrl()
    {
        CategoryEntity category = CategoryWithContentType();
        FileReferenceDto poster = FileReferenceDtoFactory.CreateWithStorageUrl("https://cdn.116.test/posters/show.jpg");
        category.SetPosterFileId(poster.Id);

        var files = new Dictionary<Guid, FileReferenceDto> { [poster.Id] = poster };

        CategoryDto dto = category.ToCategoryDto(Mapper, Lookups(files));

        dto.PosterUrl.Should().Be("https://cdn.116.test/posters/show.jpg");
    }

    [Fact]
    public void ToCategoryDto_WhenNoPosterFileId_ShouldLeavePosterUrlNull()
    {
        CategoryEntity category = CategoryWithContentType();

        CategoryDto dto = category.ToCategoryDto(Mapper, Lookups());

        dto.PosterUrl.Should().BeNull();
    }

    [Fact]
    public void ToCategoryDto_WhenPosterMissingFromFileMap_ShouldLeavePosterUrlNull()
    {
        CategoryEntity category = CategoryWithContentType();
        category.SetPosterFileId(Guid.NewGuid());

        CategoryDto dto = category.ToCategoryDto(Mapper, Lookups());

        dto.PosterUrl.Should().BeNull();
    }

    [Fact]
    public void ToCategoryDto_WhenPosterHasColors_ShouldPassColorsThrough()
    {
        CategoryEntity category = CategoryWithContentType();
        FileReferenceDto poster = FileReferenceDtoFactory.CreateWithColors("#FFEB3B", "#000000");
        category.SetPosterFileId(poster.Id);

        var files = new Dictionary<Guid, FileReferenceDto> { [poster.Id] = poster };

        CategoryDto dto = category.ToCategoryDto(Mapper, Lookups(files));

        dto.Colors.Should().NotBeNull();
        dto.Colors!.Background.Should().Be("#FFEB3B");
        dto.Colors.Foreground.Should().Be("#000000");
    }

    [Fact]
    public void ToCategoryDto_WhenNoPosterFileId_ShouldLeaveColorsNull()
    {
        CategoryEntity category = CategoryWithContentType();

        CategoryDto dto = category.ToCategoryDto(Mapper, Lookups());

        dto.Colors.Should().BeNull();
    }

    [Fact]
    public void ToCategoryDto_WhenPosterHasNoExtractedColors_ShouldLeaveColorsNull()
    {
        CategoryEntity category = CategoryWithContentType();
        FileReferenceDto poster = FileReferenceDtoFactory.CreateWithStorageUrl("https://cdn.116.test/posters/show.jpg");
        category.SetPosterFileId(poster.Id);

        var files = new Dictionary<Guid, FileReferenceDto> { [poster.Id] = poster };

        CategoryDto dto = category.ToCategoryDto(Mapper, Lookups(files));

        dto.PosterUrl.Should().Be("https://cdn.116.test/posters/show.jpg");
        dto.Colors.Should().BeNull();
    }

    [Fact]
    public void ToCategoryDto_WhenPosterHasDominantButNoForeground_ShouldLeaveColorsNull()
    {
        CategoryEntity category = CategoryWithContentType();
        FileReferenceDto poster = FileReferenceDtoFactory.CreateWithColors("#FFEB3B", foregroundColorHex: null);
        category.SetPosterFileId(poster.Id);

        var files = new Dictionary<Guid, FileReferenceDto> { [poster.Id] = poster };

        CategoryDto dto = category.ToCategoryDto(Mapper, Lookups(files));

        // Colors are atomic: a half-populated pair resolves to null, not a partial object.
        dto.Colors.Should().BeNull();
    }
}
