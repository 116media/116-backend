using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using Moq;

namespace _116.Unit.Tests.Common.Mocks.Repositories;

/// <summary>
/// Provides mock setup helpers for <see cref="IArtistClaimRequestRepository"/>.
/// </summary>
public static class MockArtistClaimRequestRepository
{
    /// <summary>
    /// Creates a new mock instance of IArtistClaimRequestRepository with safe default setups.
    /// </summary>
    public static Mock<IArtistClaimRequestRepository> Create()
    {
        Mock<IArtistClaimRequestRepository> mock = new();
        SetupDefaults(mock);
        return mock;
    }

    public static void VerifyAddCalled(this Mock<IArtistClaimRequestRepository> mock)
    {
        mock.Verify(x => x.AddAsync(It.IsAny<ArtistClaimRequestEntity>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// Sets up the duplicate lookup to report whether a request already exists for the
    /// artist and user pair under test.
    /// </summary>
    /// <param name="mock">The repository mock to configure.</param>
    /// <param name="exists">The result the lookup should return.</param>
    public static void SetupExistsForArtistAndUser(this Mock<IArtistClaimRequestRepository> mock, bool exists)
    {
        mock.Setup(x =>
                x.ExistsForArtistAndUserAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>())
            )
            .ReturnsAsync(exists);
    }

    private static void SetupDefaults(Mock<IArtistClaimRequestRepository> mock)
    {
        mock.Setup(x => x.AddAsync(It.IsAny<ArtistClaimRequestEntity>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        mock.SetupExistsForArtistAndUser(exists: false);
    }
}
