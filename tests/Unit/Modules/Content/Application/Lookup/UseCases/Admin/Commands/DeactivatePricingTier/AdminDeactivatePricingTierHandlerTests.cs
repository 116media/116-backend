using _116.Content.Application.Lookup.UseCases.Admin.Commands.DeactivatePricingTier;
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

namespace _116.Unit.Tests.Modules.Content.Application.Lookup.UseCases.Admin.Commands.DeactivatePricingTier;

/// <summary>
/// Unit tests for <see cref="AdminDeactivatePricingTierHandler"/>.
/// </summary>
public class AdminDeactivatePricingTierHandlerTests : BaseContentHandlerTest
{
    private readonly Mock<ILookupRepository> _lookupRepositoryMock;
    private readonly Mock<IContentUnitOfWork> _unitOfWorkMock;
    private readonly AdminDeactivatePricingTierHandler _handler;

    public AdminDeactivatePricingTierHandlerTests()
    {
        _lookupRepositoryMock = MockLookupRepository.Create();
        _unitOfWorkMock = MockContentUnitOfWork.Create();
        _handler = new AdminDeactivatePricingTierHandler(_lookupRepositoryMock.Object, _unitOfWorkMock.Object, Mapper);
    }

    #region Success Cases

    [Fact]
    public async Task Handle_WhenActive_ShouldDeactivateAndReturnDto()
    {
        // Arrange
        PricingTierEntity active = PricingTierFactory.CreateDefault();
        var command = new AdminDeactivatePricingTierCommand(Id: active.Id.ToString());

        _lookupRepositoryMock.SetupGetPricingTierByIdOrThrow(active);

        // Act
        AdminDeactivatePricingTierResult result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.PricingTier.IsActive.Should().BeFalse();
        _unitOfWorkMock.VerifyCommitCalled();
    }

    #endregion

    #region Failure Cases

    [Fact]
    public async Task Handle_WhenAlreadyInactive_ShouldThrowConflictException()
    {
        // Arrange
        PricingTierEntity inactive = PricingTierFactory.CreateInactive();
        var command = new AdminDeactivatePricingTierCommand(Id: inactive.Id.ToString());

        _lookupRepositoryMock.SetupGetPricingTierByIdOrThrow(inactive);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ConflictException>();
    }

    [Fact]
    public async Task Handle_WhenAlreadyInactive_ShouldNotCommit()
    {
        // Arrange
        PricingTierEntity inactive = PricingTierFactory.CreateInactive();
        var command = new AdminDeactivatePricingTierCommand(Id: inactive.Id.ToString());

        _lookupRepositoryMock.SetupGetPricingTierByIdOrThrow(inactive);

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
        var nonExistentId = Guid.NewGuid();
        var command = new AdminDeactivatePricingTierCommand(Id: nonExistentId.ToString());

        _lookupRepositoryMock.SetupGetPricingTierByIdOrThrowNotFound(nonExistentId);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    #endregion
}
