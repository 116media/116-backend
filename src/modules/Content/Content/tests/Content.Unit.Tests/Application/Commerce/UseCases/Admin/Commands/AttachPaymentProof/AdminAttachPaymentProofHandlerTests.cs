using _116.BuildingBlocks.Application.Exceptions;
using _116.Content.Application.Commerce.Services;
using _116.Content.Application.Commerce.UseCases.Admin.Commands.AttachPaymentProof;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Enums;
using _116.Content.TestData;
using _116.Content.TestData.Mocks.Repositories;
using _116.Content.TestData.Mocks.Services;
using _116.Storage.Contracts.Application.DTOs;
using _116.Storage.TestData.Factories;
using _116.Tests.TestData.Helpers;
using AwesomeAssertions;
using Microsoft.AspNetCore.Http;
using Moq;
using Xunit;

namespace _116.Content.Unit.Tests.Application.Commerce.UseCases.Admin.Commands.AttachPaymentProof;

/// <summary>
/// Unit tests for <see cref="AdminAttachPaymentProofHandler"/>: the order gate, the attach call and
/// the response. The upload and the transaction are covered by <c>OrderPaymentServiceTests</c>.
/// </summary>
public class AdminAttachPaymentProofHandlerTests : BaseContentHandlerTest
{
    private readonly Mock<IOrderPaymentService> _orderPaymentServiceMock;
    private readonly Mock<IContentOrderRepository> _orderRepositoryMock;
    private readonly AdminAttachPaymentProofHandler _handler;

    public AdminAttachPaymentProofHandlerTests()
    {
        _orderPaymentServiceMock = MockOrderPaymentService.Create();
        _orderRepositoryMock = MockContentOrderRepository.Create();
        _handler = new AdminAttachPaymentProofHandler(
            _orderPaymentServiceMock.Object,
            _orderRepositoryMock.Object,
            Mapper
        );
    }

    private static AdminAttachPaymentProofCommand Command(Guid orderId)
    {
        Mock<IFormFile> fileMock = new();
        fileMock.Setup(f => f.ContentType).Returns("image/jpeg");
        fileMock.Setup(f => f.FileName).Returns("proof.jpg");

        return new AdminAttachPaymentProofCommand(
            OrderId: orderId.ToString(),
            File: fileMock.Object,
            PaymentMethod: EnumPaymentMethod.BankTransfer
        );
    }

    [Fact]
    public async Task Handle_ShouldAttachTheProofAndReturnItsDto()
    {
        // Arrange
        Guid orderId = Guid.NewGuid();
        FileReferenceDto proofFile = FileReferenceDtoFactory.CreateJpeg();
        AdminAttachPaymentProofCommand command = Command(orderId);

        _orderPaymentServiceMock
            .Setup(x =>
                x.AttachProofAsync(
                    orderId,
                    command.File!,
                    EnumPaymentMethod.BankTransfer,
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(proofFile);

        // Act
        AdminAttachPaymentProofResult result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Proof.Id.Should().Be(proofFile.Id);
        result.Proof.OriginalFileName.Should().Be(proofFile.OriginalFileName);
        result.Proof.StorageUrl.Should().Be(proofFile.StorageUrl);
    }

    [Fact]
    public async Task Handle_WhenPaymentNotFound_ShouldPropagateTheNotFoundException()
    {
        // Arrange
        Guid orderId = Guid.NewGuid();
        _orderPaymentServiceMock
            .Setup(x =>
                x.AttachProofAsync(
                    orderId,
                    It.IsAny<IFormFile>(),
                    It.IsAny<EnumPaymentMethod>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .ThrowsAsync(TestErrorsFactory.CreateContentOrderErrors().PaymentNotFound(orderId));

        // Act
        Func<Task> act = async () => await _handler.Handle(Command(orderId), CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_WhenOrderNotFound_ShouldThrowNotFoundExceptionWithoutAttaching()
    {
        // Arrange
        Guid orderId = Guid.NewGuid();
        _orderRepositoryMock.SetupGetByIdOrThrowNotFound(orderId);

        // Act
        Func<Task> act = async () => await _handler.Handle(Command(orderId), CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
        _orderPaymentServiceMock.Verify(
            x =>
                x.AttachProofAsync(
                    It.IsAny<Guid>(),
                    It.IsAny<IFormFile>(),
                    It.IsAny<EnumPaymentMethod>(),
                    It.IsAny<CancellationToken>()
                ),
            Times.Never
        );
    }
}
