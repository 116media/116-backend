using _116.Content.Application.Lookup.UseCases.Admin.Queries.GetAllTags;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;
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
    private readonly Mock<ITagRepository> _tagRepositoryMock;
    private readonly AdminGetAllTagsHandler _handler;

    public AdminGetAllTagsHandlerTests()
    {
        _tagRepositoryMock = MockTagRepository.Create();
        _handler = new AdminGetAllTagsHandler(_tagRepositoryMock.Object, Mapper);
    }

    #region Success Cases

    [Fact]
    public async Task Handle_WithNoSearch_ShouldReturnAllTags()
    {
        // Arrange
        List<TagEntity> tags = TagFactory.CreateMany(3);
        _tagRepositoryMock.SetupGetAllTags(tags);

        var query = new AdminGetAllTagsQuery(Search: null);

        // Act
        AdminGetAllTagsResult result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Tags.Should().HaveCount(3);
    }

    [Fact]
    public async Task Handle_WithSearchTerm_ShouldPassSearchToRepository()
    {
        // Arrange
        string searchTerm = TestConstants.Tag.ValidName;
        TagEntity tag = TagFactory.CreateDefault();
        _tagRepositoryMock.SetupGetAllTags(new List<TagEntity> { tag });

        var query = new AdminGetAllTagsQuery(Search: searchTerm);

        // Act
        AdminGetAllTagsResult result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Tags.Should().ContainSingle();
        _tagRepositoryMock.Verify(
            x =>
                x.GetAllAsync(
                    searchTerm,
                    It.IsAny<EnumCoreContentType?>(),
                    It.IsAny<int?>(),
                    It.IsAny<CancellationToken>()
                ),
            Times.Once
        );
    }

    [Fact]
    public async Task Handle_WithEmptyList_ShouldReturnEmptyList()
    {
        // Arrange
        _tagRepositoryMock.SetupGetAllTags(new List<TagEntity>());

        var query = new AdminGetAllTagsQuery();

        // Act
        AdminGetAllTagsResult result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Tags.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_WithSingleTag_ShouldReturnMappedDto()
    {
        // Arrange
        TagEntity tag = TagFactory.CreateDefault();
        _tagRepositoryMock.SetupGetAllTags(new List<TagEntity> { tag });

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
