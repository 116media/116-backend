using _116.BuildingBlocks.Constants;
using _116.Content.Application.Commerce.Factories;
using _116.Content.Application.Commerce.UseCases.Admin.Queries.GetOrderPayment;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Core.Application.Shared.Repositories;
using _116.Core.Contracts.Application.DTOs;
using _116.Core.Contracts.Application.Services;
using _116.Core.Domain.Entities;
using _116.Identity.Contracts.Application.DTOs;
using _116.Identity.Contracts.Application.Services;
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
    private readonly Mock<IFileStorageService> _fileStorageMock;
    private readonly Mock<IUserLookupService> _userLookupMock;
    private readonly AdminGetOrderPaymentHandler _handler;

    public AdminGetOrderPaymentHandlerTests()
    {
        _orderPaymentFactoryMock = MockOrderPaymentFactory.Create();
        _orderRepositoryMock = MockContentOrderRepository.Create();
        _fileStorageMock = MockFileStorageService.Create();
        _userLookupMock = MockUserLookupService.Create();
        _handler = new AdminGetOrderPaymentHandler(
            _orderPaymentFactoryMock.Object,
            _orderRepositoryMock.Object,
            _fileStorageMock.Object,
            Mapper,
            new PaymentDtoFactory(Mapper, _userLookupMock.Object, CreateOrderDtoFactory())
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

        _orderPaymentFactoryMock.SetupGetByOrderId(orderId, payment);
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

        _orderPaymentFactoryMock.SetupGetByOrderId(orderId, payment);

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

        _orderPaymentFactoryMock.SetupGetByOrderId(orderId, payment);
        _userLookupMock.SetupGetAuthorInfosByIds(
            new Dictionary<Guid, AuthorDto>
            {
                [verifierId] = new(
                    TestConstants.User.ValidUserName,
                    Email: null,
                    AvatarFileId: null,
                    Role: null,
                    PreferredLocale: UserConstants.DefaultLocale
                ),
            }
        );

        var query = new AdminGetOrderPaymentQuery(OrderId: orderId);

        // Act
        AdminGetOrderPaymentResult result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Payment.VerifiedByUserName.Should().Be(TestConstants.User.ValidUserName);
        _userLookupMock.VerifyGetAuthorInfosByIdsCalledOnce();
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
