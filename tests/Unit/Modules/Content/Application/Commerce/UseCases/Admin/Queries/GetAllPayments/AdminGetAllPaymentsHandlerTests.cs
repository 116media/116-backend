using _116.Content.Application.Commerce.Factories;
using _116.Content.Application.Commerce.UseCases.Admin.Queries.GetAllPayments;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;
using _116.Identity.Contracts.Application.DTOs;
using _116.Identity.Contracts.Application.Services;
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
    private readonly CustomerEntity _customer = CustomerFactory.Create();
    private readonly Mock<IContentOrderRepository> _orderRepositoryMock;
    private readonly Mock<IUserLookupService> _userLookupMock;
    private readonly AdminGetAllPaymentsHandler _handler;

    public AdminGetAllPaymentsHandlerTests()
    {
        _orderRepositoryMock = MockContentOrderRepository.Create();
        _userLookupMock = MockUserLookupService.Create();
        _handler = new AdminGetAllPaymentsHandler(
            _orderRepositoryMock.Object,
            new PaymentDtoFactory(Mapper, _userLookupMock.Object, CreateOrderDtoFactory(_customer))
        );
    }

    #region Success Cases

    [Fact]
    public async Task Handle_ShouldReturnPaginatedResult()
    {
        // Arrange
        ContentOrderEntity order = new ContentOrderBuilder().WithCustomer(_customer).Build();
        order.AttachPayment();

        List<ContentOrderEntity> orders = [order];
        _orderRepositoryMock.SetupGetOrdersWithPaymentAsync(orders, orders.Count);

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
        ContentOrderEntity order = new ContentOrderBuilder().WithCustomer(_customer).Build();
        ContentPaymentEntity payment = order.AttachPayment();
        payment.AttachProof(proofFileId: Guid.NewGuid(), paymentMethod: EnumPaymentMethod.BankTransfer);
        payment.Verify(
            adminUserId: Guid.NewGuid(),
            receiptUrl: TestConstants.Commerce.ValidReceiptUrl,
            now: TestConstants.Clock.Instant
        );

        Guid verifierId = payment.VerifiedById!.Value;
        _userLookupMock.SetupGetAuthorInfosByIds(
            new Dictionary<Guid, AuthorDto>
            {
                [verifierId] = new(TestConstants.User.ValidUserName, Email: null, AvatarFileId: null, Role: null),
            }
        );

        List<ContentOrderEntity> orders = [order];
        _orderRepositoryMock.SetupGetOrdersWithPaymentAsync(orders, orders.Count);

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
        _userLookupMock.VerifyGetAuthorInfosByIdsCalledOnce();
    }

    [Fact]
    public async Task Handle_WithUnverifiedPayment_ShouldNotCallUserLookup()
    {
        // Arrange
        ContentOrderEntity order = new ContentOrderBuilder().WithCustomer(_customer).Build();
        order.AttachPayment();

        List<ContentOrderEntity> orders = [order];
        _orderRepositoryMock.SetupGetOrdersWithPaymentAsync(orders, orders.Count);

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
        _orderRepositoryMock.SetupGetOrdersWithPaymentAsync([], 0);

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
        _orderRepositoryMock.SetupGetOrdersWithPaymentAsync([], 0);

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
        _orderRepositoryMock.SetupGetOrdersWithPaymentAsync([], 0);

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
