using _116.Content.Application.Lookup.UseCases.Admin.Commands.UpdatePromotionLevel;
using _116.Content.Application.Shared.Persistence;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Shared.Application.Exceptions;
using _116.Tests.Fixtures.Constants;
using _116.Tests.Fixtures.Factories.Content;
using _116.Unit.Tests.Common;
using _116.Unit.Tests.Common.Mocks.Infrastructure;
using _116.Unit.Tests.Common.Mocks.Repositories;
using AwesomeAssertions;
using Moq;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Lookup.UseCases.Admin.Commands.UpdatePromotionLevel;

/// <summary>
/// Unit tests for <see cref="AdminUpdatePromotionLevelHandler"/>.
/// </summary>
public class AdminUpdatePromotionLevelHandlerTests : BaseContentHandlerTest
{
    private readonly Mock<ILookupRepository> _lookupRepositoryMock;
    private readonly Mock<IContentUnitOfWork> _unitOfWorkMock;
    private readonly AdminUpdatePromotionLevelHandler _handler;

    public AdminUpdatePromotionLevelHandlerTests()
    {
        _lookupRepositoryMock = MockLookupRepository.Create();
        _unitOfWorkMock = MockContentUnitOfWork.Create();
        _handler = new AdminUpdatePromotionLevelHandler(_lookupRepositoryMock.Object, _unitOfWorkMock.Object, Mapper);
    }

    #region Success Cases

    [Fact]
    public async Task Handle_WithNewName_ShouldUpdateAndReturnDto()
    {
        // Arrange
        PromotionLevelEntity existing = PromotionLevelFactory.CreateDefault();
        string newName = TestConstants.Content.PromotionLevel.AnotherValidName;
        int newDuration = 14;
        decimal newPrice = 99.99m;
        var command = new AdminUpdatePromotionLevelCommand(
            Id: existing.Id.ToString(),
            Name: newName,
            DurationDays: newDuration,
            PriceUsd: newPrice
        );

        _lookupRepositoryMock.SetupGetPromotionLevelByIdOrThrow(existing);
        _lookupRepositoryMock.SetupPromotionLevelExistsByName(newName, false);

        // Act
        AdminUpdatePromotionLevelResult result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.PromotionLevel.Name.Should().Be(newName);
        result.PromotionLevel.DurationDays.Should().Be(newDuration);
        result.PromotionLevel.PriceUsd.Should().Be(newPrice);
        _unitOfWorkMock.VerifyCommitCalled();
    }

    [Fact]
    public async Task Handle_WithSameName_ShouldAllowUpdateDespiteConflict()
    {
        // Arrange
        string sameName = TestConstants.Content.PromotionLevel.ValidName;
        PromotionLevelEntity existing = PromotionLevelFactory.Create(sameName, 7, 50m);
        var command = new AdminUpdatePromotionLevelCommand(
            Id: existing.Id.ToString(),
            Name: sameName,
            DurationDays: 10,
            PriceUsd: 60m
        );

        _lookupRepositoryMock.SetupGetPromotionLevelByIdOrThrow(existing);
        _lookupRepositoryMock.SetupPromotionLevelExistsByName(sameName, true);

        // Act
        AdminUpdatePromotionLevelResult result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.PromotionLevel.Name.Should().Be(sameName);
        _unitOfWorkMock.VerifyCommitCalled();
    }

    #endregion

    #region Failure Cases

    [Fact]
    public async Task Handle_WhenNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();
        var command = new AdminUpdatePromotionLevelCommand(
            Id: nonExistentId.ToString(),
            Name: TestConstants.Content.PromotionLevel.ValidName,
            DurationDays: 7,
            PriceUsd: 50m
        );

        _lookupRepositoryMock.SetupGetPromotionLevelByIdOrThrowNotFound(nonExistentId);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_WhenNameConflictWithDifferentEntity_ShouldThrowConflictException()
    {
        // Arrange
        PromotionLevelEntity existing = PromotionLevelFactory.CreateDefault();
        string conflictingName = TestConstants.Content.PromotionLevel.AnotherValidName;
        var command = new AdminUpdatePromotionLevelCommand(
            Id: existing.Id.ToString(),
            Name: conflictingName,
            DurationDays: 14,
            PriceUsd: 99.99m
        );

        _lookupRepositoryMock.SetupGetPromotionLevelByIdOrThrow(existing);
        _lookupRepositoryMock.SetupPromotionLevelExistsByName(conflictingName, true);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ConflictException>();
    }

    [Fact]
    public async Task Handle_WhenNameConflictWithDifferentEntity_ShouldNotCommit()
    {
        // Arrange
        PromotionLevelEntity existing = PromotionLevelFactory.CreateDefault();
        string conflictingName = TestConstants.Content.PromotionLevel.AnotherValidName;
        var command = new AdminUpdatePromotionLevelCommand(
            Id: existing.Id.ToString(),
            Name: conflictingName,
            DurationDays: 14,
            PriceUsd: 99.99m
        );

        _lookupRepositoryMock.SetupGetPromotionLevelByIdOrThrow(existing);
        _lookupRepositoryMock.SetupPromotionLevelExistsByName(conflictingName, true);

        // Act
        try
        {
            await _handler.Handle(command, CancellationToken.None);
        }
        catch (ConflictException)
        {
            // Expected
        }

        // Assert
        _unitOfWorkMock.VerifyCommitNotCalled();
    }

    #endregion
}
