using _116.Content.Application.Commerce.Factories;
using _116.Content.Application.Commerce.UseCases.Admin.Commands.AttachPaymentProof;
using _116.Content.Application.Shared.Persistence;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;
using _116.Core.Application.Shared.Repositories;
using _116.Core.Domain.Entities;
using _116.Shared.Application.Exceptions;
using _116.Tests.Fixtures.Factories;
using _116.Tests.Fixtures.Factories.Content;
using _116.Unit.Tests.Common;
using _116.Unit.Tests.Common.Mocks.Factories;
using _116.Unit.Tests.Common.Mocks.Infrastructure;
using _116.Unit.Tests.Common.Mocks.Repositories;
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
    private readonly Mock<IOrderPaymentFactory> _orderPaymentFactoryMock;
    private readonly Mock<IFileRepository> _fileRepositoryMock;
    private readonly Mock<IContentOrderRepository> _orderRepositoryMock;
    private readonly Mock<IContentUnitOfWork> _unitOfWorkMock;
    private readonly AdminAttachPaymentProofHandler _handler;

    public AdminAttachPaymentProofHandlerTests()
    {
        _orderPaymentFactoryMock = MockOrderPaymentFactory.Create();
        _fileRepositoryMock = MockFileRepository.Create();
        _orderRepositoryMock = MockContentOrderRepository.Create();
        _unitOfWorkMock = MockContentUnitOfWork.Create();
        _handler = new AdminAttachPaymentProofHandler(
            _orderPaymentFactoryMock.Object,
            _fileRepositoryMock.Object,
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
        FileEntity proofFile = FileFactory.CreateJpeg();

        _orderPaymentFactoryMock.SetupGetByOrderId(orderId, payment);
        _fileRepositoryMock
            .Setup(x =>
                x.UploadAndStoreRawFileAsync(
                    It.IsAny<IFormFile>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<CancellationToken>()
                )
            )
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
        result.Should().NotBeNull();
        result.Proof.Should().NotBeNull();
        _orderRepositoryMock.VerifyUpdatePaymentCalled();
        _unitOfWorkMock.VerifyCommitCalled();
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
    }

    [Fact]
    public async Task Handle_ShouldExtractMimeTypeBeforeSemicolon()
    {
        // Arrange — verify mimeType is correctly extracted from ContentType
        Guid orderId = Guid.NewGuid();
        ContentPaymentEntity payment = ContentPaymentFactory.Create(orderId);
        FileEntity proofFile = FileFactory.CreateJpeg();

        _orderPaymentFactoryMock.SetupGetByOrderId(orderId, payment);

        string capturedMimeType = string.Empty;
        _fileRepositoryMock
            .Setup(x =>
                x.UploadAndStoreRawFileAsync(
                    It.IsAny<IFormFile>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .Callback<IFormFile, string, string, string, string, CancellationToken>(
                (_, _, _, _, mime, _) => capturedMimeType = mime
            )
            .ReturnsAsync(proofFile);

        Mock<IFormFile> fileMock = new();
        fileMock.Setup(f => f.ContentType).Returns("image/jpeg; charset=utf-8");
        fileMock.Setup(f => f.FileName).Returns("proof.jpg");

        var command = new AdminAttachPaymentProofCommand(
            OrderId: orderId.ToString(),
            File: fileMock.Object,
            PaymentMethod: EnumPaymentMethod.BankTransfer
        );

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        capturedMimeType.Should().Be("image/jpeg");
    }

    #endregion
}
