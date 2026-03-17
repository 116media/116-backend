using _116.Content.Application.Commerce.UseCases.Admin.Commands.VerifyPayment.Contracts;
using Moq;

namespace _116.Unit.Tests.Common.Mocks.Factories;

/// <summary>
/// Provides mock setup helpers for <see cref="IVerifyPaymentFactory"/>.
/// </summary>
public static class MockVerifyPaymentFactory
{
    /// <summary>
    /// Creates a new mock instance of IVerifyPaymentFactory.
    /// </summary>
    public static Mock<IVerifyPaymentFactory> Create() => new();

    public static Mock<IVerifyPaymentFactory> SetupVerifyAsync(this Mock<IVerifyPaymentFactory> mock)
    {
        mock.Setup(x =>
                x.VerifyAsync(
                    It.IsAny<_116.Content.Domain.Entities.ContentOrderEntity>(),
                    It.IsAny<_116.Content.Domain.Entities.ContentPaymentEntity>(),
                    It.IsAny<Guid>(),
                    It.IsAny<string>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .Returns(Task.CompletedTask);
        return mock;
    }

    public static Mock<IVerifyPaymentFactory> SetupVerifyAsyncThrows(
        this Mock<IVerifyPaymentFactory> mock,
        Exception exception
    )
    {
        mock.Setup(x =>
                x.VerifyAsync(
                    It.IsAny<_116.Content.Domain.Entities.ContentOrderEntity>(),
                    It.IsAny<_116.Content.Domain.Entities.ContentPaymentEntity>(),
                    It.IsAny<Guid>(),
                    It.IsAny<string>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .ThrowsAsync(exception);
        return mock;
    }

    public static void VerifyVerifyCalled(this Mock<IVerifyPaymentFactory> mock)
    {
        mock.Verify(
            x =>
                x.VerifyAsync(
                    It.IsAny<_116.Content.Domain.Entities.ContentOrderEntity>(),
                    It.IsAny<_116.Content.Domain.Entities.ContentPaymentEntity>(),
                    It.IsAny<Guid>(),
                    It.IsAny<string>(),
                    It.IsAny<CancellationToken>()
                ),
            Times.Once
        );
    }
}
