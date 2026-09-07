using _116.Content.Application.Lookup.UseCases.Public.Queries.GetAllContentTypes;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Tests.Fixtures.Constants;
using _116.Tests.Fixtures.Factories.Content;
using _116.Unit.Tests.Common;
using _116.Unit.Tests.Common.Mocks.Repositories;
using AwesomeAssertions;
using Moq;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Lookup.UseCases.Public.Queries.GetAllContentTypes;

/// <summary>
/// Unit tests for <see cref="PublicGetAllContentTypesHandler"/>.
/// </summary>
public class PublicGetAllContentTypesHandlerTests : BaseContentHandlerTest
{
    private readonly Mock<IContentTypeRepository> _contentTypeRepositoryMock;
    private readonly PublicGetAllContentTypesHandler _handler;

    public PublicGetAllContentTypesHandlerTests()
    {
        _contentTypeRepositoryMock = MockContentTypeRepository.Create();
        _handler = new PublicGetAllContentTypesHandler(_contentTypeRepositoryMock.Object, Mapper);
    }

    #region Success Cases

    [Fact]
    public async Task Handle_ShouldReturnOnlyActiveContentTypes()
    {
        // Arrange
        List<ContentTypeEntity> activeList = ContentTypeFactory.CreateMany(3);
        _contentTypeRepositoryMock.SetupGetActiveContentTypes(activeList);

        var query = new PublicGetAllContentTypesQuery();

        // Act
        PublicGetAllContentTypesResult result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.ContentTypes.Should().HaveCount(3);
    }

    [Fact]
    public async Task Handle_WithEmptyList_ShouldReturnEmptyList()
    {
        // Arrange
        _contentTypeRepositoryMock.SetupGetActiveContentTypes(new List<ContentTypeEntity>());

        var query = new PublicGetAllContentTypesQuery();

        // Act
        PublicGetAllContentTypesResult result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.ContentTypes.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_ShouldCallGetActiveContentTypesOnce()
    {
        // Arrange
        _contentTypeRepositoryMock.SetupGetActiveContentTypes(new List<ContentTypeEntity>());

        var query = new PublicGetAllContentTypesQuery();

        // Act
        await _handler.Handle(query, CancellationToken.None);

        // Assert
        _contentTypeRepositoryMock.Verify(x => x.GetActiveAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithSingleContentType_ShouldReturnMappedDto()
    {
        // Arrange
        ContentTypeEntity contentType = ContentTypeFactory.CreateDefault();
        _contentTypeRepositoryMock.SetupGetActiveContentTypes(new List<ContentTypeEntity> { contentType });

        var query = new PublicGetAllContentTypesQuery();

        // Act
        PublicGetAllContentTypesResult result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.ContentTypes.Should().ContainSingle();
        result.ContentTypes[0].Name.Should().Be(TestConstants.ContentType.ValidName);
    }

    #endregion
}
