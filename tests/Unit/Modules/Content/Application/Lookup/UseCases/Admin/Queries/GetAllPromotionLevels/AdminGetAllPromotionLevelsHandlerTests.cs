using _116.Content.Application.Lookup.UseCases.Admin.Queries.GetAllPromotionLevels;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Tests.Fixtures.Constants;
using _116.Tests.Fixtures.Factories.Content;
using _116.Unit.Tests.Common;
using _116.Unit.Tests.Common.Mocks.Repositories;
using AwesomeAssertions;
using Moq;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Lookup.UseCases.Admin.Queries.GetAllPromotionLevels;

/// <summary>
/// Unit tests for <see cref="AdminGetAllPromotionLevelsHandler"/>.
/// </summary>
public class AdminGetAllPromotionLevelsHandlerTests : BaseContentHandlerTest
{
    private readonly Mock<ILookupRepository> _lookupRepositoryMock;
    private readonly AdminGetAllPromotionLevelsHandler _handler;

    public AdminGetAllPromotionLevelsHandlerTests()
    {
        _lookupRepositoryMock = MockLookupRepository.Create();
        _handler = new AdminGetAllPromotionLevelsHandler(_lookupRepositoryMock.Object, Mapper);
    }

    #region Success Cases

    [Fact]
    public async Task Handle_WithNoSearch_ShouldReturnAllPromotionLevels()
    {
        // Arrange
        List<PromotionLevelEntity> promotionLevels = PromotionLevelFactory.CreateMany(3);
        _lookupRepositoryMock.SetupGetAllPromotionLevels(promotionLevels);

        var query = new AdminGetAllPromotionLevelsQuery(Search: null);

        // Act
        AdminGetAllPromotionLevelsResult result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.PromotionLevels.Should().HaveCount(3);
    }

    [Fact]
    public async Task Handle_WithSearchTerm_ShouldPassSearchToRepository()
    {
        // Arrange
        string searchTerm = TestConstants.Content.PromotionLevel.ValidName;
        PromotionLevelEntity level = PromotionLevelFactory.CreateDefault();
        _lookupRepositoryMock.SetupGetAllPromotionLevels(new List<PromotionLevelEntity> { level });

        var query = new AdminGetAllPromotionLevelsQuery(Search: searchTerm);

        // Act
        AdminGetAllPromotionLevelsResult result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.PromotionLevels.Should().ContainSingle();
        _lookupRepositoryMock.Verify(
            x => x.GetAllPromotionLevelsAsync(searchTerm, It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    [Fact]
    public async Task Handle_WithEmptyList_ShouldReturnEmptyList()
    {
        // Arrange
        _lookupRepositoryMock.SetupGetAllPromotionLevels(new List<PromotionLevelEntity>());

        var query = new AdminGetAllPromotionLevelsQuery();

        // Act
        AdminGetAllPromotionLevelsResult result = await _handler.Handle(query, CancellationToken.None);

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

        var query = new AdminGetAllPromotionLevelsQuery();

        // Act
        AdminGetAllPromotionLevelsResult result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.PromotionLevels.Should().ContainSingle();
        result.PromotionLevels[0].Name.Should().Be(level.Name);
    }

    #endregion
}
