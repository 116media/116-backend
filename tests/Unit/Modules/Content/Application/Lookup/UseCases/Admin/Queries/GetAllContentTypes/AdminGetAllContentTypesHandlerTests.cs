using _116.Content.Application.Lookup.UseCases.Admin.Queries.GetAllContentTypes;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
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
    private readonly Mock<ILookupRepository> _lookupRepositoryMock;
    private readonly AdminGetAllContentTypesHandler _handler;

    public AdminGetAllContentTypesHandlerTests()
    {
        _lookupRepositoryMock = MockLookupRepository.Create();
        _handler = new AdminGetAllContentTypesHandler(_lookupRepositoryMock.Object, Mapper);
    }

    #region Success Cases

    [Fact]
    public async Task Handle_WithMultipleContentTypes_ShouldReturnAllMapped()
    {
        // Arrange
        List<ContentTypeEntity> contentTypes = ContentTypeFactory.CreateMany(3);
        _lookupRepositoryMock.SetupGetAllContentTypes(contentTypes);

        var query = new AdminGetAllContentTypesQuery();

        // Act
        AdminGetAllContentTypesResult result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.ContentTypes.Should().HaveCount(3);
    }

    [Fact]
    public async Task Handle_WithEmptyList_ShouldReturnEmptyList()
    {
        // Arrange
        _lookupRepositoryMock.SetupGetAllContentTypes(new List<ContentTypeEntity>());

        var query = new AdminGetAllContentTypesQuery();

        // Act
        AdminGetAllContentTypesResult result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.ContentTypes.Should().BeEmpty();
    }

    #endregion
}
