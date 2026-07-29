using _116.Content.Application.Commerce.Services;
using _116.Content.Application.Editorial.UseCases.Admin.Commands.ForceUnpromoteLyrics;
using _116.Content.Application.Shared.Persistence;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Shared.Application.Exceptions;
using _116.Shared.Application.Services;
using _116.Tests.Fixtures.Factories.Content;
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
            currentActor,
            new Mock<ICommerceCustomerNotifier>().Object
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
        result.UnpromotedAt.Should().NotBe(default);
        lyrics.IsPromoted.Should().BeFalse();
        lyrics.UnpromotedBy.Should().Be(ActorUserId);
        _lyricsRepositoryMock.VerifyUpdateCalled();
        _unitOfWorkMock.VerifyCommitCalled();
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
    }

    /// <summary>
    /// Exercises the <c>!IsPromoted</c> guard in <c>LyricsEntity.ForceUnpromote</c> — an
    /// unpromoted lyrics page cannot be force-unpromoted again.
    /// </summary>
    [Fact]
    public async Task Handle_WhenLyricsNotCurrentlyPromoted_ShouldThrowBadRequestException()
    {
        // Arrange
        LyricsEntity lyrics = LyricsFactory.CreatePublished(CategoryId);
        _lyricsRepositoryMock.SetupGetByIdOrThrow(lyrics);
        var command = new AdminForceUnpromoteLyricsCommand(lyrics.Id, "Government takedown request.");

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<BadRequestException>();
    }

    #endregion
}
