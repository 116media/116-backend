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
/// Unit tests for <see cref="AdminActivatePricingTierHandler"/>.
/// </summary>
public class AdminActivatePricingTierHandlerTests : BaseContentHandlerTest
{
    private readonly Mock<ILookupRepository> _lookupRepositoryMock;
    private readonly Mock<IContentUnitOfWork> _unitOfWorkMock;
    private readonly AdminActivatePricingTierHandler _handler;

    public AdminActivatePricingTierHandlerTests()
    {
        _lookupRepositoryMock = MockLookupRepository.Create();
        _unitOfWorkMock = MockContentUnitOfWork.Create();
        _handler = new AdminActivatePricingTierHandler(_lookupRepositoryMock.Object, _unitOfWorkMock.Object, Mapper);
    }

    #region Success Cases

    [Fact]
    public async Task Handle_WhenInactive_ShouldActivateAndReturnDto()
    {
        // Arrange
        PricingTierEntity inactive = PricingTierFactory.CreateInactive();
        var command = new AdminActivatePricingTierCommand(Id: inactive.Id.ToString());

        _lookupRepositoryMock.SetupGetPricingTierByIdOrThrow(inactive);

        // Act
        AdminActivatePricingTierResult result = await _handler.Handle(command, CancellationToken.None);

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
        var command = new AdminActivatePricingTierCommand(Id: active.Id.ToString());

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
        var command = new AdminActivatePricingTierCommand(Id: active.Id.ToString());

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
        var nonExistentId = Guid.NewGuid();
        var command = new AdminActivatePricingTierCommand(Id: nonExistentId.ToString());

        _lookupRepositoryMock.SetupGetPricingTierByIdOrThrowNotFound(nonExistentId);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    #endregion
}
