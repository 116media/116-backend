using _116.Content.Application.Editorial.UseCases.Admin.Commands.RejectLyrics;
using _116.Content.Application.Shared.Persistence;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Shared.Application.Exceptions;
using _116.Tests.Fixtures.Constants;
using _116.Tests.Fixtures.Factories.Content;
using _116.Tests.Fixtures.Helpers;
using _116.Unit.Tests.Common.Mocks.Infrastructure;
using _116.Unit.Tests.Common.Mocks.Repositories;
using AwesomeAssertions;
using Moq;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Editorial.UseCases.Admin.Commands.RejectLyrics;

/// <summary>
/// Unit tests for <see cref="AdminRejectLyricsHandler"/>.
/// </summary>
public class AdminRejectLyricsHandlerTests
{
    private readonly Mock<ILyricsRepository> _lyricsRepositoryMock;
    private readonly Mock<IContentUnitOfWork> _unitOfWorkMock;
    private readonly AdminRejectLyricsHandler _handler;

    private static readonly Guid CategoryId = Guid.NewGuid();

    public AdminRejectLyricsHandlerTests()
    {
        _lyricsRepositoryMock = MockLyricsRepository.Create();
        _unitOfWorkMock = MockContentUnitOfWork.Create();
        _handler = new AdminRejectLyricsHandler(
            _lyricsRepositoryMock.Object,
            _unitOfWorkMock.Object,
            TestErrorsFactory.CreateContentI18n()
        );
    }

    #region Success Cases

    [Fact]
    public async Task Handle_WhenLyricsInPendingReview_ShouldRejectWithReasonAndReturnSuccess()
    {
        // Arrange
        LyricsEntity lyrics = LyricsFactory.CreatePendingReview(CategoryId);
        var command = new AdminRejectLyricsCommand(
            Id: lyrics.Id.ToString(),
            Reason: TestConstants.Content.Editorial.Lyrics.ValidRejectionReason
        );
        _lyricsRepositoryMock.SetupGetByIdOrThrow(lyrics);

        // Act
        AdminRejectLyricsResult result = await _handler.Handle(command, CancellationToken.None);

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
        var command = new AdminRejectLyricsCommand(
            Id: nonExistentId.ToString(),
            Reason: TestConstants.Content.Editorial.Lyrics.ValidRejectionReason
        );
        _lyricsRepositoryMock.SetupGetByIdOrThrowNotFound(nonExistentId);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_WhenLyricsAlreadyRejected_ShouldThrowConflictException()
    {
        // Arrange
        LyricsEntity lyrics = LyricsFactory.CreateRejected(CategoryId);
        var command = new AdminRejectLyricsCommand(
            Id: lyrics.Id.ToString(),
            Reason: TestConstants.Content.Editorial.Lyrics.ValidRejectionReason
        );
        _lyricsRepositoryMock.SetupGetByIdOrThrow(lyrics);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ConflictException>();
    }

    [Fact]
    public async Task Handle_WhenLyricsInWrongStatus_ShouldThrowBadRequestException()
    {
        // Arrange
        LyricsEntity lyrics = LyricsFactory.Create(CategoryId); // Draft status
        var command = new AdminRejectLyricsCommand(
            Id: lyrics.Id.ToString(),
            Reason: TestConstants.Content.Editorial.Lyrics.ValidRejectionReason
        );
        _lyricsRepositoryMock.SetupGetByIdOrThrow(lyrics);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<BadRequestException>();
    }

    #endregion
}
