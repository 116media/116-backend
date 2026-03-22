using _116.Content.Application.Commerce.UseCases.Admin.Queries.GetOrderById;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Core.Application.Shared.Repositories;
using _116.Shared.Application.Exceptions;
using _116.Tests.Fixtures.Factories.Content;
using _116.Unit.Tests.Common;
using _116.Unit.Tests.Common.Mocks.Repositories;
using AwesomeAssertions;
using Moq;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Commerce.UseCases.Admin.Queries.GetOrderById;

/// <summary>
/// Unit tests for <see cref="AdminGetOrderByIdHandler"/>.
/// </summary>
public class AdminGetOrderByIdHandlerTests : BaseContentHandlerTest
{
    private readonly Mock<IContentOrderRepository> _orderRepositoryMock;
    private readonly Mock<IFileRepository> _fileRepositoryMock;
    private readonly AdminGetOrderByIdHandler _handler;

    public AdminGetOrderByIdHandlerTests()
    {
        _orderRepositoryMock = MockContentOrderRepository.Create();
        _fileRepositoryMock = MockFileRepository.Create();
        _handler = new AdminGetOrderByIdHandler(_orderRepositoryMock.Object, _fileRepositoryMock.Object, Mapper);
    }

    #region Success Cases

    [Fact]
    public async Task Handle_WithoutPayment_ShouldReturnOrderDtoWithNullPayment()
    {
        // Arrange
        ContentOrderEntity order = ContentOrderFactory.Create();
        _orderRepositoryMock.SetupGetByIdWithItems(order);

        var query = new AdminGetOrderByIdQuery(Id: order.Id);

        // Act
        AdminGetOrderByIdResult result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Order.Should().NotBeNull();
        result.Order.Id.Should().Be(order.Id);
        result.Order.Payment.Should().BeNull();
    }

    #endregion

    #region Failure Cases

    [Fact]
    public async Task Handle_WhenOrderNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        _orderRepositoryMock.SetupGetByIdWithItems(null);

        var query = new AdminGetOrderByIdQuery(Id: Guid.NewGuid());

        // Act
        Func<Task> act = async () => await _handler.Handle(query, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    #endregion
}
