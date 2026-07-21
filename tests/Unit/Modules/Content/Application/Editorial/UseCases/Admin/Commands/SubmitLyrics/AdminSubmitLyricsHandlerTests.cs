using _116.Content.Application.Editorial.UseCases.Admin.Commands.SubmitLyrics;
using _116.Content.Application.Shared.Persistence;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Shared.Application.Exceptions;
using _116.Tests.Fixtures.Factories.Content;
using _116.Tests.Fixtures.Helpers;
using _116.Unit.Tests.Common.Mocks.Infrastructure;
using _116.Unit.Tests.Common.Mocks.Repositories;
using AwesomeAssertions;
using Moq;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Editorial.UseCases.Admin.Commands.SubmitLyrics;

/// <summary>
/// Unit tests for <see cref="AdminSubmitLyricsHandler"/>.
/// </summary>
public class AdminSubmitLyricsHandlerTests
{
    private readonly Mock<ILyricsRepository> _lyricsRepositoryMock;
    private readonly Mock<IContentOrderRepository> _orderRepositoryMock;
    private readonly Mock<IContentUnitOfWork> _unitOfWorkMock;
    private readonly AdminSubmitLyricsHandler _handler;

    private static readonly Guid CategoryId = Guid.NewGuid();

    public AdminSubmitLyricsHandlerTests()
    {
        _lyricsRepositoryMock = MockLyricsRepository.Create();
        _orderRepositoryMock = MockContentOrderRepository.Create();
        _unitOfWorkMock = MockContentUnitOfWork.Create();
        _handler = new AdminSubmitLyricsHandler(
            _lyricsRepositoryMock.Object,
            _orderRepositoryMock.Object,
            _unitOfWorkMock.Object,
            TestErrorsFactory.CreateContentI18n()
        );
    }

    #region Success Cases

    [Fact]
    public async Task Handle_WhenFreeLyricsInDraft_ShouldTransitionToPendingReview()
    {
        // Arrange
        LyricsEntity lyrics = LyricsFactory.Create(CategoryId);
        var command = new AdminSubmitLyricsCommand(Id: lyrics.Id.ToString());
        _lyricsRepositoryMock.SetupGetByIdOrThrow(lyrics);

        // Act
        AdminSubmitLyricsResult result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _lyricsRepositoryMock.VerifyUpdateCalled();
        _unitOfWorkMock.VerifyCommitCalled();
    }

    [Fact]
    public async Task Handle_WhenPaidLyricsInDraftWithUnpaidOrder_ShouldTransitionToPendingPayment()
    {
        // Arrange
        Guid customerId = Guid.NewGuid();
        Guid orderItemId = Guid.NewGuid();
        LyricsEntity lyrics = LyricsFactory.CreatePaid(CategoryId, customerId, orderItemId);
        ContentOrderEntity order = ContentOrderFactory.CreateSubmitted();
        var command = new AdminSubmitLyricsCommand(Id: lyrics.Id.ToString());
        _lyricsRepositoryMock.SetupGetByIdOrThrow(lyrics);
        _orderRepositoryMock.SetupGetOrderByItemId(orderItemId, order);

        // Act
        AdminSubmitLyricsResult result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _lyricsRepositoryMock.VerifyUpdateCalled();
        _unitOfWorkMock.VerifyCommitCalled();
    }

    [Fact]
    public async Task Handle_WhenPaidLyricsInDraftWithAlreadyPaidOrder_ShouldTransitionToPendingReview()
    {
        // Arrange
        Guid customerId = Guid.NewGuid();
        Guid orderItemId = Guid.NewGuid();
        LyricsEntity lyrics = LyricsFactory.CreatePaid(CategoryId, customerId, orderItemId);
        ContentOrderEntity order = ContentOrderFactory.CreatePaid();
        var command = new AdminSubmitLyricsCommand(Id: lyrics.Id.ToString());
        _lyricsRepositoryMock.SetupGetByIdOrThrow(lyrics);
        _orderRepositoryMock.SetupGetOrderByItemId(orderItemId, order);

        // Act
        AdminSubmitLyricsResult result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _lyricsRepositoryMock.VerifyUpdateCalled();
        _unitOfWorkMock.VerifyCommitCalled();
    }

    #endregion

    #region Failure Cases

    [Fact]
    public async Task Handle_WhenLyricsNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        Guid nonExistentId = Guid.NewGuid();
        var command = new AdminSubmitLyricsCommand(Id: nonExistentId.ToString());
        _lyricsRepositoryMock.SetupGetByIdOrThrowNotFound(nonExistentId);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_WhenFreeLyricsAlreadyPendingReview_ShouldThrowConflictException()
    {
        // Arrange
        LyricsEntity lyrics = LyricsFactory.CreatePendingReview(CategoryId);
        var command = new AdminSubmitLyricsCommand(Id: lyrics.Id.ToString());
        _lyricsRepositoryMock.SetupGetByIdOrThrow(lyrics);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ConflictException>();
    }

    [Fact]
    public async Task Handle_WhenPaidLyricsAlreadyPendingPayment_ShouldThrowConflictException()
    {
        // Arrange
        Guid customerId = Guid.NewGuid();
        Guid orderItemId = Guid.NewGuid();
        LyricsEntity lyrics = LyricsFactory.CreatePendingPayment(CategoryId, customerId, orderItemId);
        ContentOrderEntity order = ContentOrderFactory.CreateSubmitted();
        var command = new AdminSubmitLyricsCommand(Id: lyrics.Id.ToString());
        _lyricsRepositoryMock.SetupGetByIdOrThrow(lyrics);
        _orderRepositoryMock.SetupGetOrderByItemId(orderItemId, order);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ConflictException>();
    }

    #endregion
}
