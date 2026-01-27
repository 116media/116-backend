using _116.Core.Application.Shared.Persistence;
using Moq;

namespace _116.Unit.Tests.Common.Mocks.Infrastructure;

/// <summary>
/// Provides mock setup helpers for <see cref="ICoreUnitOfWork"/>.
/// </summary>
public static class MockCoreUnitOfWork
{
    /// <summary>
    /// Creates a new mock instance of ICoreUnitOfWork.
    /// </summary>
    /// <returns>A configured Mock of ICoreUnitOfWork.</returns>
    public static Mock<ICoreUnitOfWork> Create()
    {
        Mock<ICoreUnitOfWork> mock = new();
        SetupDefaultCommit(mock);
        return mock;
    }

    /// <summary>
    /// Sets up the CommitAsync method to return a successful result.
    /// </summary>
    /// <param name="mock">The mock instance.</param>
    /// <param name="result">The number of affected rows to return (default: 1).</param>
    /// <returns>The mock instance for chaining.</returns>
    public static Mock<ICoreUnitOfWork> SetupCommit(this Mock<ICoreUnitOfWork> mock, int result = 1)
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
    public static Mock<ICoreUnitOfWork> SetupCommitThrows(this Mock<ICoreUnitOfWork> mock, Exception exception)
    {
        mock.Setup(x => x.CommitAsync(It.IsAny<CancellationToken>())).ThrowsAsync(exception);
        return mock;
    }

    /// <summary>
    /// Verifies that CommitAsync was called exactly once.
    /// </summary>
    /// <param name="mock">The mock instance.</param>
    public static void VerifyCommitCalled(this Mock<ICoreUnitOfWork> mock)
    {
        mock.Verify(x => x.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// Verifies that CommitAsync was never called.
    /// </summary>
    /// <param name="mock">The mock instance.</param>
    public static void VerifyCommitNotCalled(this Mock<ICoreUnitOfWork> mock)
    {
        mock.Verify(x => x.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    /// <summary>
    /// Verifies that CommitAsync was called the specified number of times.
    /// </summary>
    /// <param name="mock">The mock instance.</param>
    /// <param name="times">The number of times the method should have been called.</param>
    public static void VerifyCommitCalled(this Mock<ICoreUnitOfWork> mock, int times)
    {
        mock.Verify(x => x.CommitAsync(It.IsAny<CancellationToken>()), Times.Exactly(times));
    }

    /// <summary>
    /// Sets up default behavior for the mock.
    /// </summary>
    private static void SetupDefaultCommit(Mock<ICoreUnitOfWork> mock)
    {
        mock.Setup(x => x.CommitAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
    }
}
