using _116.Content.Application.Shared.Persistence;
using Moq;

namespace _116.Unit.Tests.Common.Mocks.Infrastructure;

/// <summary>
/// Provides mock setup helpers for <see cref="IContentUnitOfWork"/>.
/// </summary>
public static class MockContentUnitOfWork
{
    /// <summary>
    /// Creates a new mock instance of IContentUnitOfWork.
    /// </summary>
    /// <returns>A configured Mock of IContentUnitOfWork.</returns>
    public static Mock<IContentUnitOfWork> Create()
    {
        Mock<IContentUnitOfWork> mock = new();
        SetupDefaultCommit(mock);
        return mock;
    }

    /// <summary>
    /// Sets up the CommitAsync method to return a successful result.
    /// </summary>
    /// <param name="mock">The mock instance.</param>
    /// <param name="result">The number of affected rows to return (default: 1).</param>
    /// <returns>The mock instance for chaining.</returns>
    public static Mock<IContentUnitOfWork> SetupCommit(this Mock<IContentUnitOfWork> mock, int result = 1)
    {
        mock.Setup(x => x.CommitAsync(It.IsAny<CancellationToken>())).ReturnsAsync(result);
        return mock;
    }

    /// <summary>
    /// Sets up the CommitAsync method to throw an exception.
    /// </summary>
    /// <param name="mock">The mock instance.</param>
    /// <param name="exception">The exception to throw.</param>
    /// <returns>The mock instance for chaining.</returns>
    public static Mock<IContentUnitOfWork> SetupCommitThrows(this Mock<IContentUnitOfWork> mock, Exception exception)
    {
        mock.Setup(x => x.CommitAsync(It.IsAny<CancellationToken>())).ThrowsAsync(exception);
        return mock;
    }

    /// <summary>
    /// Verifies that CommitAsync was called exactly once.
    /// </summary>
    /// <param name="mock">The mock instance.</param>
    public static void VerifyCommitCalled(this Mock<IContentUnitOfWork> mock)
    {
        mock.Verify(x => x.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// Verifies that CommitAsync was never called.
    /// </summary>
    /// <param name="mock">The mock instance.</param>
    public static void VerifyCommitNotCalled(this Mock<IContentUnitOfWork> mock)
    {
        mock.Verify(x => x.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    /// <summary>
    /// Verifies that CommitAsync was called the specified number of times.
    /// </summary>
    /// <param name="mock">The mock instance.</param>
    /// <param name="times">The number of times the method should have been called.</param>
    public static void VerifyCommitCalled(this Mock<IContentUnitOfWork> mock, int times)
    {
        mock.Verify(x => x.CommitAsync(It.IsAny<CancellationToken>()), Times.Exactly(times));
    }

    /// <summary>
    /// Sets up default behavior for the mock.
    /// </summary>
    private static void SetupDefaultCommit(Mock<IContentUnitOfWork> mock)
    {
        mock.Setup(x => x.CommitAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
    }
}
