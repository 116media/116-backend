using _116.BuildingBlocks.Application.Exceptions;
using _116.Content.Application.Commerce.Factories;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Content.TestData.Factories;
using _116.Content.TestData.Factories.Helpers;
using _116.Content.TestData.Mocks.Factories;
using _116.Content.TestData.Mocks.Infrastructure;
using _116.Content.TestData.Mocks.Repositories;
using _116.Content.TestData.Mocks.Services;
using _116.Identity.TestData.Factories;
using _116.Identity.TestData.Mocks.Infrastructure;
using _116.Identity.TestData.Mocks.Repositories;
using _116.Identity.TestData.Mocks.Services;
using _116.Storage.TestData.Factories;
using _116.Storage.TestData.Mocks.Infrastructure;
using _116.Storage.TestData.Mocks.Services;
using _116.Tests.TestData.Helpers;
using _116.Tests.TestData.Mocks;
using AwesomeAssertions;
using Moq;
using Xunit;

namespace _116.Content.Unit.Tests.Application.Commerce.Factories;

/// <summary>
/// Unit tests for <see cref="OrderPaymentFactory"/>.
/// </summary>
public class OrderPaymentFactoryTests
{
    private readonly Mock<IContentOrderRepository> _orderRepositoryMock;
    private readonly OrderPaymentFactory _factory;

    public OrderPaymentFactoryTests()
    {
        _orderRepositoryMock = MockContentOrderRepository.Create();
        _factory = new OrderPaymentFactory(_orderRepositoryMock.Object, TestErrorsFactory.CreateContentOrderErrors());
    }

    #region Success Cases

    [Fact]
    public async Task GetByOrderIdOrThrowAsync_WhenPaymentExists_ShouldReturnPayment()
    {
        // Arrange
        ContentOrderEntity order = ContentOrderFactory.Create();
        ContentPaymentEntity payment = order.AttachPayment();
        _orderRepositoryMock.SetupGetByIdWithItems(order);

        // Act
        ContentPaymentEntity result = await _factory.GetByOrderIdOrThrowAsync(order.Id, CancellationToken.None);

        // Assert
        result.Id.Should().Be(payment.Id);
        result.OrderId.Should().Be(order.Id);
    }

    #endregion

    #region Failure Cases

    [Fact]
    public async Task GetByOrderIdOrThrowAsync_WhenPaymentNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        Guid orderId = Guid.NewGuid();

        // Act
        Func<Task> act = async () => await _factory.GetByOrderIdOrThrowAsync(orderId, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    #endregion
}
