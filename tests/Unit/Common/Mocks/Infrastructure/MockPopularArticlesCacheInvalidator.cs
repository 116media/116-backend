using _116.Content.Application.Shared.Cache;
using Moq;

namespace _116.Unit.Tests.Common.Mocks.Infrastructure;

/// <summary>
/// Provides mock setup helpers for <see cref="IPopularArticlesCacheInvalidator"/>.
/// </summary>
public static class MockPopularArticlesCacheInvalidator
{
    /// <summary>
    /// Creates a new mock instance of <see cref="IPopularArticlesCacheInvalidator" />.
    /// <see cref="IPopularArticlesCacheInvalidator.GetEvictionToken" /> returns a default (never-cancelled)
    /// <see cref="CancellationToken" /> so cache entries stored during a test do not expire unexpectedly.
    /// </summary>
    /// <returns>
    /// A configured <see cref="Mock{T}" /> of <see cref="IPopularArticlesCacheInvalidator" />.
    /// </returns>
    public static Mock<IPopularArticlesCacheInvalidator> Create()
    {
        Mock<IPopularArticlesCacheInvalidator> mock = new();
        mock.Setup(x => x.GetEvictionToken()).Returns(CancellationToken.None);
        return mock;
    }

    /// <summary>
    /// Verifies that <see cref="IPopularArticlesCacheInvalidator.Invalidate" /> was called exactly once.
    /// </summary>
    /// <param name="mock">The mock instance.</param>
    public static void VerifyInvalidateCalled(this Mock<IPopularArticlesCacheInvalidator> mock)
    {
        mock.Verify(x => x.Invalidate(), Times.Once);
    }

    /// <summary>
    /// Verifies that <see cref="IPopularArticlesCacheInvalidator.Invalidate" /> was never called.
    /// </summary>
    /// <param name="mock">The mock instance.</param>
    public static void VerifyInvalidateNotCalled(this Mock<IPopularArticlesCacheInvalidator> mock)
    {
        mock.Verify(x => x.Invalidate(), Times.Never);
    }
}
