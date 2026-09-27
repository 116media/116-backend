using _116.BuildingBlocks.Application.Exceptions;
using _116.Content.Application.Commerce.Factories;
using _116.Content.Application.Commerce.UseCases.Admin.Commands.AttachPaymentProof;
using _116.Content.Application.Shared.Persistence;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;
using _116.Content.TestData;
using _116.Content.TestData.Factories;
using _116.Content.TestData.Factories.Helpers;
using _116.Content.TestData.Mocks.Factories;
using _116.Content.TestData.Mocks.Infrastructure;
using _116.Content.TestData.Mocks.Repositories;
using _116.Content.TestData.Mocks.Services;
using _116.Identity.TestData.Factories;
using _116.Identity.TestData.Mocks.Infrastructure;
using _116.Identity.TestData.Mocks.Repositories;
using _116.Identity.TestData.Mocks.Services;
using _116.Storage.Application.Shared.Services;
using _116.Storage.Contracts.Application.DTOs;
using _116.Storage.Contracts.Application.Services;
using _116.Storage.Contracts.Domain.Enums;
using _116.Storage.Domain.Entities;
using _116.Storage.TestData.Factories;
using _116.Storage.TestData.Mocks.Infrastructure;
using _116.Storage.TestData.Mocks.Services;
using _116.Tests.TestData;
using _116.Tests.TestData.Helpers;
using _116.Tests.TestData.Mocks;
using AwesomeAssertions;
using Microsoft.AspNetCore.Http;
using Moq;
using Xunit;

namespace _116.Content.Unit.Tests.Application.Commerce.UseCases.Admin.Commands.AttachPaymentProof;

/// <summary>
/// Unit tests for <see cref="AdminAttachPaymentProofHandler"/>.
/// </summary>
public class AdminAttachPaymentProofHandlerTests : BaseContentHandlerTest
{
    private readonly Mock<IFileStorageService> _fileStorageMock;
    private readonly Mock<IOrderPaymentFactory> _orderPaymentFactoryMock;
    private readonly Mock<IContentOrderRepository> _orderRepositoryMock;
    private readonly Mock<IContentUnitOfWork> _unitOfWorkMock;
    private readonly AdminAttachPaymentProofHandler _handler;

    public AdminAttachPaymentProofHandlerTests()
    {
        _fileStorageMock = MockFileStorageService.Create();
        _orderPaymentFactoryMock = MockOrderPaymentFactory.Create();
        _orderRepositoryMock = MockContentOrderRepository.Create();
        _unitOfWorkMock = MockContentUnitOfWork.Create().SetupExecuteInTransaction<FileReferenceDto>();
        _handler = new AdminAttachPaymentProofHandler(
            _orderPaymentFactoryMock.Object,
            _fileStorageMock.Object,
            _orderRepositoryMock.Object,
            _unitOfWorkMock.Object,
            Mapper
        );
    }

    #region Success Cases

    [Fact]
    public async Task Handle_ShouldAttachProofAndReturnProofDto()
    {
        // Arrange
        Guid orderId = Guid.NewGuid();
        ContentPaymentEntity payment = ContentPaymentFactory.Create(orderId);
        FileReferenceDto proofFile = FileReferenceDtoFactory.CreateJpeg();

        _orderPaymentFactoryMock.SetupGetByOrderId(orderId, payment);
        _fileStorageMock
            .Setup(x =>
                x.UploadAsync(
                    It.IsAny<IFormFile>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<EnumStoredFileKind>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(StoredFileFactory.From(proofFile));
        _fileStorageMock
            .Setup(x => x.RecordAsync(It.IsAny<StoredFile>(), It.IsAny<Guid?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(proofFile);

        Mock<IFormFile> fileMock = new();
        fileMock.Setup(f => f.ContentType).Returns("image/jpeg");
        fileMock.Setup(f => f.FileName).Returns("proof.jpg");

        var command = new AdminAttachPaymentProofCommand(
            OrderId: orderId.ToString(),
            File: fileMock.Object,
            PaymentMethod: EnumPaymentMethod.BankTransfer
        );

        // Act
        AdminAttachPaymentProofResult result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        payment.PaymentProofFileId.Should().Be(proofFile.Id);
        payment.PaymentMethod.Should().Be(EnumPaymentMethod.BankTransfer);
        result.Proof.Id.Should().Be(proofFile.Id);
        result.Proof.OriginalFileName.Should().Be(proofFile.OriginalFileName);
        result.Proof.StorageUrl.Should().Be(proofFile.StorageUrl);
        _unitOfWorkMock.VerifyExecutedInTransaction<FileReferenceDto>();
    }

    #endregion

    #region Failure Cases

    [Fact]
    public async Task Handle_WhenPaymentNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        Guid orderId = Guid.NewGuid();
        _orderPaymentFactoryMock.SetupGetByOrderIdNotFound(orderId);

        Mock<IFormFile> fileMock = new();
        fileMock.Setup(f => f.ContentType).Returns("image/jpeg");
        fileMock.Setup(f => f.FileName).Returns("proof.jpg");

        var command = new AdminAttachPaymentProofCommand(
            OrderId: orderId.ToString(),
            File: fileMock.Object,
            PaymentMethod: EnumPaymentMethod.BankTransfer
        );

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
        _unitOfWorkMock.VerifyExecutedInTransaction<FileReferenceDto>(0);
    }

    [Fact]
    public async Task Handle_WhenOrderNotFound_ShouldThrowNotFoundExceptionWithoutUploading()
    {
        // Arrange
        Guid orderId = Guid.NewGuid();
        _orderRepositoryMock.SetupGetByIdOrThrowNotFound(orderId);

        Mock<IFormFile> fileMock = new();
        fileMock.Setup(f => f.ContentType).Returns("image/jpeg");
        fileMock.Setup(f => f.FileName).Returns("proof.jpg");

        var command = new AdminAttachPaymentProofCommand(
            OrderId: orderId.ToString(),
            File: fileMock.Object,
            PaymentMethod: EnumPaymentMethod.BankTransfer
        );

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
        _orderPaymentFactoryMock.Verify(
            x => x.GetByOrderIdOrThrowAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
            Times.Never
        );

        _unitOfWorkMock.VerifyExecutedInTransaction<FileReferenceDto>(0);
    }

    #endregion
}
