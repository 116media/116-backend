using _116.Content.Application.Commerce.UseCases.Admin.Commands.CreateOrder;
using _116.Content.Application.Shared.Persistence;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Shared.Application.Exceptions;
using _116.Tests.Fixtures.Factories.Content;
using _116.Unit.Tests.Common.Mocks.Infrastructure;
using _116.Unit.Tests.Common.Mocks.Repositories;
using AwesomeAssertions;
using Moq;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Commerce.UseCases.Admin.Commands.CreateOrder;

/// <summary>
/// Unit tests for <see cref="AdminCreateOrderHandler"/>.
/// </summary>
public class AdminCreateOrderHandlerTests
{
    private readonly Mock<ICustomerRepository> _customerRepositoryMock;
    private readonly Mock<IPackageRepository> _packageRepositoryMock;
    private readonly Mock<IContentOrderRepository> _orderRepositoryMock;
    private readonly Mock<IContentUnitOfWork> _unitOfWorkMock;
    private readonly AdminCreateOrderHandler _handler;

    public AdminCreateOrderHandlerTests()
    {
        _customerRepositoryMock = MockCustomerRepository.Create();
        _packageRepositoryMock = MockPackageRepository.Create();
        _orderRepositoryMock = MockContentOrderRepository.Create();
        _unitOfWorkMock = MockContentUnitOfWork.Create();
        _handler = new AdminCreateOrderHandler(
            _customerRepositoryMock.Object,
            _packageRepositoryMock.Object,
            _orderRepositoryMock.Object,
            _unitOfWorkMock.Object
        );
    }

    #region Success Cases

    [Fact]
    public async Task Handle_WithoutPackage_ShouldCreateOrderAndReturnSummary()
    {
        // Arrange
        CustomerEntity customer = CustomerFactory.CreateDefault();
        _customerRepositoryMock
            .Setup(x => x.GetByIdAsync(customer.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(customer);

        var command = new AdminCreateOrderCommand(CustomerId: customer.Id.ToString(), PackageId: null);

        // Act
        AdminCreateOrderResult result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Order.Should().NotBeNull();
        result.Order.CustomerName.Should().Be(customer.FullName);
        _orderRepositoryMock.VerifyAddCalled();
        _unitOfWorkMock.VerifyCommitCalled();
    }

    [Fact]
    public async Task Handle_WithActivePackage_ShouldCreateOrderWithPackageId()
    {
        // Arrange
        CustomerEntity customer = CustomerFactory.Create();
        PackageEntity package = PackageFactory.Create();

        _customerRepositoryMock
            .Setup(x => x.GetByIdAsync(customer.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(customer);
        _packageRepositoryMock
            .Setup(x => x.GetByIdWithSlotsAsync(package.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(package);

        var command = new AdminCreateOrderCommand(CustomerId: customer.Id.ToString(), PackageId: package.Id);

        // Act
        AdminCreateOrderResult result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Order.Should().NotBeNull();
        _orderRepositoryMock.VerifyAddCalled();
        _unitOfWorkMock.VerifyCommitCalled();
    }

    #endregion

    #region Failure Cases

    [Fact]
    public async Task Handle_WhenCustomerNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        Guid customerId = Guid.NewGuid();
        _customerRepositoryMock
            .Setup(x => x.GetByIdAsync(customerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((CustomerEntity?)null);

        var command = new AdminCreateOrderCommand(CustomerId: customerId.ToString(), PackageId: null);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_WhenPackageNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        CustomerEntity customer = CustomerFactory.Create();
        Guid packageId = Guid.NewGuid();

        _customerRepositoryMock
            .Setup(x => x.GetByIdAsync(customer.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(customer);
        _packageRepositoryMock
            .Setup(x => x.GetByIdWithSlotsAsync(packageId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((PackageEntity?)null);

        var command = new AdminCreateOrderCommand(CustomerId: customer.Id.ToString(), PackageId: packageId);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    #endregion
}
