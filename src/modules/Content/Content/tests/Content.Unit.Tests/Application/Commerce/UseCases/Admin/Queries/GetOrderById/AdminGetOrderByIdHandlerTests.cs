using _116.BuildingBlocks.Application.Exceptions;
using _116.Content.Application.Commerce.Services;
using _116.Content.Application.Commerce.UseCases.Admin.Queries.GetOrderById;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;
using _116.Content.TestData;
using _116.Content.TestData.Builders.Entities;
using _116.Content.TestData.Factories;
using _116.Content.TestData.Mocks.Repositories;
using _116.Content.TestData.Mocks.Services;
using _116.Identity.Contracts.Application.DTOs;
using _116.Identity.Contracts.Application.Services;
using _116.Identity.TestData.Mocks.Services;
using _116.Shared.Domain.Constants;
using _116.Storage.Contracts.Application.DTOs;
using _116.Storage.Contracts.Application.Services;
using _116.Storage.TestData.Factories;
using _116.Tests.TestData.Constants;
using _116.Tests.TestData.Helpers;
using AwesomeAssertions;
using Moq;
using Xunit;

namespace _116.Content.Unit.Tests.Application.Commerce.UseCases.Admin.Queries.GetOrderById;

/// <summary>
/// Unit tests for <see cref="AdminGetOrderByIdHandler"/>.
/// </summary>
public class AdminGetOrderByIdHandlerTests : BaseContentHandlerTest
{
    private readonly Mock<IContentOrderRepository> _orderRepositoryMock;
    private readonly Mock<IFileStorageService> _fileStorageMock;
    private readonly Mock<IUserLookupService> _userLookupMock;
    private readonly AdminGetOrderByIdHandler _handler;

    public AdminGetOrderByIdHandlerTests()
    {
        _orderRepositoryMock = MockContentOrderRepository.Create();
        _fileStorageMock = MockFileStorageService.Create();
        _userLookupMock = MockUserLookupService.Create();
        _handler = new AdminGetOrderByIdHandler(
            _orderRepositoryMock.Object,
            CreateOrderDtoService(),
            new PaymentDtoService(Mapper, _userLookupMock.Object, CreateOrderDtoService(), _fileStorageMock.Object),
            TestErrorsFactory.CreateContentI18n()
        );
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
        result.Order.Id.Should().Be(order.Id);
        result.Order.Payment.Should().BeNull();
    }

    [Fact]
    public async Task Handle_WithVerifiedPayment_ShouldResolveVerifierUserName()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        Guid proofFileId = Guid.NewGuid();
        ContentOrderEntity order = new ContentOrderBuilder().WithId(orderId).WithPayment().Build();
        ContentPaymentEntity payment = order.Payment!;
        payment.AttachProof(proofFileId: proofFileId, paymentMethod: EnumPaymentMethod.BankTransfer);
        payment.Verify(
            adminUserId: Guid.NewGuid(),
            receiptUrl: TestConstants.Commerce.ValidReceiptUrl,
            now: TestConstants.Clock.Instant
        );
        Guid verifierId = payment.VerifiedById!.Value;

        FileReferenceDto proofFile = FileReferenceDtoFactory.CreateWithId(proofFileId);

        _orderRepositoryMock.SetupGetByIdWithItems(order);
        _fileStorageMock.SetupResolve(proofFile);
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

        var query = new AdminGetOrderByIdQuery(Id: orderId);

        // Act
        AdminGetOrderByIdResult result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Order.Payment.Should().NotBeNull();
        result.Order.Payment!.VerifiedByUserName.Should().Be(TestConstants.User.ValidUserName);
        _userLookupMock.VerifyGetUserProfilesByIdsCalledOnce();
    }

    [Fact]
    public async Task Handle_WithUnverifiedPayment_ShouldNotCallUserLookup()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        Guid proofFileId = Guid.NewGuid();
        ContentOrderEntity order = new ContentOrderBuilder().WithId(orderId).WithPayment().Build();
        order.Payment!.AttachProof(proofFileId: proofFileId, paymentMethod: EnumPaymentMethod.BankTransfer);

        FileReferenceDto proofFile = FileReferenceDtoFactory.CreateWithId(proofFileId);

        _orderRepositoryMock.SetupGetByIdWithItems(order);
        _fileStorageMock.SetupResolve(proofFile);

        var query = new AdminGetOrderByIdQuery(Id: orderId);

        // Act
        AdminGetOrderByIdResult result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Order.Payment.Should().NotBeNull();
        result.Order.Payment!.VerifiedByUserName.Should().BeNull();
        _userLookupMock.VerifyGetUserNameByIdNotCalled();
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
