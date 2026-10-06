using _116.BuildingBlocks.Application.Exceptions;
using _116.Content.Application.Commerce.Services;
using _116.Content.Application.Commerce.UseCases.Admin.Queries.GetOrderPayment;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Content.TestData;
using _116.Content.TestData.Factories;
using _116.Content.TestData.Mocks.Repositories;
using _116.Content.TestData.Mocks.Services;
using _116.Identity.Contracts.Application.DTOs;
using _116.Identity.Contracts.Application.Services;
using _116.Identity.TestData.Mocks.Repositories;
using _116.Identity.TestData.Mocks.Services;
using _116.Shared.Domain.Constants;
using _116.Storage.Contracts.Application.DTOs;
using _116.Storage.Contracts.Application.Services;
using _116.Storage.TestData.Factories;
using _116.Tests.TestData.Constants;
using AwesomeAssertions;
using Moq;
using Xunit;

namespace _116.Content.Unit.Tests.Application.Commerce.UseCases.Admin.Queries.GetOrderPayment;

/// <summary>
/// Unit tests for <see cref="AdminGetOrderPaymentHandler"/>.
/// </summary>
public class AdminGetOrderPaymentHandlerTests : BaseContentHandlerTest
{
    private readonly Mock<IOrderPaymentService> _orderPaymentServiceMock;
    private readonly Mock<IContentOrderRepository> _orderRepositoryMock;
    private readonly Mock<IFileStorageService> _fileStorageMock;
    private readonly Mock<IUserLookupService> _userLookupMock;
    private readonly AdminGetOrderPaymentHandler _handler;

    public AdminGetOrderPaymentHandlerTests()
    {
        _orderPaymentServiceMock = MockOrderPaymentService.Create();
        _orderRepositoryMock = MockContentOrderRepository.Create();
        _fileStorageMock = MockFileStorageService.Create();
        _userLookupMock = MockUserLookupService.Create();
        _handler = new AdminGetOrderPaymentHandler(
            _orderPaymentServiceMock.Object,
            _orderRepositoryMock.Object,
            new PaymentDtoService(Mapper, _userLookupMock.Object, CreateOrderDtoService(), _fileStorageMock.Object)
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
        FileReferenceDto proofFile = FileReferenceDtoFactory.CreateWithId(proofFileId);

        _orderPaymentServiceMock.SetupGetByOrderId(orderId, payment);
        _fileStorageMock.SetupResolve(proofFile);

        var query = new AdminGetOrderPaymentQuery(OrderId: orderId);

        // Act
        AdminGetOrderPaymentResult result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Payment.PaymentProof.Should().NotBeNull();
        result.Payment.PaymentProof!.Id.Should().Be(proofFile.Id);
    }

    [Fact]
    public async Task Handle_WithoutProofFile_ShouldReturnPaymentWithNullProof()
    {
        // Arrange
        Guid orderId = Guid.NewGuid();
        ContentPaymentEntity payment = ContentPaymentFactory.Create(orderId);

        _orderPaymentServiceMock.SetupGetByOrderId(orderId, payment);

        var query = new AdminGetOrderPaymentQuery(OrderId: orderId);

        // Act
        AdminGetOrderPaymentResult result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Payment.PaymentProof.Should().BeNull();

        _fileStorageMock.Verify(x => x.ResolveAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WithVerifiedPayment_ShouldResolveVerifierUserName()
    {
        // Arrange
        Guid orderId = Guid.NewGuid();
        ContentPaymentEntity payment = ContentPaymentFactory.CreateVerified(orderId);
        Guid verifierId = payment.VerifiedById!.Value;

        _orderPaymentServiceMock.SetupGetByOrderId(orderId, payment);
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

        var query = new AdminGetOrderPaymentQuery(OrderId: orderId);

        // Act
        AdminGetOrderPaymentResult result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Payment.VerifiedByUserName.Should().Be(TestConstants.User.ValidUserName);
        _userLookupMock.VerifyGetUserProfilesByIdsCalledOnce();
    }

    [Fact]
    public async Task Handle_WithUnverifiedPayment_ShouldNotCallUserLookup()
    {
        // Arrange
        Guid orderId = Guid.NewGuid();
        ContentPaymentEntity payment = ContentPaymentFactory.Create(orderId);

        _orderPaymentServiceMock.SetupGetByOrderId(orderId, payment);

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
        _orderPaymentServiceMock.Verify(
            x => x.GetByOrderIdOrThrowAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
            Times.Never
        );
    }

    #endregion
}
