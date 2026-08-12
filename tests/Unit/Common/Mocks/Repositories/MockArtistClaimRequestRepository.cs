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
    /// Answers the duplicate lookup for one artist and user pair only. Any other pair falls through
    /// to the default false, so a handler that checks a different artist or a different claimant is
    /// not silently handed this answer.
    /// </summary>
    /// <param name="mock">The repository mock to configure.</param>
    /// <param name="artistId">The artist the arrangement answers for.</param>
    /// <param name="userId">The claimant the arrangement answers for.</param>
    /// <param name="exists">The result the lookup should return for that pair.</param>
    public static void SetupExistsForArtistAndUser(
        this Mock<IArtistClaimRequestRepository> mock,
        Guid artistId,
        Guid userId,
        bool exists
    )
    {
        mock.Setup(x =>
                x.ExistsForArtistAndUserAsync(
                    It.Is<Guid>(id => id == artistId),
                    It.Is<Guid>(id => id == userId),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(exists);
    }

    /// <summary>
    /// Installs defaults for write, void and aggregate members only. Identity lookups are left
    /// unconfigured so that a miss has to be arranged by the test, naming the identifier it is a
    /// miss for, rather than being asserted for every identifier before the test says anything.
    /// </summary>
    /// <param name="mock">The repository mock to configure.</param>
    private static void SetupDefaults(Mock<IArtistClaimRequestRepository> mock)
    {
        mock.Setup(x => x.AddAsync(It.IsAny<ArtistClaimRequestEntity>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        mock.Setup(x =>
                x.ExistsForArtistAndUserAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>())
            )
            .ReturnsAsync(false);
    }
}
