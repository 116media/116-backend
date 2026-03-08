using _116.Content.Application.Lookup.UseCases.Admin.Commands.ActivatePricingTier;
using _116.Content.Application.Shared.Persistence;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Shared.Application.Exceptions;
using _116.Tests.Fixtures.Factories.Content;
using _116.Unit.Tests.Common;
using _116.Unit.Tests.Common.Mocks.Infrastructure;
using _116.Unit.Tests.Common.Mocks.Repositories;
using AwesomeAssertions;
using Moq;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Lookup.UseCases.Admin.Commands.ActivatePricingTier;

/// <summary>
/// Unit tests for <see cref="ActivatePricingTierHandler"/>.
/// </summary>
public class ActivatePricingTierHandlerTests : BaseContentHandlerTest
{
    private readonly Mock<ILookupRepository> _lookupRepositoryMock;
    private readonly Mock<IContentUnitOfWork> _unitOfWorkMock;
    private readonly ActivatePricingTierHandler _handler;

    public ActivatePricingTierHandlerTests()
    {
        _lookupRepositoryMock = MockLookupRepository.Create();
        _unitOfWorkMock = MockContentUnitOfWork.Create();
        _handler = new ActivatePricingTierHandler(_lookupRepositoryMock.Object, _unitOfWorkMock.Object, Mapper);
    }

    #region Success Cases

    [Fact]
    public async Task Handle_WhenInactive_ShouldActivateAndReturnDto()
    {
        // Arrange
        PricingTierEntity inactive = PricingTierFactory.CreateInactive();
        var command = new ActivatePricingTierCommand(Id: inactive.Id);

        _lookupRepositoryMock.SetupGetPricingTierByIdOrThrow(inactive);

        // Act
        ActivatePricingTierResult result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.PricingTier.IsActive.Should().BeTrue();
        _unitOfWorkMock.VerifyCommitCalled();
    }

    #endregion

    #region Failure Cases

    [Fact]
    public async Task Handle_WhenAlreadyActive_ShouldThrowConflictException()
    {
        // Arrange
        PricingTierEntity active = PricingTierFactory.CreateDefault();
        var command = new ActivatePricingTierCommand(Id: active.Id);

        _lookupRepositoryMock.SetupGetPricingTierByIdOrThrow(active);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ConflictException>();
    }

    [Fact]
    public async Task Handle_WhenAlreadyActive_ShouldNotCommit()
    {
        // Arrange
        PricingTierEntity active = PricingTierFactory.CreateDefault();
        var command = new ActivatePricingTierCommand(Id: active.Id);

        _lookupRepositoryMock.SetupGetPricingTierByIdOrThrow(active);

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

    [Fact]
    public async Task Handle_WhenNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        Guid nonExistentId = Guid.NewGuid();
        var command = new ActivatePricingTierCommand(Id: nonExistentId);

        _lookupRepositoryMock.SetupGetPricingTierByIdOrThrowNotFound(nonExistentId);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    #endregion
}
