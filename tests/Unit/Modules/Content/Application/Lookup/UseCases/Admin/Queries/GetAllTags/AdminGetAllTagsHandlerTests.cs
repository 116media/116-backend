using _116.Content.Application.Lookup.UseCases.Admin.Queries.GetAllTags;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Tests.Fixtures.Constants;
using _116.Tests.Fixtures.Factories.Content;
using _116.Unit.Tests.Common;
using _116.Unit.Tests.Common.Mocks.Repositories;
using AwesomeAssertions;
using Moq;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Lookup.UseCases.Admin.Queries.GetAllTags;

/// <summary>
/// Unit tests for <see cref="AdminGetAllTagsHandler"/>.
/// </summary>
public class AdminGetAllTagsHandlerTests : BaseContentHandlerTest
{
    private readonly Mock<ILookupRepository> _lookupRepositoryMock;
    private readonly AdminGetAllTagsHandler _handler;

    public AdminGetAllTagsHandlerTests()
    {
        _lookupRepositoryMock = MockLookupRepository.Create();
        _handler = new AdminGetAllTagsHandler(_lookupRepositoryMock.Object, Mapper);
    }

    #region Success Cases

    [Fact]
    public async Task Handle_WithNoSearch_ShouldReturnAllTags()
    {
        // Arrange
        List<TagEntity> tags = TagFactory.CreateMany(3);
        _lookupRepositoryMock.SetupGetAllTags(tags);

        var query = new AdminGetAllTagsQuery(Search: null);

        // Act
        AdminGetAllTagsResult result = await _handler.Handle(query, CancellationToken.None);

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

        var query = new AdminGetAllTagsQuery(Search: searchTerm);

        // Act
        AdminGetAllTagsResult result = await _handler.Handle(query, CancellationToken.None);

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

        var query = new AdminGetAllTagsQuery();

        // Act
        AdminGetAllTagsResult result = await _handler.Handle(query, CancellationToken.None);

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

        var query = new AdminGetAllTagsQuery();

        // Act
        AdminGetAllTagsResult result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Tags.Should().ContainSingle();
        result.Tags[0].Name.Should().Be(tag.Name);
        result.Tags[0].Slug.Should().Be(tag.Slug);
    }

    #endregion
}
