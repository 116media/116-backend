using _116.Content.Application.Editorial.UseCases.Admin.Commands.PublishLyrics;
using _116.Content.Application.Shared.Persistence;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;
using _116.Content.Domain.Events;
using _116.Shared.Application.Exceptions;
using _116.Shared.Domain.Exceptions;
using _116.Tests.Fixtures.Factories.Content;
using _116.Tests.Fixtures.Helpers;
using _116.Unit.Tests.Common.Mocks.Infrastructure;
using _116.Unit.Tests.Common.Mocks.Repositories;
using AwesomeAssertions;
using Moq;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Editorial.UseCases.Admin.Commands.PublishLyrics;

/// <summary>
/// Unit tests for <see cref="AdminPublishLyricsHandler"/>.
/// </summary>
public class AdminPublishLyricsHandlerTests
{
    private readonly Mock<ILyricsRepository> _lyricsRepositoryMock;
    private readonly Mock<IContentUnitOfWork> _unitOfWorkMock;
    private readonly AdminPublishLyricsHandler _handler;

    private static readonly Guid CategoryId = Guid.NewGuid();

    public AdminPublishLyricsHandlerTests()
    {
        _lyricsRepositoryMock = MockLyricsRepository.Create();
        _unitOfWorkMock = MockContentUnitOfWork.Create();
        _handler = new AdminPublishLyricsHandler(
            _lyricsRepositoryMock.Object,
            _unitOfWorkMock.Object,
            TestErrorsFactory.CreateContentI18n()
        );
    }

    #region Success Cases

    [Fact]
    public async Task Handle_WhenLyricsIsApproved_ShouldTransitionToPublished()
    {
        // Arrange
        LyricsEntity lyrics = LyricsFactory.CreateApproved(CategoryId);
        var command = new AdminPublishLyricsCommand(Id: lyrics.Id.ToString());
        _lyricsRepositoryMock.SetupGetByIdOrThrow(lyrics);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        lyrics.Status.Should().Be(EnumContentStatus.Published);
        lyrics.PublishedAt.Should().NotBeNull();
        _lyricsRepositoryMock.VerifyUpdateCalled(lyrics);
        _unitOfWorkMock.VerifyCommitCalled();
    }

    [Fact]
    public async Task Handle_WhenLyricsIsApproved_ShouldRaiseCommissionedContentPublishedEvent()
    {
        // Arrange
        LyricsEntity lyrics = LyricsFactory.CreateApproved(CategoryId);
        lyrics.ClearDomainEvents();
        var command = new AdminPublishLyricsCommand(Id: lyrics.Id.ToString());
        _lyricsRepositoryMock.SetupGetByIdOrThrow(lyrics);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        lyrics
            .DomainEvents.OfType<CommissionedContentPublishedEvent>()
            .Should()
            .ContainSingle()
            .Which.Should()
            .Be(
                new CommissionedContentPublishedEvent(
                    ContentId: lyrics.Id,
                    ContentType: EnumCoreContentType.Lyrics,
                    CustomerId: lyrics.CustomerId,
                    Title: lyrics.SongTitle,
                    Slug: lyrics.Slug
                )
            );
    }

    #endregion

    #region Failure Cases

    [Fact]
    public async Task Handle_WhenLyricsNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        Guid nonExistentId = Guid.NewGuid();
        var command = new AdminPublishLyricsCommand(Id: nonExistentId.ToString());
        _lyricsRepositoryMock.SetupGetByIdOrThrowNotFound(nonExistentId);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
        _unitOfWorkMock.VerifyCommitNotCalled();
    }

    [Fact]
    public async Task Handle_WhenLyricsAlreadyPublished_ShouldThrowConflictException()
    {
        // Arrange
        LyricsEntity lyrics = LyricsFactory.CreatePublished(CategoryId);
        lyrics.ClearDomainEvents();
        var command = new AdminPublishLyricsCommand(Id: lyrics.Id.ToString());
        _lyricsRepositoryMock.SetupGetByIdOrThrow(lyrics);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ConflictException>();
        lyrics.Status.Should().Be(EnumContentStatus.Published);
        lyrics.DomainEvents.Should().BeEmpty();
        _unitOfWorkMock.VerifyCommitNotCalled();
    }

    [Fact]
    public async Task Handle_WhenLyricsInWrongStatus_ShouldThrowDomainRuleException()
    {
        // Arrange
        LyricsEntity lyrics = LyricsFactory.Create(CategoryId);
        lyrics.ClearDomainEvents();
        var command = new AdminPublishLyricsCommand(Id: lyrics.Id.ToString());
        _lyricsRepositoryMock.SetupGetByIdOrThrow(lyrics);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<DomainRuleException>();
        lyrics.Status.Should().Be(EnumContentStatus.Draft);
        lyrics.PublishedAt.Should().BeNull();
        lyrics.DomainEvents.Should().BeEmpty();
        _unitOfWorkMock.VerifyCommitNotCalled();
    }

    #endregion
}
