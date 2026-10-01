using _116.Content.Application.Commerce.UseCases.Admin.Commands.SubmitOrder.Contracts;
using _116.Content.Domain.Entities;
using Moq;

namespace _116.Content.TestData.Mocks.Services;

/// <summary>
/// Provides mock setup helpers for <see cref="ISubmitOrderService"/>.
/// </summary>
public static class MockSubmitOrderService
{
    /// <summary>
    /// Creates a new mock instance of ISubmitOrderService.
    /// </summary>
    public static Mock<ISubmitOrderService> Create() => new();

    public static Mock<ISubmitOrderService> SetupSubmitAsync(this Mock<ISubmitOrderService> mock)
    {
        mock.Setup(x => x.SubmitAsync(It.IsAny<ContentOrderEntity>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        return mock;
    }

    public static void VerifySubmitCalled(this Mock<ISubmitOrderService> mock)
    {
        mock.Verify(x => x.SubmitAsync(It.IsAny<ContentOrderEntity>(), It.IsAny<CancellationToken>()), Times.Once);
    }
}
