using _116.BuildingBlocks.Application.Exceptions;
using _116.Content.Application.Interactions.UseCases.Public.Commands.ShareLyrics;
using _116.Content.Application.Shared.Persistence;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;
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
using _116.Tests.TestData.Mocks;
using AwesomeAssertions;
using Moq;
using Xunit;

namespace _116.Content.Unit.Tests.Application.Interactions.UseCases.Public.Commands.ShareLyrics;

/// <summary>
/// Unit tests for <see cref="PublicShareLyricsHandler"/>.
/// </summary>
public class PublicShareLyricsHandlerTests
{
    private readonly Mock<ILyricsRepository> _lyricsRepositoryMock;
    private readonly Mock<IContentUnitOfWork> _unitOfWorkMock;
    private readonly PublicShareLyricsHandler _handler;

    public PublicShareLyricsHandlerTests()
    {
        _lyricsRepositoryMock = MockLyricsRepository.Create();
        _unitOfWorkMock = MockContentUnitOfWork.Create();
        _handler = new PublicShareLyricsHandler(_lyricsRepositoryMock.Object, _unitOfWorkMock.Object);
    }

    #region Success Cases

    [Fact]
    public async Task Handle_WhenLyricsExistsAndAnonymous_ShouldAddShareAndCommit()
    {
        // Arrange
        LyricsEntity lyrics = LyricsFactory.Create(Guid.NewGuid());

        _lyricsRepositoryMock.SetupGetByIdOrThrow(lyrics);

        var command = new PublicShareLyricsCommand(LyricsId: lyrics.Id, UserId: null);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _lyricsRepositoryMock.VerifyAddShareCalled();
        _unitOfWorkMock.VerifyCommitCalled();
    }

    [Fact]
    public async Task Handle_WhenLyricsExistsAndAuthenticated_ShouldAddShareAndCommit()
    {
        // Arrange
        LyricsEntity lyrics = LyricsFactory.Create(Guid.NewGuid());

        _lyricsRepositoryMock.SetupGetByIdOrThrow(lyrics);

        var command = new PublicShareLyricsCommand(
            LyricsId: lyrics.Id,
            UserId: Guid.NewGuid(),
            ShareChannel: EnumShareChannel.WhatsApp
        );

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _lyricsRepositoryMock.VerifyAddShareCalled();
        _unitOfWorkMock.VerifyCommitCalled();
    }

    #endregion

    #region Failure Cases

    [Fact]
    public async Task Handle_WhenLyricsNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        Guid id = Guid.NewGuid();
        _lyricsRepositoryMock.SetupGetByIdOrThrowNotFound(id);

        var command = new PublicShareLyricsCommand(LyricsId: id, UserId: null);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    #endregion
}
