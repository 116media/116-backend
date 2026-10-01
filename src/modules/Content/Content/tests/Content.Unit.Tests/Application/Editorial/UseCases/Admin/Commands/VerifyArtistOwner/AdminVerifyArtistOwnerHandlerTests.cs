using _116.BuildingBlocks.Application.Exceptions;
using _116.Content.Application.Editorial.Services;
using _116.Content.Application.Editorial.UseCases.Admin.Commands.VerifyArtistOwner;
using _116.Content.Application.Shared.Persistence;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Content.Domain.Events;
using _116.Content.Domain.Exceptions;
using _116.Content.Domain.StateMachines;
using _116.Content.TestData.Factories;
using _116.Content.TestData.Mocks.Infrastructure;
using _116.Content.TestData.Mocks.Repositories;
using _116.Content.TestData.Mocks.Services;
using _116.Identity.TestData.Mocks.Infrastructure;
using _116.Identity.TestData.Mocks.Repositories;
using _116.Storage.Contracts.Application.Services;
using _116.Storage.TestData.Mocks.Infrastructure;
using AwesomeAssertions;
using Moq;
using Xunit;

namespace _116.Content.Unit.Tests.Application.Editorial.UseCases.Admin.Commands.VerifyArtistOwner;

/// <summary>
/// Unit tests for <see cref="AdminVerifyArtistOwnerHandler"/>.
/// </summary>
public class AdminVerifyArtistOwnerHandlerTests
{
    private readonly Mock<IArtistRepository> _artistRepositoryMock;
    private readonly Mock<IContentUnitOfWork> _unitOfWorkMock;
    private readonly AdminVerifyArtistOwnerHandler _handler;

    public AdminVerifyArtistOwnerHandlerTests()
    {
        _artistRepositoryMock = MockArtistRepository.Create();
        _unitOfWorkMock = MockContentUnitOfWork.Create();
        Mock<IFileStorageService> fileStorageMock = MockFileStorageService.Create();
        _handler = new AdminVerifyArtistOwnerHandler(
            _artistRepositoryMock.Object,
            _unitOfWorkMock.Object,
            new ArtistDtoService(fileStorageMock.Object),
            TimeProvider.System
        );
    }

    [Fact]
    public async Task Handle_WhenArtistUnclaimed_ShouldClaimOwnership()
    {
        // Arrange
        ArtistEntity artist = ArtistFactory.Create();
        _artistRepositoryMock.SetupGetByIdOrThrow(artist);
        Guid userId = Guid.NewGuid();
        var command = new AdminVerifyArtistOwnerCommand(artist.Id, userId);

        // Act
        AdminVerifyArtistOwnerResult result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        artist.UserId.Should().Be(userId);
        artist.VerifiedAt.Should().NotBeNull();
        result.Artist.Id.Should().Be(artist.Id);
        _unitOfWorkMock.VerifyCommitCalled();
    }

    [Fact]
    public async Task Handle_WhenArtistUnclaimed_ShouldRaiseArtistOwnershipVerifiedEvent()
    {
        // Arrange
        ArtistEntity artist = ArtistFactory.Create();
        artist.ClearDomainEvents();
        _artistRepositoryMock.SetupGetByIdOrThrow(artist);
        Guid userId = Guid.NewGuid();
        var command = new AdminVerifyArtistOwnerCommand(artist.Id, userId);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        artist
            .DomainEvents.OfType<ArtistOwnershipVerifiedEvent>()
            .Should()
            .ContainSingle()
            .Which.Should()
            .Be(new ArtistOwnershipVerifiedEvent(ArtistId: artist.Id, UserId: userId));
    }

    [Fact]
    public async Task Handle_WhenArtistAlreadyClaimed_ShouldThrowConflictException()
    {
        // Arrange
        Guid originalOwnerId = Guid.NewGuid();
        ArtistEntity artist = ArtistFactory.CreateClaimed(originalOwnerId);
        artist.ClearDomainEvents();
        _artistRepositoryMock.SetupGetByIdOrThrow(artist);
        var command = new AdminVerifyArtistOwnerCommand(artist.Id, Guid.NewGuid());

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        (await act.Should().ThrowAsync<ContentRuleException>())
            .Which.Code.Should()
            .Be(ContentRuleCodes.ArtistAlreadyClaimed);
        artist.UserId.Should().Be(originalOwnerId);
        artist.DomainEvents.Should().BeEmpty();
        _unitOfWorkMock.VerifyCommitNotCalled();
    }

    [Fact]
    public async Task Handle_WhenArtistNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        Guid nonExistentId = Guid.NewGuid();
        _artistRepositoryMock.SetupGetByIdOrThrowNotFound(nonExistentId);
        var command = new AdminVerifyArtistOwnerCommand(nonExistentId, Guid.NewGuid());

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
        _unitOfWorkMock.VerifyCommitNotCalled();
    }
}
