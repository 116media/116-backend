using _116.Content.Application.Commerce.UseCases.Admin.Commands.VerifyPayment.Contracts;
using _116.Content.Domain.Entities;
using Moq;

namespace _116.Content.TestData.Mocks.Services;

/// <summary>
/// Provides mock setup helpers for <see cref="IVerifyPaymentService"/>.
/// </summary>
public static class MockVerifyPaymentService
{
    /// <summary>
    /// Creates a new mock instance of IVerifyPaymentService.
    /// </summary>
    public static Mock<IVerifyPaymentService> Create() => new();

    public static Mock<IVerifyPaymentService> SetupVerifyAsync(this Mock<IVerifyPaymentService> mock)
    {
        mock.Setup(x =>
                x.VerifyAsync(
                    It.IsAny<ContentOrderEntity>(),
                    It.IsAny<ContentPaymentEntity>(),
                    It.IsAny<Guid>(),
                    It.IsAny<string>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .Returns(Task.CompletedTask);
        return mock;
    }
}
