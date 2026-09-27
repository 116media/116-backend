using _116.BuildingBlocks.Application.Exceptions;
using _116.Content.Application.Editorial.UseCases.Admin.Commands.SubmitVideo;
using _116.Content.Application.Shared.Persistence;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;
using _116.Content.TestData.Builders.Entities;
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
using _116.Storage.TestData.Factories;
using _116.Storage.TestData.Mocks.Infrastructure;
using _116.Storage.TestData.Mocks.Services;
using _116.Tests.TestData.Helpers;
using _116.Tests.TestData.Mocks;
using AwesomeAssertions;
using Moq;
using Xunit;

namespace _116.Content.Unit.Tests.Application.Editorial.UseCases.Admin.Commands.SubmitVideo;

/// <summary>
/// Unit tests for <see cref="AdminSubmitVideoHandler"/>.
/// </summary>
public class AdminSubmitVideoHandlerTests
{
    private readonly Mock<IVideoRepository> _videoRepositoryMock;
    private readonly Mock<IContentOrderRepository> _orderRepositoryMock;
    private readonly Mock<IContentUnitOfWork> _unitOfWorkMock;
    private readonly AdminSubmitVideoHandler _handler;

    private static readonly Guid CategoryId = Guid.NewGuid();

    public AdminSubmitVideoHandlerTests()
    {
        _videoRepositoryMock = MockVideoRepository.Create();
        _orderRepositoryMock = MockContentOrderRepository.Create();
        _unitOfWorkMock = MockContentUnitOfWork.Create();
        _handler = new AdminSubmitVideoHandler(
            _videoRepositoryMock.Object,
            _orderRepositoryMock.Object,
            _unitOfWorkMock.Object,
            TestErrorsFactory.CreateContentI18n()
        );
    }

    #region Success Cases

    [Fact]
    public async Task Handle_WhenFreeVideoInDraft_ShouldTransitionToPendingReview()
    {
        // Arrange
        VideoEntity video = VideoFactory.Create(CategoryId);
        var command = new AdminSubmitVideoCommand(Id: video.Id.ToString());
        _videoRepositoryMock.SetupGetByIdOrThrow(video);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        video.Status.Should().Be(EnumContentStatus.PendingReview);
        _unitOfWorkMock.VerifyCommitCalled();
    }

    [Fact]
    public async Task Handle_WhenPaidVideoInDraftWithUnpaidOrder_ShouldTransitionToPendingPayment()
    {
        // Arrange
        Guid customerId = Guid.NewGuid();
        Guid orderItemId = Guid.NewGuid();
        VideoEntity video = VideoFactory.CreatePaid(CategoryId, customerId, orderItemId);
        ContentOrderEntity order = ContentOrderFactory.CreateSubmitted();
        var command = new AdminSubmitVideoCommand(Id: video.Id.ToString());
        _videoRepositoryMock.SetupGetByIdOrThrow(video);
        _orderRepositoryMock.SetupGetOrderByItemId(orderItemId, order);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        video.Status.Should().Be(EnumContentStatus.PendingPayment);
        _unitOfWorkMock.VerifyCommitCalled();
    }

    [Fact]
    public async Task Handle_WhenPaidVideoInDraftWithAlreadyPaidOrder_ShouldTransitionToPendingReview()
    {
        // Arrange
        Guid customerId = Guid.NewGuid();
        Guid orderItemId = Guid.NewGuid();
        VideoEntity video = VideoFactory.CreatePaid(CategoryId, customerId, orderItemId);
        ContentOrderEntity order = ContentOrderFactory.CreatePaid();
        var command = new AdminSubmitVideoCommand(Id: video.Id.ToString());
        _videoRepositoryMock.SetupGetByIdOrThrow(video);
        _orderRepositoryMock.SetupGetOrderByItemId(orderItemId, order);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        video.Status.Should().Be(EnumContentStatus.PendingReview);
        _unitOfWorkMock.VerifyCommitCalled();
    }

    #endregion

    #region Failure Cases

    [Fact]
    public async Task Handle_WhenVideoNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        Guid nonExistentId = Guid.NewGuid();
        var command = new AdminSubmitVideoCommand(Id: nonExistentId.ToString());
        _videoRepositoryMock.SetupGetByIdOrThrowNotFound(nonExistentId);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
        _unitOfWorkMock.VerifyCommitNotCalled();
    }

    [Fact]
    public async Task Handle_WhenFreeVideoAlreadyPendingReview_ShouldThrowConflictException()
    {
        // Arrange
        VideoEntity video = VideoFactory.CreatePendingReview(CategoryId);
        var command = new AdminSubmitVideoCommand(Id: video.Id.ToString());
        _videoRepositoryMock.SetupGetByIdOrThrow(video);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ConflictException>();
        video.Status.Should().Be(EnumContentStatus.PendingReview);
        _unitOfWorkMock.VerifyCommitNotCalled();
    }

    [Fact]
    public async Task Handle_WhenPaidVideoAlreadyPendingPayment_ShouldThrowConflictException()
    {
        // Arrange
        Guid customerId = Guid.NewGuid();
        Guid orderItemId = Guid.NewGuid();
        VideoEntity video = new VideoBuilder(CategoryId)
            .WithCustomer(customerId, orderItemId)
            .AsPendingPayment()
            .Build();
        ContentOrderEntity order = ContentOrderFactory.CreateSubmitted();
        var command = new AdminSubmitVideoCommand(Id: video.Id.ToString());
        _videoRepositoryMock.SetupGetByIdOrThrow(video);
        _orderRepositoryMock.SetupGetOrderByItemId(orderItemId, order);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ConflictException>();
        video.Status.Should().Be(EnumContentStatus.PendingPayment);
        _unitOfWorkMock.VerifyCommitNotCalled();
    }

    #endregion
}
