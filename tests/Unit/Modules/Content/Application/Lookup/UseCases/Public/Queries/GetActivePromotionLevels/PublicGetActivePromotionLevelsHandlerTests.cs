using _116.Content.Application.Lookup.UseCases.Public.Queries.GetActivePromotionLevels;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Tests.Fixtures.Factories.Content;
using _116.Unit.Tests.Common;
using _116.Unit.Tests.Common.Mocks.Repositories;
using AwesomeAssertions;
using Moq;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Lookup.UseCases.Public.Queries.GetActivePromotionLevels;

/// <summary>
/// Unit tests for <see cref="PublicGetActivePromotionLevelsHandler"/>.
/// </summary>
public class PublicGetActivePromotionLevelsHandlerTests : BaseContentHandlerTest
{
    private readonly Mock<ILookupRepository> _lookupRepositoryMock;
    private readonly PublicGetActivePromotionLevelsHandler _handler;

    public PublicGetActivePromotionLevelsHandlerTests()
    {
        _lookupRepositoryMock = MockLookupRepository.Create();
        _handler = new PublicGetActivePromotionLevelsHandler(_lookupRepositoryMock.Object, Mapper);
    }

    #region Success Cases

    [Fact]
    public async Task Handle_ShouldReturnOnlyActivePromotionLevels()
    {
        // Arrange
        List<PromotionLevelEntity> activeList = PromotionLevelFactory.CreateMany(2);
        _lookupRepositoryMock.SetupGetActivePromotionLevels(activeList);

        var query = new PublicGetActivePromotionLevelsQuery();

        // Act
        PublicGetActivePromotionLevelsResult result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.PromotionLevels.Should().HaveCount(2);
    }

    [Fact]
    public async Task Handle_WithEmptyList_ShouldReturnEmptyList()
    {
        // Arrange
        _lookupRepositoryMock.SetupGetActivePromotionLevels(new List<PromotionLevelEntity>());

        var query = new PublicGetActivePromotionLevelsQuery();

        // Act
        PublicGetActivePromotionLevelsResult result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.PromotionLevels.Should().BeEmpty();
    }

    #endregion
}
