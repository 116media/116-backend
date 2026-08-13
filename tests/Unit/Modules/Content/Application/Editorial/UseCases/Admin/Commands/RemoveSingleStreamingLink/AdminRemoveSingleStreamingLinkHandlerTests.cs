using _116.Content.Application.Editorial.UseCases.Admin.Commands.RemoveSingleStreamingLink;
using _116.Content.Application.Shared.Persistence;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;
using _116.Tests.Fixtures.Factories.Content;
using _116.Unit.Tests.Common.Mocks.Infrastructure;
using _116.Unit.Tests.Common.Mocks.Repositories;
using AwesomeAssertions;
using Moq;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Editorial.UseCases.Admin.Commands.RemoveSingleStreamingLink;

/// <summary>
/// Unit tests for <see cref="AdminRemoveSingleStreamingLinkHandler"/>.
/// </summary>
public class AdminRemoveSingleStreamingLinkHandlerTests
{
    private readonly Mock<IStreamingLinkRepository> _streamingLinkRepositoryMock;
    private readonly Mock<IContentUnitOfWork> _unitOfWorkMock;
    private readonly AdminRemoveSingleStreamingLinkHandler _handler;

    public AdminRemoveSingleStreamingLinkHandlerTests()
    {
        _streamingLinkRepositoryMock = MockStreamingLinkRepository.Create();
        _unitOfWorkMock = MockContentUnitOfWork.Create();
        _handler = new AdminRemoveSingleStreamingLinkHandler(
            _streamingLinkRepositoryMock.Object,
            _unitOfWorkMock.Object
        );
    }

    [Fact]
    public async Task Handle_WhenLinkExists_ShouldRemoveAndCommit()
    {
        // Arrange
        Guid lyricsId = Guid.NewGuid();
        StreamingLinkEntity existing = StreamingLinkFactory.CreateForLyrics(lyricsId, EnumStreamingPlatform.AppleMusic);
        _streamingLinkRepositoryMock.SetupGetByLyricsAndPlatformAsync(
            lyricsId,
            EnumStreamingPlatform.AppleMusic,
            existing
        );
        var command = new AdminRemoveSingleStreamingLinkCommand(lyricsId, EnumStreamingPlatform.AppleMusic);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _streamingLinkRepositoryMock.VerifyRemoveCalled();
        _unitOfWorkMock.VerifyCommitCalled();
    }

    [Fact]
    public async Task Handle_WhenNoLinkExists_ShouldBeNoOpAndStillSucceed()
    {
        // Arrange
        Guid lyricsId = Guid.NewGuid();
        var command = new AdminRemoveSingleStreamingLinkCommand(lyricsId, EnumStreamingPlatform.AppleMusic);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _streamingLinkRepositoryMock.Verify(x => x.Remove(It.IsAny<StreamingLinkEntity>()), Times.Never);
        _unitOfWorkMock.Verify(x => x.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}
