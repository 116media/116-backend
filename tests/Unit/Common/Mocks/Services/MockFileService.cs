using _116.Core.Application.Shared.Services;
using Moq;

namespace _116.Unit.Tests.Common.Mocks.Services;

/// <summary>
/// Provides mock setup helpers for <see cref="IFileService"/>.
/// </summary>
public static class MockFileService
{
    /// <summary>
    /// Creates a new mock instance of IFileService with safe default setups.
    /// </summary>
    public static Mock<IFileService> Create()
    {
        Mock<IFileService> mock = new();
        mock.Setup(x => x.DeleteFileAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(true);
        return mock;
    }

    /// <summary>
    /// Verifies that DeleteFileAsync was called at least once.
    /// </summary>
    /// <param name="mock">The mock instance.</param>
    public static void VerifyDeleteFileCalled(this Mock<IFileService> mock)
    {
        mock.Verify(x => x.DeleteFileAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.AtLeastOnce);
    }

    /// <summary>
    /// Verifies that DeleteFileAsync was called a specific number of times.
    /// </summary>
    /// <param name="mock">The mock instance.</param>
    /// <param name="times">The expected number of calls.</param>
    public static void VerifyDeleteFileCalled(this Mock<IFileService> mock, Times times)
    {
        mock.Verify(x => x.DeleteFileAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), times);
    }

    /// <summary>
    /// Verifies that DeleteFileAsync was not called.
    /// </summary>
    /// <param name="mock">The mock instance.</param>
    public static void VerifyDeleteFileNotCalled(this Mock<IFileService> mock)
    {
        mock.Verify(x => x.DeleteFileAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
