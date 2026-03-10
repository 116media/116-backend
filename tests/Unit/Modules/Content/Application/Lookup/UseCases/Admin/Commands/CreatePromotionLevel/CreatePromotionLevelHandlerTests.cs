using _116.Content.Application.Lookup.UseCases.Admin.Commands.CreatePromotionLevel;
using _116.Content.Application.Shared.Persistence;
using _116.Content.Application.Shared.Repositories;
using _116.Shared.Application.Exceptions;
using _116.Tests.Fixtures.Constants;
using _116.Unit.Tests.Common;
using _116.Unit.Tests.Common.Mocks.Infrastructure;
using _116.Unit.Tests.Common.Mocks.Repositories;
using AwesomeAssertions;
using Moq;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Lookup.UseCases.Admin.Commands.CreatePromotionLevel;

/// <summary>
/// Unit tests for <see cref="CreatePromotionLevelHandler"/>.
/// </summary>
public class CreatePromotionLevelHandlerTests : BaseContentHandlerTest
{
    private readonly Mock<ILookupRepository> _lookupRepositoryMock;
    private readonly Mock<IContentUnitOfWork> _unitOfWorkMock;
    private readonly CreatePromotionLevelHandler _handler;

    public CreatePromotionLevelHandlerTests()
    {
        _lookupRepositoryMock = MockLookupRepository.Create();
        _unitOfWorkMock = MockContentUnitOfWork.Create();
        _handler = new CreatePromotionLevelHandler(_lookupRepositoryMock.Object, _unitOfWorkMock.Object, Mapper);
    }

    #region Success Cases

    [Fact]
    public async Task Handle_WhenNameDoesNotExist_ShouldCreateAndReturnDto()
    {
        // Arrange
        string name = TestConstants.Content.PromotionLevel.ValidName;
        int durationDays = TestConstants.Content.PromotionLevel.ValidDurationDays;
        decimal priceUsd = TestConstants.Content.PromotionLevel.ValidPriceUsd;
        var command = new CreatePromotionLevelCommand(Name: name, DurationDays: durationDays, PriceUsd: priceUsd);

        _lookupRepositoryMock.SetupPromotionLevelExistsByName(name, false);

        // Act
        CreatePromotionLevelResult result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.PromotionLevel.Name.Should().Be(name);
        result.PromotionLevel.DurationDays.Should().Be(durationDays);
        result.PromotionLevel.PriceUsd.Should().Be(priceUsd);
        result.PromotionLevel.IsActive.Should().BeTrue();

        _lookupRepositoryMock.VerifyAddPromotionLevelCalled();
        _unitOfWorkMock.VerifyCommitCalled();
    }

    [Fact]
    public async Task Handle_WithZeroPrice_ShouldCreateSuccessfully()
    {
        // Arrange
        string name = TestConstants.Content.PromotionLevel.ValidName;
        int durationDays = TestConstants.Content.PromotionLevel.ValidDurationDays;
        decimal priceUsd = TestConstants.Content.PromotionLevel.ZeroPriceUsd;
        var command = new CreatePromotionLevelCommand(Name: name, DurationDays: durationDays, PriceUsd: priceUsd);

        _lookupRepositoryMock.SetupPromotionLevelExistsByName(name, false);

        // Act
        CreatePromotionLevelResult result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.PromotionLevel.PriceUsd.Should().Be(0m);
    }

    #endregion

    #region Failure Cases

    [Fact]
    public async Task Handle_WhenNameAlreadyExists_ShouldThrowConflictException()
    {
        // Arrange
        string name = TestConstants.Content.PromotionLevel.ValidName;
        var command = new CreatePromotionLevelCommand(Name: name, DurationDays: 7, PriceUsd: 49.99m);

        _lookupRepositoryMock.SetupPromotionLevelExistsByName(name, true);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ConflictException>();
    }

    [Fact]
    public async Task Handle_WhenNameAlreadyExists_ShouldNotAddOrCommit()
    {
        // Arrange
        string name = TestConstants.Content.PromotionLevel.ValidName;
        var command = new CreatePromotionLevelCommand(Name: name, DurationDays: 7, PriceUsd: 49.99m);

        _lookupRepositoryMock.SetupPromotionLevelExistsByName(name, true);

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
        _lookupRepositoryMock.VerifyAddPromotionLevelNotCalled();
        _unitOfWorkMock.VerifyCommitNotCalled();
    }

    #endregion

    #region Cancellation Token

    [Fact]
    public async Task Handle_WithCancellationToken_ShouldPassTokenToRepository()
    {
        // Arrange
        string name = TestConstants.Content.PromotionLevel.ValidName;
        var command = new CreatePromotionLevelCommand(Name: name, DurationDays: 7, PriceUsd: 49.99m);
        using var cts = new CancellationTokenSource();

        _lookupRepositoryMock.SetupPromotionLevelExistsByName(name, false);

        // Act
        CreatePromotionLevelResult result = await _handler.Handle(command, cts.Token);

        // Assert
        result.Should().NotBeNull();
        _unitOfWorkMock.Verify(x => x.CommitAsync(cts.Token), Times.Once);
    }

    #endregion
}
