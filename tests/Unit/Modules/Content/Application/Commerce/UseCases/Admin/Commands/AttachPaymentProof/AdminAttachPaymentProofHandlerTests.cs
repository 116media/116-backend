using _116.Content.Application.Commerce.Factories;
using _116.Content.Application.Commerce.UseCases.Admin.Commands.AttachPaymentProof;
using _116.Content.Application.Shared.Persistence;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;
using _116.Core.Application.Shared.Services;
using _116.Core.Contracts.Application.DTOs;
using _116.Core.Contracts.Application.Services;
using _116.Core.Contracts.Domain.Enums;
using _116.Core.Domain.Entities;
using _116.Shared.Application.Exceptions;
using _116.Tests.Fixtures.Factories.Content;
using _116.Tests.Fixtures.Factories.Core;
using _116.Tests.Fixtures.Helpers;
using _116.Unit.Tests.Common;
using _116.Unit.Tests.Common.Mocks.Factories;
using _116.Unit.Tests.Common.Mocks.Infrastructure;
using _116.Unit.Tests.Common.Mocks.Repositories;
using _116.Unit.Tests.Common.Mocks.Services;
using AwesomeAssertions;
using Microsoft.AspNetCore.Http;
using Moq;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Commerce.UseCases.Admin.Commands.AttachPaymentProof;

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
