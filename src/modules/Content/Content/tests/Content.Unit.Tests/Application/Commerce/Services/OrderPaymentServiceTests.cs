using _116.BuildingBlocks.Application.Exceptions;
using _116.Content.Application.Commerce.Services;
using _116.Content.Application.Shared.Persistence;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;
using _116.Content.TestData.Factories;
using _116.Content.TestData.Mocks.Infrastructure;
using _116.Content.TestData.Mocks.Repositories;
using _116.Content.TestData.Mocks.Services;
using _116.Storage.Contracts.Application.DTOs;
using _116.Storage.Contracts.Application.Services;
using _116.Storage.Contracts.Domain.Enums;
using _116.Storage.TestData.Factories;
using _116.Tests.TestData.Helpers;
using AwesomeAssertions;
using Microsoft.AspNetCore.Http;
using Moq;
using Xunit;

namespace _116.Content.Unit.Tests.Application.Commerce.Services;

/// <summary>
/// Unit tests for <see cref="OrderPaymentService"/>.
/// </summary>
public class OrderPaymentServiceTests
{
    private readonly Mock<IContentOrderRepository> _orderRepositoryMock;
    private readonly Mock<IFileStorageService> _fileStorageMock;
    private readonly Mock<IContentUnitOfWork> _unitOfWorkMock;
    private readonly OrderPaymentService _service;

    public OrderPaymentServiceTests()
    {
        _orderRepositoryMock = MockContentOrderRepository.Create();
        _fileStorageMock = MockFileStorageService.Create();
        _unitOfWorkMock = MockContentUnitOfWork.Create().SetupExecuteInTransaction<FileReferenceDto>();
        _service = new OrderPaymentService(
            _orderRepositoryMock.Object,
            TestErrorsFactory.CreateContentOrderErrors(),
            _fileStorageMock.Object,
            _unitOfWorkMock.Object
        );
    }

    private static IFormFile ProofFile()
    {
        Mock<IFormFile> fileMock = new();
        fileMock.Setup(f => f.ContentType).Returns("image/jpeg");
        fileMock.Setup(f => f.FileName).Returns("proof.jpg");
        return fileMock.Object;
    }

    #region AttachProofAsync Tests

    [Fact]
    public async Task AttachProofAsync_ShouldUploadRecordAndAttachTheProofInOneTransaction()
    {
        // Arrange
        ContentOrderEntity order = ContentOrderFactory.Create();
        ContentPaymentEntity payment = order.AttachPayment();
        FileReferenceDto proofFile = FileReferenceDtoFactory.CreateJpeg();
        _orderRepositoryMock.SetupGetByIdWithItems(order);
        _fileStorageMock
            .Setup(x =>
                x.UploadAsync(
                    It.IsAny<IFormFile>(),
                    order.Id.ToString(),
                    "content/payment-proofs",
                    EnumStoredFileKind.Raw,
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(StoredFileFactory.From(proofFile));
        _fileStorageMock
            .Setup(x => x.RecordAsync(It.IsAny<StoredFile>(), It.IsAny<Guid?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(proofFile);

        // Act
        FileReferenceDto result = await _service.AttachProofAsync(
            order.Id,
            ProofFile(),
            EnumPaymentMethod.BankTransfer,
            CancellationToken.None
        );

        // Assert
        result.Should().BeSameAs(proofFile);
        payment.PaymentProofFileId.Should().Be(proofFile.Id);
        payment.PaymentMethod.Should().Be(EnumPaymentMethod.BankTransfer);
        _unitOfWorkMock.VerifyExecutedInTransaction<FileReferenceDto>();
    }

    [Fact]
    public async Task AttachProofAsync_WhenPaymentNotFound_ShouldThrowWithoutUploading()
    {
        // Arrange
        Guid orderId = Guid.NewGuid();

        // Act
        Func<Task> act = async () =>
            await _service.AttachProofAsync(
                orderId,
                ProofFile(),
                EnumPaymentMethod.BankTransfer,
                CancellationToken.None
            );

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
        _fileStorageMock.Verify(
            x =>
                x.UploadAsync(
                    It.IsAny<IFormFile>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<EnumStoredFileKind>(),
                    It.IsAny<CancellationToken>()
                ),
            Times.Never
        );
        _unitOfWorkMock.VerifyExecutedInTransaction<FileReferenceDto>(0);
    }

    #endregion

    #region Success Cases

    [Fact]
    public async Task GetByOrderIdOrThrowAsync_WhenPaymentExists_ShouldReturnPayment()
    {
        // Arrange
        ContentOrderEntity order = ContentOrderFactory.Create();
        ContentPaymentEntity payment = order.AttachPayment();
        _orderRepositoryMock.SetupGetByIdWithItems(order);

        // Act
        ContentPaymentEntity result = await _service.GetByOrderIdOrThrowAsync(order.Id, CancellationToken.None);

        // Assert
        result.Id.Should().Be(payment.Id);
        result.OrderId.Should().Be(order.Id);
    }

    #endregion

    #region Failure Cases

    [Fact]
    public async Task GetByOrderIdOrThrowAsync_WhenPaymentNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        Guid orderId = Guid.NewGuid();

        // Act
        Func<Task> act = async () => await _service.GetByOrderIdOrThrowAsync(orderId, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    #endregion
}
