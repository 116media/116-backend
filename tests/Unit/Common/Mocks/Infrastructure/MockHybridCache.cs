using Microsoft.Extensions.Caching.Hybrid;
using Moq;

namespace _116.Unit.Tests.Common.Mocks.Infrastructure;

/// <summary>
/// Provides mock setup helpers for <see cref="HybridCache"/>.
/// </summary>
public static class MockHybridCache
{
    /// <summary>
    /// Creates a new mock instance of HybridCache.
    /// </summary>
    /// <returns>A configured Mock of HybridCache.</returns>
    public static Mock<HybridCache> Create()
    {
        return new Mock<HybridCache>();
    }

    /// <summary>
    /// Verifies that RemoveByTagAsync was called exactly once with the specified tag.
    /// </summary>
    /// <param name="mock">The mock instance.</param>
    /// <param name="tag">The tag whose entries must have been evicted.</param>
    public static void VerifyRemovedByTag(this Mock<HybridCache> mock, string tag)
    {
        mock.Verify(x => x.RemoveByTagAsync(tag, It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// Verifies that RemoveByTagAsync was never called with any tag.
    /// </summary>
    /// <param name="mock">The mock instance.</param>
    public static void VerifyNoTagRemoved(this Mock<HybridCache> mock)
    {
        mock.Verify(x => x.RemoveByTagAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
