using _116.Content.Application.Editorial.UseCases.Admin.Commands.UpdateLyrics;
using _116.Content.Application.Shared.Persistence;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Shared.Application.Exceptions;
using _116.Tests.Fixtures.Constants;
using _116.Tests.Fixtures.Factories.Content;
using _116.Unit.Tests.Common;
using _116.Unit.Tests.Common.Mocks.Infrastructure;
using _116.Unit.Tests.Common.Mocks.Repositories;
using AwesomeAssertions;
using Moq;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Editorial.UseCases.Admin.Commands.UpdateLyrics;

/// <summary>
/// Unit tests for <see cref="AdminUpdateLyricsHandler"/>.
/// </summary>
public class AdminUpdateLyricsHandlerTests : BaseContentHandlerTest
{
    private readonly Mock<ILyricsRepository> _lyricsRepositoryMock;
    private readonly Mock<IContentUnitOfWork> _unitOfWorkMock;
    private readonly AdminUpdateLyricsHandler _handler;

    public AdminUpdateLyricsHandlerTests()
    {
        _lyricsRepositoryMock = MockLyricsRepository.Create();
        _unitOfWorkMock = MockContentUnitOfWork.Create();
        _handler = new AdminUpdateLyricsHandler(_lyricsRepositoryMock.Object, _unitOfWorkMock.Object, Mapper);
    }

    [Fact]
    public async Task Handle_WhenLyricsExists_ShouldUpdateAndReturnLyrics()
    {
        // Arrange
        LyricsEntity lyrics = LyricsFactory.Create();
        var command = new AdminUpdateLyricsCommand(
            Id: lyrics.Id.ToString(),
            LyricsText: TestConstants.Content.Editorial.Lyrics.ValidLyricsText
        );

        _lyricsRepositoryMock
            .Setup(x => x.GetByIdOrThrowAsync(lyrics.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(lyrics);

        // Act
        AdminUpdateLyricsResult result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Lyrics.Should().NotBeNull();
        _lyricsRepositoryMock.VerifyUpdateCalled();
        _unitOfWorkMock.VerifyCommitCalled();
    }

    [Fact]
    public async Task Handle_WhenLyricsNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        Guid nonExistentId = Guid.NewGuid();
        var command = new AdminUpdateLyricsCommand(
            Id: nonExistentId.ToString(),
            LyricsText: TestConstants.Content.Editorial.Lyrics.ValidLyricsText
        );
        _lyricsRepositoryMock.SetupGetByIdOrThrowNotFound(nonExistentId);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }
}
