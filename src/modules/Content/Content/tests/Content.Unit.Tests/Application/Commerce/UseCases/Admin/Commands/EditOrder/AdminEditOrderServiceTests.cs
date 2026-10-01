using _116.BuildingBlocks.Application.Exceptions;
using _116.Content.Application.Commerce.UseCases.Admin.Commands.EditOrder;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Content.TestData.Factories;
using _116.Content.TestData.Mocks.Repositories;
using _116.Tests.TestData.Helpers;
using AwesomeAssertions;
using Moq;
using Xunit;

namespace _116.Content.Unit.Tests.Application.Commerce.UseCases.Admin.Commands.EditOrder;

/// <summary>
/// Unit tests for <see cref="AdminEditOrderService"/>: the order and customer gates and the update.
/// </summary>
public class AdminEditOrderServiceTests
{
    private readonly Mock<IContentOrderRepository> _orderRepositoryMock = MockContentOrderRepository.Create();
    private readonly Mock<ICustomerRepository> _customerRepositoryMock = MockCustomerRepository.Create();
    private readonly AdminEditOrderService _service;

    public AdminEditOrderServiceTests()
    {
        _service = new AdminEditOrderService(
            _orderRepositoryMock.Object,
            _customerRepositoryMock.Object,
            TestErrorsFactory.CreateContentI18n()
        );
    }

    [Fact]
    public async Task EditAsync_WithAnExistingCustomer_ShouldReassignTheOrder()
    {
        // Arrange
        ContentOrderEntity order = ContentOrderFactory.Create();
        CustomerEntity customer = CustomerFactory.Create();
        _orderRepositoryMock.SetupGetByIdWithItems(order);
        _customerRepositoryMock.SetupGetByIdOrThrow(customer);

        // Act
        ContentOrderEntity result = await _service.EditAsync(order.Id, customer.Id, null, CancellationToken.None);

        // Assert
        result.Should().BeSameAs(order);
        order.CustomerId.Should().Be(customer.Id);
    }

    [Fact]
    public async Task EditAsync_WhenTheOrderDoesNotExist_ShouldThrowNotFoundException()
    {
        // Arrange
        Guid orderId = Guid.NewGuid();
        _orderRepositoryMock.SetupGetByIdWithItems(null);

        // Act
        Func<Task> act = async () => await _service.EditAsync(orderId, null, null, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task EditAsync_WhenTheCustomerDoesNotExist_ShouldThrowNotFoundException()
    {
        // Arrange
        ContentOrderEntity order = ContentOrderFactory.Create();
        Guid customerId = Guid.NewGuid();
        _orderRepositoryMock.SetupGetByIdWithItems(order);
        _customerRepositoryMock.SetupGetByIdOrThrowNotFound(customerId);

        // Act
        Func<Task> act = async () => await _service.EditAsync(order.Id, customerId, null, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }
}
