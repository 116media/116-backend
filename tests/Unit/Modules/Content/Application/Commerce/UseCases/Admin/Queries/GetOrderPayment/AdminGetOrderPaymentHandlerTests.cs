using _116.Content.Application.Commerce.Factories;
using _116.Content.Application.Commerce.UseCases.Admin.Queries.GetOrderPayment;
using _116.Content.Domain.Entities;
using _116.Core.Application.Shared.Repositories;
using _116.Core.Domain.Entities;
using _116.Tests.Fixtures.Factories;
using _116.Tests.Fixtures.Factories.Content;
using _116.Unit.Tests.Common;
using _116.Unit.Tests.Common.Mocks.Factories;
using _116.Unit.Tests.Common.Mocks.Repositories;
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
    private readonly Mock<IFileRepository> _fileRepositoryMock;
    private readonly AdminGetOrderPaymentHandler _handler;

    public AdminGetOrderPaymentHandlerTests()
    {
        _orderPaymentFactoryMock = MockOrderPaymentFactory.Create();
        _fileRepositoryMock = MockFileRepository.Create();
        _handler = new AdminGetOrderPaymentHandler(_orderPaymentFactoryMock.Object, _fileRepositoryMock.Object, Mapper);
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

        var query = new AdminGetOrderPaymentQuery(OrderId: orderId.ToString());

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

        var query = new AdminGetOrderPaymentQuery(OrderId: orderId.ToString());

        // Act
        AdminGetOrderPaymentResult result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Payment.Should().NotBeNull();
        result.Payment.PaymentProof.Should().BeNull();

        _fileRepositoryMock.Verify(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    #endregion
}
