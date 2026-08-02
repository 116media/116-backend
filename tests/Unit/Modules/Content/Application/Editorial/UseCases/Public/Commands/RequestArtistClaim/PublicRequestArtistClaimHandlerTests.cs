using _116.Content.Application.Editorial.UseCases.Public.Commands.RequestArtistClaim;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Shared.Application.Exceptions;
using _116.Tests.Fixtures.Factories.Content;
using _116.Unit.Tests.Common.Mocks.Repositories;
using AwesomeAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Editorial.UseCases.Public.Commands.RequestArtistClaim;

/// <summary>
/// Unit tests for <see cref="PublicRequestArtistClaimHandler"/>.
/// </summary>
public class PublicRequestArtistClaimHandlerTests
{
    private readonly Mock<IArtistRepository> _artistRepositoryMock;
    private readonly PublicRequestArtistClaimHandler _handler;

    public PublicRequestArtistClaimHandlerTests()
    {
        _artistRepositoryMock = MockArtistRepository.Create();
        Mock<ILogger<PublicRequestArtistClaimHandler>> loggerMock = new();
        _handler = new PublicRequestArtistClaimHandler(_artistRepositoryMock.Object, loggerMock.Object);
    }

    [Fact]
    public async Task Handle_WhenArtistExists_ShouldReturnSuccessWithoutMutatingTheProfile()
    {
        // Arrange
        ArtistEntity artist = ArtistFactory.Create();
        _artistRepositoryMock.SetupGetByIdOrThrow(artist);
        var command = new PublicRequestArtistClaimCommand(artist.Id, Guid.NewGuid());

        // Act
        PublicRequestArtistClaimResult result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        artist.UserId.Should().BeNull();
        artist.VerifiedAt.Should().BeNull();
    }

    /// <summary>
    /// This is the crux of spec 08's request/verify split: requesting a claim must never call
    /// <see cref="ArtistEntity.ClaimOwnership"/> or persist any mutation — only an admin's
    /// separate verify-owner action does that. This asserts the repository's <c>Update</c> is
    /// never invoked as proof no mutation happened.
    /// </summary>
    [Fact]
    public async Task Handle_ShouldNeverCallRepositoryUpdate()
    {
        // Arrange
        ArtistEntity artist = ArtistFactory.Create();
        _artistRepositoryMock.SetupGetByIdOrThrow(artist);
        var command = new PublicRequestArtistClaimCommand(artist.Id, Guid.NewGuid());

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _artistRepositoryMock.Verify(x => x.Update(It.IsAny<ArtistEntity>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenArtistNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        Guid nonExistentId = Guid.NewGuid();
        _artistRepositoryMock.SetupGetByIdOrThrowNotFound(nonExistentId);
        var command = new PublicRequestArtistClaimCommand(nonExistentId, Guid.NewGuid());

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }
}
