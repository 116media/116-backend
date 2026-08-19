using _116.Content.Application.Editorial.UseCases.Admin.Commands.ApproveLyrics;
using _116.Content.Application.Shared.Persistence;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;
using _116.Shared.Application.Exceptions;
using _116.Shared.Domain.Exceptions;
using _116.Tests.Fixtures.Factories.Content;
using _116.Tests.Fixtures.Helpers;
using _116.Unit.Tests.Common.Mocks.Infrastructure;
using _116.Unit.Tests.Common.Mocks.Repositories;
using AwesomeAssertions;
using Moq;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Editorial.UseCases.Admin.Commands.ApproveLyrics;

/// <summary>
/// Unit tests for <see cref="AdminApproveLyricsHandler"/>.
/// </summary>
public class AdminApproveLyricsHandlerTests
{
    private readonly Mock<ILyricsRepository> _lyricsRepositoryMock;
    private readonly Mock<IContentUnitOfWork> _unitOfWorkMock;
    private readonly AdminApproveLyricsHandler _handler;

    private static readonly Guid CategoryId = Guid.NewGuid();

    public AdminApproveLyricsHandlerTests()
    {
        _lyricsRepositoryMock = MockLyricsRepository.Create();
        _unitOfWorkMock = MockContentUnitOfWork.Create();
        _handler = new AdminApproveLyricsHandler(
            _lyricsRepositoryMock.Object,
            _unitOfWorkMock.Object,
            TestErrorsFactory.CreateContentI18n()
        );
    }

    #region Success Cases

    [Fact]
    public async Task Handle_WhenLyricsInPendingReview_ShouldTransitionToApproved()
    {
        // Arrange
        LyricsEntity lyrics = LyricsFactory.CreatePendingReview(CategoryId);
        var command = new AdminApproveLyricsCommand(Id: lyrics.Id.ToString());
        _lyricsRepositoryMock.SetupGetByIdOrThrow(lyrics);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        lyrics.Status.Should().Be(EnumContentStatus.Approved);
        _lyricsRepositoryMock.VerifyUpdateCalled(lyrics);
        _unitOfWorkMock.VerifyCommitCalled();
    }

    #endregion

    #region Failure Cases

    [Fact]
    public async Task Handle_WhenLyricsNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        Guid nonExistentId = Guid.NewGuid();
        var command = new AdminApproveLyricsCommand(Id: nonExistentId.ToString());
        _lyricsRepositoryMock.SetupGetByIdOrThrowNotFound(nonExistentId);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
        _unitOfWorkMock.VerifyCommitNotCalled();
    }

    [Fact]
    public async Task Handle_WhenLyricsAlreadyApproved_ShouldThrowConflictException()
    {
        // Arrange
        LyricsEntity lyrics = LyricsFactory.CreateApproved(CategoryId);
        var command = new AdminApproveLyricsCommand(Id: lyrics.Id.ToString());
        _lyricsRepositoryMock.SetupGetByIdOrThrow(lyrics);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ConflictException>();
        lyrics.Status.Should().Be(EnumContentStatus.Approved);
        _unitOfWorkMock.VerifyCommitNotCalled();
    }

    [Fact]
    public async Task Handle_WhenLyricsInWrongStatus_ShouldThrowDomainRuleException()
    {
        // Arrange
        LyricsEntity lyrics = LyricsFactory.Create(CategoryId);
        var command = new AdminApproveLyricsCommand(Id: lyrics.Id.ToString());
        _lyricsRepositoryMock.SetupGetByIdOrThrow(lyrics);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<DomainRuleException>();
        lyrics.Status.Should().Be(EnumContentStatus.Draft);
        _unitOfWorkMock.VerifyCommitNotCalled();
    }

    #endregion
}
