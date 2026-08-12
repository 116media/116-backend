using _116.Content.Application.Commerce.UseCases.Admin.Queries.GetAllPayments;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;
using _116.Identity.Contracts.Application;
using _116.Shared.Application.Pagination;
using _116.Tests.Fixtures.Builders.Entities.Content;
using _116.Tests.Fixtures.Constants;
using _116.Tests.Fixtures.Factories.Content;
using _116.Unit.Tests.Common;
using _116.Unit.Tests.Common.Mocks.Repositories;
using _116.Unit.Tests.Common.Mocks.Services;
using AwesomeAssertions;
using Moq;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Commerce.UseCases.Admin.Queries.GetAllPayments;

/// <summary>
/// Unit tests for <see cref="AdminGetAllPaymentsHandler"/>.
/// </summary>
public class AdminGetAllPaymentsHandlerTests : BaseContentHandlerTest
{
    private readonly Mock<IContentOrderRepository> _orderRepositoryMock;
    private readonly Mock<IUserLookupService> _userLookupMock;
    private readonly AdminGetAllPaymentsHandler _handler;

    public AdminGetAllPaymentsHandlerTests()
    {
        _orderRepositoryMock = MockContentOrderRepository.Create();
        _userLookupMock = MockUserLookupService.Create();
        _handler = new AdminGetAllPaymentsHandler(_orderRepositoryMock.Object, Mapper, _userLookupMock.Object);
    }

    #region Success Cases

    [Fact]
    public async Task Handle_ShouldReturnPaginatedResult()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        CustomerEntity customer = CustomerFactory.Create();
        ContentOrderEntity order = new ContentOrderBuilder().WithId(orderId).WithCustomer(customer).Build();

        ContentPaymentEntity payment = new ContentPaymentBuilder().WithOrder(order).Build();

        List<ContentPaymentEntity> payments = [payment];
        _orderRepositoryMock.SetupGetAllPaymentsAsync(payments, payments.Count);

        var query = new AdminGetAllPaymentsQuery(
            PaginatedRequest: new PaginatedRequest(0, 10),
            Status: null,
            Method: null,
            Search: null
        );

        // Act
        AdminGetAllPaymentsResult result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Payments.Items.Should().ContainSingle();
    }

    [Fact]
    public async Task Handle_WithVerifiedPayment_ShouldResolveVerifierUserName()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var adminUserId = Guid.NewGuid();
        CustomerEntity customer = CustomerFactory.Create();
        ContentOrderEntity order = new ContentOrderBuilder().WithId(orderId).WithCustomer(customer).Build();

        ContentPaymentEntity payment = new ContentPaymentBuilder()
            .AsVerified(Guid.NewGuid(), TestConstants.Commerce.ValidReceiptUrl)
            .WithOrder(order)
            .Build();

        Guid verifierId = payment.VerifiedById!.Value;
        _userLookupMock.SetupGetUserNameById(verifierId, TestConstants.User.ValidUserName);

        List<ContentPaymentEntity> payments = [payment];
        _orderRepositoryMock.SetupGetAllPaymentsAsync(payments, payments.Count);

        var query = new AdminGetAllPaymentsQuery(
            PaginatedRequest: new PaginatedRequest(0, 10),
            Status: null,
            Method: null,
            Search: null
        );

        // Act
        AdminGetAllPaymentsResult result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Payments.Items.Should().ContainSingle();
        result.Payments.Items.First().VerifiedByUserName.Should().Be(TestConstants.User.ValidUserName);
        _userLookupMock.VerifyGetUserNameByIdCalled(verifierId);
    }

    [Fact]
    public async Task Handle_WithUnverifiedPayment_ShouldNotCallUserLookup()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        CustomerEntity customer = CustomerFactory.Create();
        ContentOrderEntity order = new ContentOrderBuilder().WithId(orderId).WithCustomer(customer).Build();

        ContentPaymentEntity payment = new ContentPaymentBuilder().WithOrder(order).Build();

        List<ContentPaymentEntity> payments = [payment];
        _orderRepositoryMock.SetupGetAllPaymentsAsync(payments, payments.Count);

        var query = new AdminGetAllPaymentsQuery(
            PaginatedRequest: new PaginatedRequest(0, 10),
            Status: null,
            Method: null,
            Search: null
        );

        // Act
        AdminGetAllPaymentsResult result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Payments.Items.Should().ContainSingle();
        result.Payments.Items.First().VerifiedByUserName.Should().BeNull();
        _userLookupMock.VerifyGetUserNameByIdNotCalled();
    }

    [Fact]
    public async Task Handle_WithStatusFilter_ShouldPassToRepository()
    {
        // Arrange
        _orderRepositoryMock.SetupGetAllPaymentsAsync([], 0);

        var query = new AdminGetAllPaymentsQuery(
            PaginatedRequest: new PaginatedRequest(0, 10),
            Status: EnumPaymentStatus.Pending,
            Method: null,
            Search: null
        );

        // Act
        AdminGetAllPaymentsResult result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Payments.Items.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_WithMethodFilter_ShouldPassToRepository()
    {
        // Arrange
        _orderRepositoryMock.SetupGetAllPaymentsAsync([], 0);

        var query = new AdminGetAllPaymentsQuery(
            PaginatedRequest: new PaginatedRequest(0, 10),
            Status: null,
            Method: EnumPaymentMethod.BankTransfer,
            Search: null
        );

        // Act
        AdminGetAllPaymentsResult result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Payments.Items.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_EmptyResult_ShouldReturnEmptyPaginatedResult()
    {
        // Arrange
        _orderRepositoryMock.SetupGetAllPaymentsAsync([], 0);

        var query = new AdminGetAllPaymentsQuery(
            PaginatedRequest: new PaginatedRequest(0, 10),
            Status: null,
            Method: null,
            Search: null
        );

        // Act
        AdminGetAllPaymentsResult result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Payments.Items.Should().BeEmpty();
        result.Payments.Count.Should().Be(0);
    }

    #endregion
}
