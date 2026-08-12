using _116.Content.Application.Commerce.UseCases.Admin.Queries.GetOrderById;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;
using _116.Core.Application.Shared.Repositories;
using _116.Core.Domain.Entities;
using _116.Identity.Contracts.Application;
using _116.Shared.Application.Exceptions;
using _116.Tests.Fixtures.Builders.Entities.Content;
using _116.Tests.Fixtures.Constants;
using _116.Tests.Fixtures.Factories.Content;
using _116.Tests.Fixtures.Factories.Core;
using _116.Tests.Fixtures.Helpers;
using _116.Unit.Tests.Common;
using _116.Unit.Tests.Common.Mocks.Repositories;
using _116.Unit.Tests.Common.Mocks.Services;
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
    private readonly Mock<IUserLookupService> _userLookupMock;
    private readonly AdminGetOrderByIdHandler _handler;

    public AdminGetOrderByIdHandlerTests()
    {
        _orderRepositoryMock = MockContentOrderRepository.Create();
        _fileRepositoryMock = MockFileRepository.Create();
        _userLookupMock = MockUserLookupService.Create();
        _handler = new AdminGetOrderByIdHandler(
            _orderRepositoryMock.Object,
            _fileRepositoryMock.Object,
            Mapper,
            _userLookupMock.Object,
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
        ContentPaymentEntity payment = new ContentPaymentBuilder()
            .WithOrderId(orderId)
            .WithProofFileId(proofFileId, EnumPaymentMethod.BankTransfer)
            .AsVerified(Guid.NewGuid(), TestConstants.Commerce.ValidReceiptUrl)
            .Build();
        Guid verifierId = payment.VerifiedById!.Value;
        ContentOrderEntity order = new ContentOrderBuilder().WithId(orderId).WithPayment(payment).Build();

        FileEntity proofFile = FileFactory.CreateWithId(proofFileId);

        _orderRepositoryMock.SetupGetByIdWithItems(order);
        _fileRepositoryMock.SetupGetById(proofFile);
        _userLookupMock.SetupGetUserNameById(verifierId, TestConstants.User.ValidUserName);

        var query = new AdminGetOrderByIdQuery(Id: orderId);

        // Act
        AdminGetOrderByIdResult result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Order.Payment.Should().NotBeNull();
        result.Order.Payment!.VerifiedByUserName.Should().Be(TestConstants.User.ValidUserName);
        _userLookupMock.VerifyGetUserNameByIdCalled(verifierId);
    }

    [Fact]
    public async Task Handle_WithUnverifiedPayment_ShouldNotCallUserLookup()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        Guid proofFileId = Guid.NewGuid();
        ContentPaymentEntity payment = new ContentPaymentBuilder()
            .WithOrderId(orderId)
            .WithProofFileId(proofFileId, EnumPaymentMethod.BankTransfer)
            .Build();
        ContentOrderEntity order = new ContentOrderBuilder().WithId(orderId).WithPayment(payment).Build();

        FileEntity proofFile = FileFactory.CreateWithId(proofFileId);

        _orderRepositoryMock.SetupGetByIdWithItems(order);
        _fileRepositoryMock.SetupGetById(proofFile);

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
