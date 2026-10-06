using _116.BuildingBlocks.Application.Pagination;
using _116.Content.Application.Commerce.Services;
using _116.Content.Application.Commerce.UseCases.Admin.Queries.GetAllPayments;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;
using _116.Content.TestData;
using _116.Content.TestData.Factories;
using _116.Content.TestData.Mocks.Repositories;
using _116.Content.TestData.Mocks.Services;
using _116.Identity.Contracts.Application.DTOs;
using _116.Identity.Contracts.Application.Services;
using _116.Identity.TestData.Mocks.Services;
using _116.Shared.Domain.Constants;
using _116.Tests.TestData.Constants;
using AwesomeAssertions;
using Moq;
using Xunit;

namespace _116.Content.Unit.Tests.Application.Commerce.UseCases.Admin.Queries.GetAllPayments;

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
            new PaymentDtoService(
                Mapper,
                _userLookupMock.Object,
                CreateOrderDtoService(_customer),
                MockFileStorageService.Create().Object
            )
        );
    }

    #region Success Cases

    [Fact]
    public async Task Handle_ShouldReturnPaginatedResult()
    {
        // Arrange
        ContentOrderEntity order = ContentOrderFactory.CreateForCustomer(_customer.Id);
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
        ContentOrderEntity order = ContentOrderFactory.CreateForCustomer(_customer.Id);
        ContentPaymentEntity payment = order.AttachPayment();
        payment.AttachProof(proofFileId: Guid.NewGuid(), paymentMethod: EnumPaymentMethod.BankTransfer);
        payment.Verify(
            adminUserId: Guid.NewGuid(),
            receiptUrl: TestConstants.Commerce.ValidReceiptUrl,
            now: TestConstants.Clock.Instant
        );

        Guid verifierId = payment.VerifiedById!.Value;
        _userLookupMock.SetupGetUserProfilesByIds(
            new Dictionary<Guid, UserProfileDto>
            {
                [verifierId] = new(
                    TestConstants.User.ValidUserName,
                    Email: null,
                    AvatarFileId: null,
                    Role: null,
                    PreferredLocale: LocaleConstants.DefaultLocale
                ),
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
        _userLookupMock.VerifyGetUserProfilesByIdsCalledOnce();
    }

    [Fact]
    public async Task Handle_WithUnverifiedPayment_ShouldNotCallUserLookup()
    {
        // Arrange
        ContentOrderEntity order = ContentOrderFactory.CreateForCustomer(_customer.Id);
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
