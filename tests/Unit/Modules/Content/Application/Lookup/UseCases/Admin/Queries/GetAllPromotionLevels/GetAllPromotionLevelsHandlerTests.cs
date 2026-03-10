using _116.Content.Application.Lookup.UseCases.Admin.Queries.GetAllPromotionLevels;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Tests.Fixtures.Factories.Content;
using _116.Unit.Tests.Common;
using _116.Unit.Tests.Common.Mocks.Repositories;
using AwesomeAssertions;
using Moq;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Lookup.UseCases.Admin.Queries.GetAllPromotionLevels;

/// <summary>
/// Unit tests for <see cref="GetAllPromotionLevelsHandler"/>.
/// </summary>
public class GetAllPromotionLevelsHandlerTests : BaseContentHandlerTest
{
    private readonly Mock<ILookupRepository> _lookupRepositoryMock;
    private readonly GetAllPromotionLevelsHandler _handler;

    public GetAllPromotionLevelsHandlerTests()
    {
        _lookupRepositoryMock = MockLookupRepository.Create();
        _handler = new GetAllPromotionLevelsHandler(_lookupRepositoryMock.Object, Mapper);
    }

    #region Success Cases

    [Fact]
    public async Task Handle_WithMultiplePromotionLevels_ShouldReturnAllMapped()
    {
        // Arrange
        List<PromotionLevelEntity> promotionLevels = PromotionLevelFactory.CreateMany(3);
        _lookupRepositoryMock.SetupGetAllPromotionLevels(promotionLevels);

        var query = new GetAllPromotionLevelsQuery();

        // Act
        GetAllPromotionLevelsResult result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.PromotionLevels.Should().HaveCount(3);
    }

    [Fact]
    public async Task Handle_WithEmptyList_ShouldReturnEmptyList()
    {
        // Arrange
        _lookupRepositoryMock.SetupGetAllPromotionLevels(new List<PromotionLevelEntity>());

        var query = new GetAllPromotionLevelsQuery();

        // Act
        GetAllPromotionLevelsResult result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.PromotionLevels.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_WithSinglePromotionLevel_ShouldReturnMappedDto()
    {
        // Arrange
        PromotionLevelEntity level = PromotionLevelFactory.CreateDefault();
        _lookupRepositoryMock.SetupGetAllPromotionLevels(new List<PromotionLevelEntity> { level });

        var query = new GetAllPromotionLevelsQuery();

        // Act
        GetAllPromotionLevelsResult result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.PromotionLevels.Should().HaveCount(1);
        result.PromotionLevels[0].Name.Should().Be(level.Name);
    }

    #endregion
}
