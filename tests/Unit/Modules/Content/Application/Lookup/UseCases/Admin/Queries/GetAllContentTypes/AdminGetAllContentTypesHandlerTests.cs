using _116.Content.Application.Lookup.UseCases.Admin.Queries.GetAllContentTypes;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Tests.Fixtures.Constants;
using _116.Tests.Fixtures.Factories.Content;
using _116.Unit.Tests.Common;
using _116.Unit.Tests.Common.Mocks.Repositories;
using AwesomeAssertions;
using Moq;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Lookup.UseCases.Admin.Queries.GetAllContentTypes;

/// <summary>
/// Unit tests for <see cref="AdminGetAllContentTypesHandler"/>.
/// </summary>
public class AdminGetAllContentTypesHandlerTests : BaseContentHandlerTest
{
    private readonly Mock<IContentTypeRepository> _contentTypeRepositoryMock;
    private readonly AdminGetAllContentTypesHandler _handler;

    public AdminGetAllContentTypesHandlerTests()
    {
        _contentTypeRepositoryMock = MockContentTypeRepository.Create();
        _handler = new AdminGetAllContentTypesHandler(_contentTypeRepositoryMock.Object, Mapper);
    }

    #region Success Cases

    [Fact]
    public async Task Handle_WithNoSearch_ShouldReturnAllContentTypes()
    {
        // Arrange
        List<ContentTypeEntity> contentTypes = ContentTypeFactory.CreateMany(3);
        _contentTypeRepositoryMock.SetupGetAllContentTypes(contentTypes);

        var query = new AdminGetAllContentTypesQuery(Search: null);

        // Act
        AdminGetAllContentTypesResult result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.ContentTypes.Should().HaveCount(3);
    }

    [Fact]
    public async Task Handle_WithSearchTerm_ShouldPassSearchToRepository()
    {
        // Arrange
        string searchTerm = TestConstants.ContentType.ValidName;
        ContentTypeEntity contentType = ContentTypeFactory.CreateDefault();
        _contentTypeRepositoryMock.SetupGetAllContentTypes(new List<ContentTypeEntity> { contentType });

        var query = new AdminGetAllContentTypesQuery(Search: searchTerm);

        // Act
        AdminGetAllContentTypesResult result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.ContentTypes.Should().ContainSingle();
        _contentTypeRepositoryMock.Verify(x => x.GetAllAsync(searchTerm, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithEmptyList_ShouldReturnEmptyList()
    {
        // Arrange
        _contentTypeRepositoryMock.SetupGetAllContentTypes(new List<ContentTypeEntity>());

        var query = new AdminGetAllContentTypesQuery();

        // Act
        AdminGetAllContentTypesResult result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.ContentTypes.Should().BeEmpty();
    }

    #endregion
}
