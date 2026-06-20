using _116.Content.Application.Shared.DTOs;
using _116.Content.Application.Shared.Mappers;
using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;
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
    private static CategoryEntity CategoryWithContentType()
    {
        ContentTypeEntity videoType = ContentTypeFactory.Create(nameof(EnumCoreContentType.Video));
        return CategoryFactory.Create(videoType);
    }

    [Fact]
    public void ToCategoryDto_WhenPosterInFileMap_ShouldResolvePosterUrl()
    {
        CategoryEntity category = CategoryWithContentType();
        FileEntity poster = FileFactory.CreateWithStorageUrl("https://cdn.116.test/posters/show.jpg");
        category.SetPosterFileId(poster.Id);

        var files = new Dictionary<Guid, FileEntity> { [poster.Id] = poster };

        CategoryDto dto = category.ToCategoryDto(Mapper, files);

        dto.PosterUrl.Should().Be("https://cdn.116.test/posters/show.jpg");
    }

    [Fact]
    public void ToCategoryDto_WhenNoPosterFileId_ShouldLeavePosterUrlNull()
    {
        CategoryEntity category = CategoryWithContentType();

        CategoryDto dto = category.ToCategoryDto(Mapper, new Dictionary<Guid, FileEntity>());

        dto.PosterUrl.Should().BeNull();
    }

    [Fact]
    public void ToCategoryDto_WhenPosterMissingFromFileMap_ShouldLeavePosterUrlNull()
    {
        CategoryEntity category = CategoryWithContentType();
        category.SetPosterFileId(Guid.NewGuid());

        CategoryDto dto = category.ToCategoryDto(Mapper, new Dictionary<Guid, FileEntity>());

        dto.PosterUrl.Should().BeNull();
    }
}
