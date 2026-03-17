using _116.Content.Application.Commerce.Factories;
using _116.Content.Domain.Entities;
using _116.Shared.Application.Exceptions;
using _116.Tests.Fixtures.Factories.Content;
using _116.Unit.Tests.Common.Mocks.Repositories;
using AwesomeAssertions;
using Moq;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Commerce.Factories;

/// <summary>
/// Unit tests for <see cref="OrderPaymentFactory"/>.
/// </summary>
public class OrderPaymentFactoryTests
{
    private readonly Mock<_116.Content.Application.Shared.Repositories.IContentOrderRepository> _orderRepositoryMock;
    private readonly OrderPaymentFactory _factory;

    public OrderPaymentFactoryTests()
    {
        _orderRepositoryMock = MockContentOrderRepository.Create();
        _factory = new OrderPaymentFactory(_orderRepositoryMock.Object);
    }

    #region Success Cases

    [Fact]
    public async Task GetByOrderIdOrThrowAsync_WhenPaymentExists_ShouldReturnPayment()
    {
        // Arrange
        Guid orderId = Guid.NewGuid();
        ContentPaymentEntity payment = ContentPaymentFactory.Create(orderId);
        _orderRepositoryMock.SetupGetPaymentByOrderId(orderId, payment);

        // Act
        ContentPaymentEntity result = await _factory.GetByOrderIdOrThrowAsync(orderId, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(payment.Id);
        result.OrderId.Should().Be(orderId);
    }

    #endregion

    #region Failure Cases

    [Fact]
    public async Task GetByOrderIdOrThrowAsync_WhenPaymentNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        Guid orderId = Guid.NewGuid();
        _orderRepositoryMock.SetupGetPaymentByOrderId(orderId, null);

        // Act
        Func<Task> act = async () => await _factory.GetByOrderIdOrThrowAsync(orderId, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    #endregion
}
