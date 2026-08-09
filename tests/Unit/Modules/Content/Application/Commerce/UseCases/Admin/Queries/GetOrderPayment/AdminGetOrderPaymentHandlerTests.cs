using _116.Content.Application.Commerce.Factories;
using _116.Content.Application.Commerce.UseCases.Admin.Queries.GetOrderPayment;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Core.Application.Shared.Repositories;
using _116.Core.Domain.Entities;
using _116.Identity.Contracts.Application;
using _116.Shared.Application.Exceptions;
using _116.Tests.Fixtures.Constants;
using _116.Tests.Fixtures.Factories.Content;
using _116.Tests.Fixtures.Factories.Core;
using _116.Unit.Tests.Common;
using _116.Unit.Tests.Common.Mocks.Factories;
using _116.Unit.Tests.Common.Mocks.Repositories;
using _116.Unit.Tests.Common.Mocks.Services;
using AwesomeAssertions;
using Moq;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Commerce.UseCases.Admin.Queries.GetOrderPayment;

/// <summary>
/// Unit tests for <see cref="AdminGetOrderPaymentHandler"/>.
/// </summary>
public class AdminGetOrderPaymentHandlerTests : BaseContentHandlerTest
{
    private readonly Mock<IOrderPaymentFactory> _orderPaymentFactoryMock;
    private readonly Mock<IContentOrderRepository> _orderRepositoryMock;
    private readonly Mock<IFileRepository> _fileRepositoryMock;
    private readonly Mock<IUserLookupService> _userLookupMock;
    private readonly AdminGetOrderPaymentHandler _handler;

    public AdminGetOrderPaymentHandlerTests()
    {
        _orderPaymentFactoryMock = MockOrderPaymentFactory.Create();
        _orderRepositoryMock = MockContentOrderRepository.Create();
        _fileRepositoryMock = MockFileRepository.Create();
        _userLookupMock = MockUserLookupService.Create();
        _handler = new AdminGetOrderPaymentHandler(
            _orderPaymentFactoryMock.Object,
            _orderRepositoryMock.Object,
            _fileRepositoryMock.Object,
            Mapper,
            _userLookupMock.Object
        );
    }

    #region Success Cases

    [Fact]
    public async Task Handle_WithProofFile_ShouldReturnPaymentWithProof()
    {
        // Arrange
        Guid orderId = Guid.NewGuid();
        Guid proofFileId = Guid.NewGuid();
        ContentPaymentEntity payment = ContentPaymentFactory.CreateWithProof(orderId, proofFileId);
        FileEntity proofFile = FileFactory.CreateWithId(proofFileId);

        _orderPaymentFactoryMock.SetupGetByOrderId(orderId, payment);
        _fileRepositoryMock.SetupGetById(proofFile);

        var query = new AdminGetOrderPaymentQuery(OrderId: orderId);

        // Act
        AdminGetOrderPaymentResult result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Payment.Should().NotBeNull();
        result.Payment.PaymentProof.Should().NotBeNull();
    }

    [Fact]
    public async Task Handle_WithoutProofFile_ShouldReturnPaymentWithNullProof()
    {
        // Arrange
        Guid orderId = Guid.NewGuid();
        ContentPaymentEntity payment = ContentPaymentFactory.Create(orderId);

        _orderPaymentFactoryMock.SetupGetByOrderId(orderId, payment);

        var query = new AdminGetOrderPaymentQuery(OrderId: orderId);

        // Act
        AdminGetOrderPaymentResult result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Payment.Should().NotBeNull();
        result.Payment.PaymentProof.Should().BeNull();

        _fileRepositoryMock.Verify(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WithVerifiedPayment_ShouldResolveVerifierUserName()
    {
        // Arrange
        Guid orderId = Guid.NewGuid();
        ContentPaymentEntity payment = ContentPaymentFactory.CreateVerified(orderId);
        Guid verifierId = payment.VerifiedById!.Value;

        _orderPaymentFactoryMock.SetupGetByOrderId(orderId, payment);
        _userLookupMock.SetupGetUserNameById(verifierId, TestConstants.User.ValidUserName);

        var query = new AdminGetOrderPaymentQuery(OrderId: orderId);

        // Act
        AdminGetOrderPaymentResult result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Payment.Should().NotBeNull();
        result.Payment.VerifiedByUserName.Should().Be(TestConstants.User.ValidUserName);
        _userLookupMock.VerifyGetUserNameByIdCalled(verifierId);
    }

    [Fact]
    public async Task Handle_WithUnverifiedPayment_ShouldNotCallUserLookup()
    {
        // Arrange
        Guid orderId = Guid.NewGuid();
        ContentPaymentEntity payment = ContentPaymentFactory.Create(orderId);

        _orderPaymentFactoryMock.SetupGetByOrderId(orderId, payment);

        var query = new AdminGetOrderPaymentQuery(OrderId: orderId);

        // Act
        AdminGetOrderPaymentResult result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Payment.VerifiedByUserName.Should().BeNull();
        _userLookupMock.VerifyGetUserNameByIdNotCalled();
    }

    #endregion

    #region Failure Cases

    [Fact]
    public async Task Handle_WhenOrderNotFound_ShouldThrowNotFoundExceptionWithoutResolvingPayment()
    {
        // Arrange
        Guid orderId = Guid.NewGuid();
        _orderRepositoryMock.SetupGetByIdOrThrowNotFound(orderId);

        var query = new AdminGetOrderPaymentQuery(OrderId: orderId);

        // Act
        Func<Task> act = async () => await _handler.Handle(query, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
        _orderPaymentFactoryMock.Verify(
            x => x.GetByOrderIdOrThrowAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
            Times.Never
        );
    }

    #endregion
}
