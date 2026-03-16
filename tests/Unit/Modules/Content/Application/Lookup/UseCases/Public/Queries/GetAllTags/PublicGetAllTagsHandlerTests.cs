using _116.Content.Application.Lookup.UseCases.Public.Queries.GetAllTags;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Tests.Fixtures.Constants;
using _116.Tests.Fixtures.Factories.Content;
using _116.Unit.Tests.Common;
using _116.Unit.Tests.Common.Mocks.Repositories;
using AwesomeAssertions;
using Moq;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Lookup.UseCases.Public.Queries.GetAllTags;

/// <summary>
/// Unit tests for <see cref="PublicGetAllTagsHandler"/>.
/// </summary>
public class PublicGetAllTagsHandlerTests : BaseContentHandlerTest
{
    private readonly Mock<ILookupRepository> _lookupRepositoryMock;
    private readonly PublicGetAllTagsHandler _handler;

    public PublicGetAllTagsHandlerTests()
    {
        _lookupRepositoryMock = MockLookupRepository.Create();
        _handler = new PublicGetAllTagsHandler(_lookupRepositoryMock.Object, Mapper);
    }

    #region Success Cases

    [Fact]
    public async Task Handle_WithNoSearch_ShouldReturnAllTags()
    {
        // Arrange
        List<TagEntity> tags = TagFactory.CreateMany(3);
        _lookupRepositoryMock.SetupGetAllTags(tags);

        var query = new PublicGetAllTagsQuery(Search: null);

        // Act
        PublicGetAllTagsResult result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Tags.Should().HaveCount(3);
    }

    [Fact]
    public async Task Handle_WithSearchTerm_ShouldPassSearchToRepository()
    {
        // Arrange
        string searchTerm = TestConstants.Content.Tag.ValidName;
        TagEntity tag = TagFactory.CreateDefault();
        _lookupRepositoryMock.SetupGetAllTags(new List<TagEntity> { tag });

        var query = new PublicGetAllTagsQuery(Search: searchTerm);

        // Act
        PublicGetAllTagsResult result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Tags.Should().ContainSingle();
        _lookupRepositoryMock.Verify(x => x.GetAllTagsAsync(searchTerm, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithEmptyList_ShouldReturnEmptyList()
    {
        // Arrange
        _lookupRepositoryMock.SetupGetAllTags(new List<TagEntity>());

        var query = new PublicGetAllTagsQuery();

        // Act
        PublicGetAllTagsResult result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Tags.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_WithSingleTag_ShouldReturnMappedDto()
    {
        // Arrange
        TagEntity tag = TagFactory.CreateDefault();
        _lookupRepositoryMock.SetupGetAllTags(new List<TagEntity> { tag });

        var query = new PublicGetAllTagsQuery();

        // Act
        PublicGetAllTagsResult result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Tags.Should().ContainSingle();
        result.Tags[0].Name.Should().Be(tag.Name);
        result.Tags[0].Slug.Should().Be(tag.Slug);
    }

    #endregion
}
