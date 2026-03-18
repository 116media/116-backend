using _116.Content.Application.Commerce.UseCases.Admin.Commands.SubmitOrder.Contracts;
using _116.Content.Domain.Entities;
using Moq;

namespace _116.Unit.Tests.Common.Mocks.Factories;

/// <summary>
/// Provides mock setup helpers for <see cref="ISubmitOrderFactory"/>.
/// </summary>
public static class MockSubmitOrderFactory
{
    /// <summary>
    /// Creates a new mock instance of ISubmitOrderFactory.
    /// </summary>
    public static Mock<ISubmitOrderFactory> Create() => new();

    public static Mock<ISubmitOrderFactory> SetupSubmitAsync(this Mock<ISubmitOrderFactory> mock)
    {
        mock.Setup(x => x.SubmitAsync(It.IsAny<ContentOrderEntity>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        return mock;
    }

    public static Mock<ISubmitOrderFactory> SetupSubmitAsyncThrows(
        this Mock<ISubmitOrderFactory> mock,
        Exception exception
    )
    {
        mock.Setup(x => x.SubmitAsync(It.IsAny<ContentOrderEntity>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(exception);
        return mock;
    }

    public static void VerifySubmitCalled(this Mock<ISubmitOrderFactory> mock)
    {
        mock.Verify(x => x.SubmitAsync(It.IsAny<ContentOrderEntity>(), It.IsAny<CancellationToken>()), Times.Once);
    }
}
