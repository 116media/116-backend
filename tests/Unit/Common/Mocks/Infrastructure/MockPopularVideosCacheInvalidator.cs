using _116.Content.Application.Shared.Cache;
using Moq;

namespace _116.Unit.Tests.Common.Mocks.Infrastructure;

/// <summary>
/// Provides mock setup helpers for <see cref="IPopularVideosCacheInvalidator"/>.
/// </summary>
public static class MockPopularVideosCacheInvalidator
{
    /// <summary>
    /// Creates a new mock instance of <see cref="IPopularVideosCacheInvalidator" />.
    /// <see cref="IPopularVideosCacheInvalidator.GetEvictionToken" /> returns a default (never-cancelled)
    /// <see cref="CancellationToken" /> so cache entries stored during a test do not expire unexpectedly.
    /// </summary>
    /// <returns>
    /// A configured <see cref="Mock{T}" /> of <see cref="IPopularVideosCacheInvalidator" />.
    /// </returns>
    public static Mock<IPopularVideosCacheInvalidator> Create()
    {
        Mock<IPopularVideosCacheInvalidator> mock = new();
        mock.Setup(x => x.GetEvictionToken()).Returns(CancellationToken.None);
        return mock;
    }

    /// <summary>
    /// Verifies that <see cref="IPopularVideosCacheInvalidator.Invalidate" /> was called exactly once.
    /// </summary>
    /// <param name="mock">The mock instance.</param>
    public static void VerifyInvalidateCalled(this Mock<IPopularVideosCacheInvalidator> mock)
    {
        mock.Verify(x => x.Invalidate(), Times.Once);
    }

    /// <summary>
    /// Verifies that <see cref="IPopularVideosCacheInvalidator.Invalidate" /> was never called.
    /// </summary>
    /// <param name="mock">The mock instance.</param>
    public static void VerifyInvalidateNotCalled(this Mock<IPopularVideosCacheInvalidator> mock)
    {
        mock.Verify(x => x.Invalidate(), Times.Never);
    }
}
