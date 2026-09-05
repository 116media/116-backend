using _116.Content.Application.Editorial.UseCases.Admin.Commands.ForceUnpromoteLyrics;
using _116.Content.Application.Shared.Persistence;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;
using _116.Content.Domain.Events;
using _116.Content.Domain.Exceptions;
using _116.Content.Domain.StateMachines;
using _116.Shared.Application.Exceptions;
using _116.Shared.Application.Services;
using _116.Tests.Fixtures.Factories.Content;
using _116.Tests.Fixtures.Helpers;
using _116.Unit.Tests.Common.Mocks.Infrastructure;
using _116.Unit.Tests.Common.Mocks.Repositories;
using AwesomeAssertions;
using Moq;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Editorial.UseCases.Admin.Commands.ForceUnpromoteLyrics;

/// <summary>
/// Unit tests for <see cref="AdminForceUnpromoteLyricsHandler"/>.
/// </summary>
public class AdminForceUnpromoteLyricsHandlerTests
{
    private readonly Mock<ILyricsRepository> _lyricsRepositoryMock;
    private readonly Mock<IContentUnitOfWork> _unitOfWorkMock;
    private readonly AdminForceUnpromoteLyricsHandler _handler;

    private static readonly Guid CategoryId = Guid.NewGuid();
    private static readonly string ActorUserId = Guid.NewGuid().ToString();

    public AdminForceUnpromoteLyricsHandlerTests()
    {
        _lyricsRepositoryMock = MockLyricsRepository.Create();
        _unitOfWorkMock = MockContentUnitOfWork.Create();

        ICurrentActor currentActor = Mock.Of<ICurrentActor>(a =>
            a.UserId == ActorUserId && a.IsAuthenticated == true && a.HasHttpContext == true
        );

        _handler = new AdminForceUnpromoteLyricsHandler(
            _lyricsRepositoryMock.Object,
            _unitOfWorkMock.Object,
            currentActor
        );
    }

    #region Success Cases

    [Fact]
    public async Task Handle_WhenLyricsIsPromoted_ShouldUnpromoteAndReturnResult()
    {
        // Arrange
        LyricsEntity lyrics = LyricsFactory.CreatePromoted(CategoryId);
        _lyricsRepositoryMock.SetupGetByIdOrThrow(lyrics);
        var command = new AdminForceUnpromoteLyricsCommand(lyrics.Id, "Government takedown request.");

        // Act
        AdminForceUnpromoteLyricsResult result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.LyricsId.Should().Be(lyrics.Id);
        result.UnpromotedAt.Should().Be(lyrics.UnpromotedAt!.Value);
        lyrics.IsPromoted.Should().BeFalse();
        lyrics.PromotedUntil.Should().BeNull();
        lyrics.UnpromotedAt.Should().NotBeNull();
        lyrics.UnpromotedBy.Should().Be(ActorUserId);
        lyrics.UnpromotedReason.Should().Be("Government takedown request.");
        _lyricsRepositoryMock.VerifyUpdateCalled(lyrics);
        _unitOfWorkMock.VerifyCommitCalled();
    }

    [Fact]
    public async Task Handle_WhenLyricsIsPromoted_ShouldRaiseContentPromotionRemovedEvent()
    {
        // Arrange
        LyricsEntity lyrics = LyricsFactory.CreatePromoted(CategoryId);
        lyrics.ClearDomainEvents();
        _lyricsRepositoryMock.SetupGetByIdOrThrow(lyrics);
        var command = new AdminForceUnpromoteLyricsCommand(lyrics.Id, "Government takedown request.");

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        lyrics
            .DomainEvents.OfType<ContentPromotionRemovedEvent>()
            .Should()
            .ContainSingle()
            .Which.Should()
            .Be(
                new ContentPromotionRemovedEvent(
                    ContentId: lyrics.Id,
                    ContentType: EnumCoreContentType.Lyrics,
                    CustomerId: lyrics.CustomerId,
                    Title: lyrics.SongTitle,
                    Reason: "Government takedown request."
                )
            );
    }

    #endregion

    #region Failure Cases

    [Fact]
    public async Task Handle_WhenLyricsNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        Guid lyricsId = Guid.NewGuid();
        _lyricsRepositoryMock.SetupGetByIdOrThrowNotFound(lyricsId);
        var command = new AdminForceUnpromoteLyricsCommand(lyricsId, "Government takedown request.");

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
        _unitOfWorkMock.VerifyCommitNotCalled();
    }

    [Fact]
    public async Task Handle_WhenLyricsNotCurrentlyPromoted_ShouldThrowBadRequestException()
    {
        // Arrange
        LyricsEntity lyrics = LyricsFactory.CreatePublished(CategoryId);
        lyrics.ClearDomainEvents();
        _lyricsRepositoryMock.SetupGetByIdOrThrow(lyrics);
        var command = new AdminForceUnpromoteLyricsCommand(lyrics.Id, "Government takedown request.");

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        (await act.Should().ThrowAsync<ContentRuleException>())
            .Which.Code.Should()
            .Be(ContentRuleCodes.LyricsNotPromoted);
        lyrics.IsPromoted.Should().BeFalse();
        lyrics.UnpromotedAt.Should().BeNull();
        lyrics.UnpromotedBy.Should().BeNull();
        lyrics.UnpromotedReason.Should().BeNull();
        lyrics.DomainEvents.Should().BeEmpty();
        _unitOfWorkMock.VerifyCommitNotCalled();
    }

    #endregion
}
