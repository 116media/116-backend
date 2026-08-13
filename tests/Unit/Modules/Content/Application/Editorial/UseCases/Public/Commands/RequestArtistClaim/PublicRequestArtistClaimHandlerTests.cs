using _116.Content.Application.Editorial.UseCases.Public.Commands.RequestArtistClaim;
using _116.Content.Application.Shared.Persistence;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Content.Domain.Events;
using _116.Shared.Application.Exceptions;
using _116.Tests.Fixtures.Factories.Content;
using _116.Tests.Fixtures.Helpers;
using _116.Unit.Tests.Common.Mocks.Infrastructure;
using _116.Unit.Tests.Common.Mocks.Repositories;
using AwesomeAssertions;
using Moq;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Editorial.UseCases.Public.Commands.RequestArtistClaim;

/// <summary>
/// Unit tests for <see cref="PublicRequestArtistClaimHandler"/>.
/// </summary>
public class PublicRequestArtistClaimHandlerTests
{
    private readonly Mock<IArtistRepository> _artistRepositoryMock;
    private readonly Mock<IArtistClaimRequestRepository> _claimRequestRepositoryMock;
    private readonly Mock<IContentUnitOfWork> _unitOfWorkMock;
    private readonly PublicRequestArtistClaimHandler _handler;

    public PublicRequestArtistClaimHandlerTests()
    {
        _artistRepositoryMock = MockArtistRepository.Create();
        _claimRequestRepositoryMock = MockArtistClaimRequestRepository.Create();
        _unitOfWorkMock = MockContentUnitOfWork.Create();
        _handler = new PublicRequestArtistClaimHandler(
            _artistRepositoryMock.Object,
            _claimRequestRepositoryMock.Object,
            _unitOfWorkMock.Object,
            TestErrorsFactory.CreateContentI18n()
        );
    }

    [Fact]
    public async Task Handle_WhenArtistExists_ShouldPersistAClaimRequestRow()
    {
        // Arrange
        ArtistEntity artist = ArtistFactory.Create();
        var userId = Guid.NewGuid();
        _artistRepositoryMock.SetupGetByIdOrThrow(artist);

        ArtistClaimRequestEntity? added = null;
        _claimRequestRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<ArtistClaimRequestEntity>(), It.IsAny<CancellationToken>()))
            .Callback<ArtistClaimRequestEntity, CancellationToken>((entity, _) => added = entity)
            .Returns(Task.CompletedTask);

        var command = new PublicRequestArtistClaimCommand(artist.Id, userId);

        // Act
        PublicRequestArtistClaimResult result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        added.Should().NotBeNull();
        added!.ArtistId.Should().Be(artist.Id);
        added.UserId.Should().Be(userId);
        _claimRequestRepositoryMock.VerifyAddCalled();
        _unitOfWorkMock.VerifyCommitCalled();
    }

    [Fact]
    public async Task Handle_ShouldRaiseTheClaimRequestedEventOnThePersistedRow()
    {
        // Arrange
        ArtistEntity artist = ArtistFactory.Create();
        var userId = Guid.NewGuid();
        _artistRepositoryMock.SetupGetByIdOrThrow(artist);

        ArtistClaimRequestEntity? added = null;
        _claimRequestRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<ArtistClaimRequestEntity>(), It.IsAny<CancellationToken>()))
            .Callback<ArtistClaimRequestEntity, CancellationToken>((entity, _) => added = entity)
            .Returns(Task.CompletedTask);

        var command = new PublicRequestArtistClaimCommand(artist.Id, userId);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        added!
            .DomainEvents.Should()
            .ContainSingle(e =>
                e is ArtistClaimRequestedEvent
                && ((ArtistClaimRequestedEvent)e).ArtistId == artist.Id
                && ((ArtistClaimRequestedEvent)e).UserId == userId
            );
    }

    [Fact]
    public async Task Handle_ShouldNeverCallArtistRepositoryUpdate()
    {
        // Arrange
        ArtistEntity artist = ArtistFactory.Create();
        _artistRepositoryMock.SetupGetByIdOrThrow(artist);
        var command = new PublicRequestArtistClaimCommand(artist.Id, Guid.NewGuid());

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _artistRepositoryMock.Verify(x => x.Update(It.IsAny<ArtistEntity>()), Times.Never);
        artist.UserId.Should().BeNull();
        artist.VerifiedAt.Should().BeNull();
    }

    [Fact]
    public async Task Handle_WhenArtistIsAlreadyOwned_ShouldThrowConflictExceptionAndPersistNothing()
    {
        // Arrange
        ArtistEntity artist = ArtistFactory.Create();
        artist.ClaimOwnership(Guid.NewGuid(), TestErrorsFactory.CreateArtistErrors());
        _artistRepositoryMock.SetupGetByIdOrThrow(artist);
        var command = new PublicRequestArtistClaimCommand(artist.Id, Guid.NewGuid());

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ConflictException>();
        _claimRequestRepositoryMock.Verify(
            x => x.AddAsync(It.IsAny<ArtistClaimRequestEntity>(), It.IsAny<CancellationToken>()),
            Times.Never
        );
        _unitOfWorkMock.Verify(x => x.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenTheSameUserAlreadyRequestedThisArtist_ShouldThrowConflictExceptionAndPersistNothing()
    {
        // Arrange
        ArtistEntity artist = ArtistFactory.Create();
        var userId = Guid.NewGuid();
        _artistRepositoryMock.SetupGetByIdOrThrow(artist);
        _claimRequestRepositoryMock.SetupExistsForArtistAndUser(artist.Id, userId, exists: true);
        var command = new PublicRequestArtistClaimCommand(artist.Id, userId);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ConflictException>();
        _claimRequestRepositoryMock.Verify(
            x => x.AddAsync(It.IsAny<ArtistClaimRequestEntity>(), It.IsAny<CancellationToken>()),
            Times.Never
        );
        _unitOfWorkMock.Verify(x => x.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenArtistNotFound_ShouldThrowNotFoundExceptionAndPersistNothing()
    {
        // Arrange
        Guid nonExistentId = Guid.NewGuid();
        _artistRepositoryMock.SetupGetByIdOrThrowNotFound(nonExistentId);
        var command = new PublicRequestArtistClaimCommand(nonExistentId, Guid.NewGuid());

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
        _claimRequestRepositoryMock.Verify(
            x => x.AddAsync(It.IsAny<ArtistClaimRequestEntity>(), It.IsAny<CancellationToken>()),
            Times.Never
        );
    }
}
